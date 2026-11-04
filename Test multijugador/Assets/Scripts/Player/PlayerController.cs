using Photon.Pun;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Input Settings")]
    private PlayerActions playerInputs;
    private Vector2 movementInput;
    private Vector2 rotationInput;

    [Header("Component Settings")]
    [SerializeField]
    private Rigidbody rb;
    [SerializeField]
    private Camera cam;

    [Header("Movement Settings")]
    [SerializeField]
    private float acceleration;
    [SerializeField]
    private float maxSpeed;
    [SerializeField]
    private float rotationSpeed;
    private bool canJump = true;
    [SerializeField] 
    private float jumpForce = 7f;
    private int jumpsUsed;
    [SerializeField] 
    private int doubleJumpAmount = 1;

    [Header("Ground/Wall Detection")]
    [SerializeField] private float groundCheckDistance = 0.5f;
    [SerializeField] private LayerMask groundLayers;
    private bool isGrounded;
    private Vector3 groundNormal = Vector3.up;

    private void Awake() {
        playerInputs = new PlayerActions();
        playerInputs.Movement.Move.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        playerInputs.Movement.Move.canceled += ctx => movementInput = Vector2.zero;
        playerInputs.Movement.Rotate.performed += ctx => rotationInput = ctx.ReadValue<Vector2>();
        playerInputs.Movement.Rotate.canceled += ctx => rotationInput = Vector2.zero;
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
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerInputs.Enable();
        if (rb == null) rb = GetComponent<Rigidbody>();
        //rb.transform.parent = null;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
    }

    // Update is called once per frame
    void Update()
    {
        if (!GetComponent<PhotonView>().IsMine)
            return; 

        UpdateGround();

        ApplyRotation();
        ApplyMovement();
    }

    void UpdateGround() {
        if (Physics.Raycast(rb.position, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayers)) {
            isGrounded = true;
            jumpsUsed = 0;
            groundNormal = hit.normal;
        }
        else {
            isGrounded = false;
            groundNormal = Vector3.up;
        }
    }

    void ApplyMovement() {
        Vector3 camForward = Vector3.ProjectOnPlane(cam.transform.forward, groundNormal).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(cam.transform.right, groundNormal).normalized;

        Vector3 inputDir = (camForward * movementInput.y + camRight * movementInput.x).normalized;
        if (inputDir.sqrMagnitude < 0.01f) return;

        // Only apply impulse if horizontal speed is below max
        Vector3 flatVel = Vector3.ProjectOnPlane(rb.linearVelocity, Vector3.up);
        if (flatVel.magnitude < maxSpeed)
            rb.linearVelocity += inputDir * acceleration;
    }

    void ApplyRotation() {
        if (Mathf.Abs(movementInput.x) > 0.01f) {
            Vector3 currentForward = transform.forward;
            Vector3 desiredForward = Quaternion.AngleAxis(rotationInput.x * rotationSpeed * Time.fixedDeltaTime, groundNormal) * currentForward;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(desiredForward, groundNormal), Time.deltaTime);
        }
    }
}
