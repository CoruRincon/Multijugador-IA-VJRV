using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// Opción B: laberinto 3D con altura de muros por Perlin Noise (2.1)
/// y A* entre dos casillas seleccionables con el ratón (2.2).
/// Uso: añade este script a un GameObject vacío y dale Play.
/// Controles: clic izquierdo = origen, clic derecho = destino.
public class LaberintoAStar : MonoBehaviour {
    [Header("Laberinto")]
    public int ancho = 21;              // usar valores impares
    public int alto = 21;
    public float celda = 1f;
    public int semilla = 0;             // 0 = aleatoria
    [Range(0f, 0.2f)] public float huecosExtra = 0.06f; // crea ciclos => varias rutas

    [Header("Perlin Noise (2.1)")]
    public float escalaRuido = 0.12f;
    public float alturaMin = 1f;
    public float alturaMax = 5f;

    [Header("Materiales")]
    public Material materialSuelo;
    [Tooltip("Del más bajo al más alto. El rango alturaMin..alturaMax se divide en tramos iguales, uno por material.")]
    public Material[] materialesMuro;

    [Header("Input System")]
    [Tooltip("Arrastra aquí InputSystem_Actions. Si lo dejas vacío se usan las Project-wide Actions.")]
    public InputActionAsset acciones;

    [Header("A*")]
    public bool permitirDiagonales = false; // false: 4 vecinos, coste 10. true: 8 vecinos, 10/14

    bool[,] libre;                       // true = transitable
    Vector3 origenMundo;
    LineRenderer linea;
    GameObject marcaInicio, marcaMeta;
    Vector2Int? inicio, meta;
    InputAction accionPunto, accionClickIzq, accionClickDer;

    // ---------- Nodo con g, h, f ----------
    class Nodo {
        public Vector2Int pos;
        public int g, h;
        public int f => g + h;
        public Nodo padre;
        public Nodo(Vector2Int p) { pos = p; }
    }

    void Start() {
        ConfigurarInput();
        if (semilla != 0) Random.InitState(semilla);
        origenMundo = transform.position;
        GenerarMapa();
        ConstruirEscena3D();
        PrepararVisualizacion();
        ConfigurarCamara();
    }

    // ================= Generación del mapa =================
    void GenerarMapa() {
        libre = new bool[ancho, alto];        // todo muro (false)
        // DFS iterativo (recursive backtracker) sobre celdas impares
        var pila = new Stack<Vector2Int>();
        var ini = new Vector2Int(1, 1);
        libre[ini.x, ini.y] = true;
        pila.Push(ini);
        var dirs = new[] { new Vector2Int(2, 0), new Vector2Int(-2, 0), new Vector2Int(0, 2), new Vector2Int(0, -2) };

        while (pila.Count > 0) {
            var c = pila.Peek();
            var vecinos = new List<Vector2Int>();
            foreach (var d in dirs) {
                var n = c + d;
                if (n.x > 0 && n.y > 0 && n.x < ancho - 1 && n.y < alto - 1 && !libre[n.x, n.y])
                    vecinos.Add(d);
            }
            if (vecinos.Count == 0) { pila.Pop(); continue; }
            var el = vecinos[Random.Range(0, vecinos.Count)];
            libre[c.x + el.x / 2, c.y + el.y / 2] = true;   // abre el muro intermedio
            libre[c.x + el.x, c.y + el.y] = true;
            pila.Push(c + el);
        }

        // Huecos extra para que existan varias rutas y A* tenga sentido
        for (int x = 1; x < ancho - 1; x++)
            for (int z = 1; z < alto - 1; z++)
                if (!libre[x, z] && Random.value < huecosExtra) libre[x, z] = true;
    }

    // ================= 2.1 Perlin Noise =================
    float AlturaMuro(int x, int z) {
        float ruido = Mathf.PerlinNoise(x * escalaRuido + 100f, z * escalaRuido + 100f); // 0..1
        return alturaMin + (alturaMax - alturaMin) * ruido;   // ruido -> altura
    }

    // Elige el material según la altura (tramos iguales entre alturaMin y alturaMax)
    void AplicarMaterialMuro(Renderer r, float altura) {
        float t = Mathf.InverseLerp(alturaMin, alturaMax, altura);   // 0..1
        if (materialesMuro != null && materialesMuro.Length > 0) {
            int i = Mathf.Min((int)(t * materialesMuro.Length), materialesMuro.Length - 1);
            if (materialesMuro[i] != null) { r.sharedMaterial = materialesMuro[i]; return; }
        }
        // Sin materiales asignados: degradado de color según altura
        r.material.color = Color.Lerp(new Color(0.35f, 0.55f, 0.8f), new Color(0.15f, 0.2f, 0.45f), t);
    }

    Vector3 Centro(int x, int z) => origenMundo + new Vector3(x * celda, 0f, z * celda);

    void ConstruirEscena3D() {
        // Suelo
        var suelo = GameObject.CreatePrimitive(PrimitiveType.Cube);
        suelo.name = "Suelo";
        suelo.transform.SetParent(transform);
        suelo.transform.localScale = new Vector3(ancho * celda, 0.1f, alto * celda);
        suelo.transform.position = Centro(ancho / 2, alto / 2) + Vector3.down * 0.05f;
        var rendSuelo = suelo.GetComponent<Renderer>();
        if (materialSuelo != null) rendSuelo.sharedMaterial = materialSuelo;
        else rendSuelo.material.color = new Color(0.85f, 0.85f, 0.8f);

        var padre = new GameObject("Muros").transform;
        padre.SetParent(transform);

        for (int x = 0; x < ancho; x++)
            for (int z = 0; z < alto; z++) {
                if (libre[x, z]) continue;
                float h = AlturaMuro(x, z);
                var muro = GameObject.CreatePrimitive(PrimitiveType.Cube);
                muro.name = $"Muro_{x}_{z}";
                muro.transform.SetParent(padre);
                muro.transform.localScale = new Vector3(celda, h, celda);
                muro.transform.position = Centro(x, z) + Vector3.up * (h / 2f);
                AplicarMaterialMuro(muro.GetComponent<Renderer>(), h);
            }
    }

    // ================= Visualización =================
    void PrepararVisualizacion() {
        linea = gameObject.AddComponent<LineRenderer>();
        linea.useWorldSpace = true;
        linea.startWidth = linea.endWidth = celda * 0.3f;
        linea.material = new Material(Shader.Find("Sprites/Default"));
        linea.startColor = linea.endColor = Color.red;
        linea.positionCount = 0;

        marcaInicio = CrearMarca(Color.green);
        marcaMeta = CrearMarca(Color.yellow);
    }

    GameObject CrearMarca(Color c) {
        var m = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        m.transform.localScale = Vector3.one * celda * 0.6f;
        m.GetComponent<Renderer>().material.color = c;
        Destroy(m.GetComponent<Collider>());
        m.SetActive(false);
        return m;
    }

    void ConfigurarCamara() {
        var cam = Camera.main;
        if (cam == null) return;
        if(ancho > alto)
            cam.orthographicSize = ancho / 2 + 5;
        else
            cam.orthographicSize = alto / 2 + 5; 
        var centro = Centro(ancho / 2, alto / 2);
        cam.transform.position = centro + new Vector3(0, 50f, 0);
        cam.transform.LookAt(centro);
    }

    // ================= 2.2 Selección de casillas =================
    void ConfigurarInput() {
        if (acciones == null) acciones = InputSystem.actions;   // Project-wide Actions
        if (acciones == null) {
            Debug.LogError("Asigna InputSystem_Actions en el Inspector.");
            enabled = false;
            return;
        }
        accionPunto = acciones.FindAction("UI/Point", true);
        accionClickIzq = acciones.FindAction("UI/Click", true);
        accionClickDer = acciones.FindAction("UI/RightClick", true);
        acciones.Enable();   // inofensivo si ya estaban activadas
    }

    void Update() {
        if (accionClickIzq.WasPressedThisFrame()) Seleccionar(true);
        if (accionClickDer.WasPressedThisFrame()) Seleccionar(false);
    }

    void Seleccionar(bool esInicio) {
        Ray rayo = Camera.main.ScreenPointToRay(accionPunto.ReadValue<Vector2>());
        if (!Physics.Raycast(rayo, out RaycastHit hit)) return;

        // Mismo origen y tamaño de celda que en Centro() (conversión mundo -> grid)
        Vector3 p = hit.point - hit.normal * 0.01f - origenMundo;
        int x = Mathf.RoundToInt(p.x / celda);
        int z = Mathf.RoundToInt(p.z / celda);

        if (x < 0 || z < 0 || x >= ancho || z >= alto) return;          // límites
        if (!libre[x, z]) { Debug.Log("Casilla no transitable"); return; } // transitabilidad

        var celdaSel = new Vector2Int(x, z);
        if (esInicio) { inicio = celdaSel; marcaInicio.SetActive(true); marcaInicio.transform.position = Centro(x, z) + Vector3.up * 0.3f; }
        else { meta = celdaSel; marcaMeta.SetActive(true); marcaMeta.transform.position = Centro(x, z) + Vector3.up * 0.3f; }

        if (inicio.HasValue && meta.HasValue) CalcularYMostrarRuta();
    }

    void CalcularYMostrarRuta() {
        var ruta = AEstrella(inicio.Value, meta.Value, out int expandidos);
        if (ruta.Count == 0) {
            linea.positionCount = 0;
            Debug.Log("No existe ruta entre las casillas.");
            return;
        }
        linea.positionCount = ruta.Count;
        for (int i = 0; i < ruta.Count; i++)
            linea.SetPosition(i, Centro(ruta[i].x, ruta[i].y) + Vector3.up * 0.1f);

        Debug.Log($"Ruta: {ruta.Count} casillas, nodos expandidos: {expandidos}");
    }

    // ================= A* =================
    int Heuristica(Vector2Int a, Vector2Int b) {
        int dx = Mathf.Abs(a.x - b.x), dz = Mathf.Abs(a.y - b.y);
        if (!permitirDiagonales) return 10 * (dx + dz);                    // Manhattan
        return 14 * Mathf.Min(dx, dz) + 10 * Mathf.Abs(dx - dz);           // Octil
    }

    bool Valido(int x, int z) => x >= 0 && z >= 0 && x < ancho && z < alto && libre[x, z];

    List<Vector2Int> AEstrella(Vector2Int ini, Vector2Int fin, out int expandidos) {
        expandidos = 0;
        var ruta = new List<Vector2Int>();

        var open = new List<Nodo>();                       // OPEN
        var closed = new HashSet<Vector2Int>();            // CLOSED
        var todos = new Dictionary<Vector2Int, Nodo>();    // para actualizar g y padre

        var nInicio = new Nodo(ini) { g = 0, h = Heuristica(ini, fin) };
        open.Add(nInicio);
        todos[ini] = nInicio;

        while (open.Count > 0) {
            // Extraer el nodo de menor f (desempate: menor h)
            Nodo actual = open[0];
            for (int i = 1; i < open.Count; i++)
                if (open[i].f < actual.f || (open[i].f == actual.f && open[i].h < actual.h))
                    actual = open[i];

            open.Remove(actual);
            closed.Add(actual.pos);
            expandidos++;

            // Terminamos al EXTRAER la meta, no al descubrirla
            if (actual.pos == fin) {
                for (var n = actual; n != null; n = n.padre) ruta.Add(n.pos);
                ruta.Reverse();
                return ruta;
            }

            for (int dx = -1; dx <= 1; dx++)
                for (int dz = -1; dz <= 1; dz++) {
                    if (dx == 0 && dz == 0) continue;
                    bool diagonal = dx != 0 && dz != 0;
                    if (diagonal && !permitirDiagonales) continue;

                    int nx = actual.pos.x + dx, nz = actual.pos.y + dz;
                    if (!Valido(nx, nz)) continue;
                    // No cortar esquinas entre muros
                    if (diagonal && (!Valido(actual.pos.x + dx, actual.pos.y) || !Valido(actual.pos.x, actual.pos.y + dz))) continue;

                    var vecPos = new Vector2Int(nx, nz);
                    if (closed.Contains(vecPos)) continue;

                    int nuevoG = actual.g + (diagonal ? 14 : 10);

                    if (!todos.TryGetValue(vecPos, out Nodo vec)) {
                        vec = new Nodo(vecPos) { g = nuevoG, h = Heuristica(vecPos, fin), padre = actual };
                        todos[vecPos] = vec;
                        open.Add(vec);
                    }
                    else if (nuevoG < vec.g)               // ruta más barata: actualizar padre
                    {
                        vec.g = nuevoG;
                        vec.padre = actual;
                    }
                }
        }
        return ruta; // frontera vacía: no hay ruta
    }
}