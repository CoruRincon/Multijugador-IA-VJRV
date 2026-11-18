
using UnityEngine;
public class MazeGenerator : MonoBehaviour {

    [SerializeField]
    private GameObject floorPrefab, wallPrefab;

    public int width = 10; // Ancho del laberinto
    public int height = 10; // Altura del laberinto
    private int[,] maze; // Representación del laberinto en un array bidimensional
    void Start() {
        GenerateMaze();
    }
    void GenerateMaze() {
        maze = new int[width, height];
        // Inicializar el laberinto
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                maze[i, j] = 1; // 1 representa un muro, 0 representa un camino
            }
        }
        // Llamar al método de generación recursiva
        GeneratePath(1, 1);
        // Puedes imprimir el laberinto en la consola para ver la representación
        PrintMaze();

        if (height > width)
            Camera.main.orthographicSize = (float)height / 1.75f;
        else
            Camera.main.orthographicSize = (float)width / 2f;

        Camera.main.transform.position = new Vector3((float)width / 2 - 0.5f, (float)height / 2 - 0.5f, -10);
    }
    void GeneratePath(int x, int y) {
        maze[x, y] = 0; // Marcar la posición actual como parte del camino
                        // Direcciones posibles (arriba, derecha, abajo, izquierda)
        int[] directions = { 0, 1, 2, 3 };
        Shuffle(directions);
        // Explorar direcciones posibles
        for (int i = 0; i < directions.Length; i++) {
            int nextX = x + 2 * (directions[i] == 1 ? 1 : (directions[i] == 3 ? -1 : 0));
            int nextY = y + 2 * (directions[i] == 2 ? 1 : (directions[i] == 0 ? -1 : 0));
            // Verificar si la próxima posición está dentro de los límites y aún no ha sido visitada
            if (nextX > 0 && nextX < width - 1 && nextY > 0 && nextY < height - 1 && maze[nextX, nextY] == 1) {
                maze[x + (nextX - x) / 2, y + (nextY - y) / 2] = 0; // Romper el muro entre las posiciones
                GeneratePath(nextX, nextY);
            }
        }
    }
    void Shuffle(int[] array) {
        // Método para barajar un array
        for (int i = array.Length - 1; i > 0; i--) {
            int j = Random.Range(0, i + 1);
            int temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }
    void PrintMaze() {
        // Método para imprimir el laberinto en la consola
        for (int j = height - 1; j >= 0; j--) {
            for (int i = 0; i < width; i++) {
                if (maze[i, j] == 1) {
                    GameObject thisPiece = Instantiate(wallPrefab, new Vector2(i, j), Quaternion.identity, transform);
                }
                else {
                    GameObject thisPiece = Instantiate(floorPrefab, new Vector2(i, j), Quaternion.identity, transform);
                }
            }
            Debug.Log("\n");
        }
    }
}