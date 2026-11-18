using UnityEngine;

public class MapGenerator : MonoBehaviour {
    public static MapGenerator Instance;

    [Header("Map Generation Settings")]
    [SerializeField]
    private GameObject piecePrefab;

    [SerializeField]
    private int mapWidth, mapHeight;

    GameObject[][] map;

    [Header("Bomb Generation Settings")]
    [SerializeField]
    private int bombAmount;

    private void Awake() {
        Instance = this;
    }

    private void Start() {
        InitializeMap();
        SetBombs();
    }

    private void InitializeMap() {
        map = new GameObject[mapWidth][];
        for (int i = 0; i < map.Length; i++) {
            map[i] = new GameObject[mapHeight];
        }

        for (int i = 0; i < mapWidth; i++) {
            for (int j = 0; j < mapHeight; j++) {
                GameObject thisPiece = Instantiate(piecePrefab, new Vector2(i, j), Quaternion.identity, transform);
                thisPiece.GetComponent<Piece>().SetPosition(i, j);
                map[i][j] = thisPiece;
            }
        }
        if (mapHeight > mapWidth)
            Camera.main.orthographicSize = (float)mapHeight / 1.75f;
        else
            Camera.main.orthographicSize = (float)mapWidth / 2f;

        Camera.main.transform.position = new Vector3((float)mapWidth / 2 - 0.5f, (float)mapHeight / 2 - 0.5f, -10);
    }

    private void SetBombs() {
        for (int i = 0; i < bombAmount; i++) {
            int xValue = Random.Range(0, mapWidth);
            int yValue = Random.Range(0, mapHeight);

            if(!map[xValue][yValue].GetComponent<Piece>().IsBomb()) {
                map[xValue][yValue].GetComponent<Piece>().SetBomb(true);
            } else {
                i--;
            }

        }
    }

    public int GetNearbyBombs(int x, int y) {
        int bombsFound = 0;

        if (x > 0 && y < mapHeight - 1 && map[x - 1][y + 1].GetComponent<Piece>().IsBomb()) {
            bombsFound++;
        }
        if (y < mapHeight - 1 && map[x][y + 1].GetComponent<Piece>().IsBomb()) {
            bombsFound++;
        }
        if (x < mapWidth - 1 && y < mapHeight - 1 && map[x + 1][y + 1].GetComponent<Piece>().IsBomb()) {
            bombsFound++;
        }

        if (x > 0 && map[x - 1][y].GetComponent<Piece>().IsBomb()) {
            bombsFound++;
        }

        if (x < mapWidth - 1 && map[x + 1][y].GetComponent<Piece>().IsBomb()) {
            bombsFound++;
        }

        if (x > 0 && y > 0 && map[x - 1][y - 1].GetComponent<Piece>().IsBomb()) {
            bombsFound++;
        }
        if (y > 0 && map[x][y - 1].GetComponent<Piece>().IsBomb()) {
            bombsFound++;
        }
        if (x < mapWidth - 1 && y > 0 && map[x + 1][y - 1].GetComponent<Piece>().IsBomb()) {
            bombsFound++;
        }

        return bombsFound;
    }

    public void RevealNearbyEmpty(int x, int y) {
        if (x > 0 && y < mapHeight - 1) {
            map[x - 1][y + 1].GetComponent<Piece>().RevealCheck();
        }
        if (y < mapHeight - 1) {
            map[x][y + 1].GetComponent<Piece>().RevealCheck();
        }
        if (x < mapWidth - 1 && y < mapHeight - 1) {
            map[x + 1][y + 1].GetComponent<Piece>().RevealCheck();
        }

        if (x > 0) {
            map[x - 1][y].GetComponent<Piece>().RevealCheck();
        }

        if (x < mapWidth - 1) {
            map[x + 1][y].GetComponent<Piece>().RevealCheck();
        }

        if (x > 0 && y > 0) {
            map[x - 1][y - 1].GetComponent<Piece>().RevealCheck();
        }
        if (y > 0) {
            map[x][y - 1].GetComponent<Piece>().RevealCheck();
        }
        if (x < mapWidth - 1 && y > 0) {
            map[x + 1][y - 1].GetComponent<Piece>().RevealCheck();
        }
    }
}
