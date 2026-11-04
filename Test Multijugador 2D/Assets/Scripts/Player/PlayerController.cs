using Photon.Pun;
using System;
using UnityEngine;

public class PlayerController : MonoBehaviour {
    public enum PlayerType {
        GREEN,
        PINK,
        YELLOW,
        BLUE
    }

    [Header("Input Settings")]
    private PlayerActions playerInputs;
    private float movementInput;


    [Header("Component Settings")]
    [SerializeField]
    private PlayerType playerType;

    [Header("Component Settings")]
    [SerializeField]
    private Rigidbody2D rb;

    [SerializeField]
    private SpriteRenderer sprite;

    [SerializeField]
    private Animator anim;

    [Header("Movement Settings")]
    [SerializeField]
    private float acceleration;
    private bool canJump = true;
    [SerializeField]
    private float jumpForce = 7f;
    private int jumpsUsed;
    [SerializeField]
    private int doubleJumpAmount = 1;

    [Header("Ground/Wall Detection")]
    [SerializeField] private float groundCheckDistance = 0.5f;
    [SerializeField] private float groundCheckRadius = 0.15f;
    [SerializeField] private LayerMask groundLayers;
    [SerializeField] private bool isGrounded;

    private float lastDirection;

    private void Awake() {
        SetPlayer();

        if (!GetComponent<PhotonView>().IsMine)
            return;

        playerInputs = new PlayerActions();
        playerInputs.Movement.Move.performed += ctx => movementInput = ctx.ReadValue<float>();
        playerInputs.Movement.Move.canceled += ctx => movementInput = 0.0f;
        playerInputs.Movement.Jump.performed += ctx => {
            if (!canJump) return;

            if (isGrounded) {
                Vector3 vel = rb.linearVelocity;
                vel.y = jumpForce;
                rb.linearVelocity = vel;
            }
            else if (jumpsUsed < doubleJumpAmount) {
                Vector3 vel = rb.linearVelocity;
                vel.y = 0f;
                vel.y = jumpForce;
                rb.linearVelocity = vel;
                jumpsUsed++;
            }
        };
    }

    void Start() {
        if (!GetComponent<PhotonView>().IsMine)
            return;
        playerInputs.Enable();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
        //rb.transform.parent = null;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }


    void Update() {
        if (!GetComponent<PhotonView>().IsMine)
            return;

        UpdateGround();
        ApplyMovement();

        UpdateSprite();
    }

    void UpdateGround() {
        RaycastHit2D[] hits = Physics2D.CircleCastAll(
            transform.position,
            groundCheckRadius,
            Vector2.down,
            groundCheckDistance,
            groundLayers
        );

        bool foundValidHit = false;
        float nearestDistance = float.MaxValue;

        foreach (RaycastHit2D hit in hits) {
            if (hit.collider == null)
                continue;

            // Ignore this player's colliders, including colliders on child objects.
            if (hit.collider.transform == transform ||
                hit.collider.transform.IsChildOf(transform))
                continue;

            if (hit.distance < nearestDistance) {
                nearestDistance = hit.distance;
                foundValidHit = true;
            }
        }

        isGrounded = foundValidHit;

        if (isGrounded) {
            anim.SetBool("airTime", false);
            jumpsUsed = 0;
        }
        else
            anim.SetBool("airTime", true);
    }

    void ApplyMovement() {
        if (!isGrounded)
            return;

        float xValue = movementInput * acceleration;
        rb.linearVelocityX = xValue;

        if (Mathf.Abs(rb.linearVelocityX) > 0.2) { lastDirection = rb.linearVelocityX; }
        ;
    }

    void UpdateSprite() {
        Debug.Log(lastDirection);
        for (int i = 1; i < Enum.GetNames(typeof(PlayerType)).Length; i++) {
            anim.SetLayerWeight(i, 0);
        }

        switch (playerType) {
            case PlayerType.GREEN:
                anim.SetLayerWeight(1, 1);
                break;

            case PlayerType.PINK:
                anim.SetLayerWeight(2, 1);
                break;

            case PlayerType.YELLOW:
                anim.SetLayerWeight(3, 1);
                break;

            case PlayerType.BLUE:
                anim.SetLayerWeight(4, 1);
                break;
        }

        if (lastDirection > 0)
            sprite.GetComponent<SyncSpriteFlip>().SetFlipX(true);
        else
            sprite.GetComponent<SyncSpriteFlip>().SetFlipX(false);

        if (Mathf.Abs(rb.linearVelocityX) > 0 && isGrounded) {
            anim.SetBool("isWalking", true);
        }
        else {
            anim.SetBool("isWalking", false);
        }

        anim.speed = 1.5f;

    }

    private void SetPlayer() {
        switch (PlayerPrefs.GetString("playerColor")) {
            case "GREEN":
                playerType = PlayerType.GREEN;
                break;

            case "PINK":
                playerType = PlayerType.PINK;
                break;

            case "YELLOW":
                playerType = PlayerType.YELLOW;
                break;

            case "BLUE":
                playerType = PlayerType.BLUE;
                break;

            default:
                playerType = PlayerType.GREEN;
                break;
        }
    }
}
