using Photon.Pun;
using System.Collections;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour {
    [SerializeField] private string playerPrefab = "Player";

    private IEnumerator Start() {
        yield return new WaitUntil(() => PhotonNetwork.InRoom);

        GameObject spawn = GameObject.FindWithTag("PlayerSpawn");
        if (spawn == null) {
            Debug.LogError("PlayerSpawn not found");
            yield break;
        }

        Debug.Log($"Spawning for actor {PhotonNetwork.LocalPlayer.ActorNumber}");
        PhotonNetwork.Instantiate(
            playerPrefab,
            spawn.transform.position,
            spawn.transform.rotation
        );
    }
}