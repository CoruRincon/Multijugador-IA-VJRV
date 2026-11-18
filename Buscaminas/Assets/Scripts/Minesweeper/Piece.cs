using TMPro;
using UnityEngine;

public class Piece : MonoBehaviour {
    [SerializeField]
    private int x, y;

    [SerializeField]
    private bool isBomb;
    public int nearbyBombs = 0;
    private bool revealed;

    public void SetPosition(int x, int y) {
        this.x = x;
        this.y = y;
    }

    public bool IsBomb() {
        return isBomb;
    }

    public void SetBomb(bool bombState) {
        isBomb = bombState;
    }

    public void SetNearbyBombs(int bombAmount) {
        nearbyBombs = bombAmount;
    }

    public void RevealCheck() {
        if(isBomb || revealed) { return; }
        revealed = true;

        nearbyBombs = MapGenerator.Instance.GetNearbyBombs(x, y);

        GetComponent<SpriteRenderer>().color = Color.green;
        GetComponentInChildren<TextMeshProUGUI>().text = nearbyBombs.ToString();

        if (nearbyBombs == 0)
            MapGenerator.Instance.RevealNearbyEmpty(x, y);
    }

    private void OnMouseDown() {
        revealed = true;
        if (isBomb) {
            GetComponent<SpriteRenderer>().color = Color.red;
        }
        else {
            nearbyBombs = MapGenerator.Instance.GetNearbyBombs(x, y);
            GetComponent<SpriteRenderer>().color = Color.green;
            GetComponentInChildren<TextMeshProUGUI>().text = nearbyBombs.ToString();

            if(nearbyBombs == 0)
                MapGenerator.Instance.RevealNearbyEmpty(x, y);
        }
    }

    private void OnMouseEnter() {
        if (revealed) { return; }
        GetComponent<SpriteRenderer>().color = Color.gray;
    }

    private void OnMouseExit() {
        if (revealed) { return; }
        GetComponent<SpriteRenderer>().color = Color.white;
    }
}
