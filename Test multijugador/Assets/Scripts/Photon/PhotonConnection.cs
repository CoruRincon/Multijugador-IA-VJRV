using UnityEngine;
using UnityEngine.UI;

using Photon.Pun;
using Photon.Realtime;
using TMPro;

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
        panel.gameObject.SetActive(false);
        text.text = $"Connection state: Connected \nPlayers: {PhotonNetwork.CurrentRoom.PlayerCount}";
        joinedRoom = true;

        SpawnPlayer();
    }

    private void FixedUpdate() {
        if(joinedRoom)
            text.text = $"Connection state: Connected \nPlayers: {PhotonNetwork.CurrentRoom.PlayerCount}";
    }

    private void SpawnPlayer() {
        Vector3 spawnPos = new Vector3(0, 1, 0);
        Quaternion spawnRotation = Quaternion.identity;

        PhotonNetwork.Instantiate(
            PlayerPrefab,
            spawnPos,
            spawnRotation);
    }
}
