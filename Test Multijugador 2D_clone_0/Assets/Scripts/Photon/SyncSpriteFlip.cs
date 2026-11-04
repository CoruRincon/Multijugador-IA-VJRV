using Photon.Pun;
using UnityEngine;

public class SyncSpriteFlip : MonoBehaviourPun {
    [SerializeField] private SpriteRenderer spriteRenderer;

    public void SetFlipX(bool flipX) {
        spriteRenderer.flipX = flipX;
        photonView.RPC(nameof(RPC_SetFlipX), RpcTarget.Others, flipX);
    }

    [PunRPC]
    private void RPC_SetFlipX(bool flipX) {
        spriteRenderer.flipX = flipX;
    }
}