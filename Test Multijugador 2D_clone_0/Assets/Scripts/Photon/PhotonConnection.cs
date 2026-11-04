using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using TMPro;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PhotonConnection : MonoBehaviourPunCallbacks {

    [SerializeField]
    GameObject panel;

    [SerializeField]
    Button button;

    [SerializeField]
    TextMeshProUGUI text;

    bool joinedRoom;

    [SerializeField]
    string PlayerPrefab = "Player";


    private void Awake() {
        DontDestroyOnLoad(this);
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start() {
        Photon.Pun.PhotonNetwork.ConnectUsingSettings();
    }

    override
    public void OnConnectedToMaster() {
        print("Connected to master");
        text.text = "Connection state: Master Connected";
        button.enabled = true;
    }

    public void ButtonConnect() {
        RoomOptions roomOptions = new RoomOptions() { MaxPlayers = 4 };
        string roomName = "Room1";
        PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
        Debug.Log($"Connecting to {roomName}");
    }

    override
    public void OnJoinedRoom() {
        panel.SetActive(false);
        text.text = $"Connection state: Connected \nPlayers: {PhotonNetwork.CurrentRoom.PlayerCount}";
        joinedRoom = true;

        if (PhotonNetwork.IsMasterClient) {
            PhotonNetwork.LoadLevel("Game");
        }
    }

    private void FixedUpdate() {
        if(joinedRoom)
            text.text = $"Connection state: Connected \nPlayers: {PhotonNetwork.CurrentRoom.PlayerCount}";
    }

    public void ChoosePlayer(string playerColor) {
        PlayerPrefs.SetString("playerColor", playerColor);
    }
}
