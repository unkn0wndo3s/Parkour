using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Move")]
    public float moveSpeed = 7f;
    public float airControl = 0.6f;

    [Header("Jump")]
    public float jumpHeight = 1.5f;
    public float gravity = -20f;

    [Header("Wallrun")]
    public float wallCheckDistance = 0.8f;
    public float wallRunGravity = -5f;
    public float wallRunSpeed = 8f;
    public float wallJumpForceUp = 8f;
    public float wallJumpForceSide = 6f;
    public LayerMask wallMask;

    [Header("Camera")]
    public Transform cameraTransform;
    public float cameraTiltAngle = 10f;
    public float cameraTiltSpeed = 8f;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isWallRunning;
    private float currentCameraTilt = 0f;
    private int wallSide = 0; // -1 left, 1 right
    private Vector3 lastWallNormal = Vector3.zero;

    
    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMovement();
        HandleWallRun();
        ApplyGravity();
        ApplyCameraTilt();
    }

    void HandleMovement()
    {
        isGrounded = controller.isGrounded;

        float h = Input.GetAxisRaw("Horizontal");   // Q/D ou A/D
        float v = Input.GetAxisRaw("Vertical");     // Z/S ou W/S

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 moveDir = (forward * v + right * h).normalized;
        float speed = moveSpeed;

        if (!isGrounded && !isWallRunning)
            speed *= airControl;

        Vector3 move = moveDir * speed;
        Vector3 horizontalVelocity = move;
        horizontalVelocity.y = velocity.y;

        controller.Move(horizontalVelocity * Time.deltaTime);

        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                Jump();
            }
            else if (isWallRunning)
            {
                WallJump();
            }
        }
    }

    void Jump()
    {
        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    void HandleWallRun()
    {
        isWallRunning = false;
        wallSide = 0;

        if (isGrounded) return;

        float v = Input.GetAxisRaw("Vertical");
        if (v <= 0.1f) return; // faut avancer

        RaycastHit hit;

        // Check mur à gauche
        if (Physics.Raycast(transform.position, -transform.right, out hit, wallCheckDistance, wallMask))
        {
            StartWallRun(-1, hit.normal);
        }
        // Check mur à droite
        else if (Physics.Raycast(transform.position, transform.right, out hit, wallCheckDistance, wallMask))
        {
            StartWallRun(1, hit.normal);
        }
    }

    void StartWallRun(int side, Vector3 wallNormal)
    {
        isWallRunning = true;
        wallSide = side;
        lastWallNormal = wallNormal; // ➜ on garde la direction du mur

        Vector3 alongWall = Vector3.Cross(wallNormal, Vector3.up);
        if (Vector3.Dot(alongWall, transform.forward) < 0f)
            alongWall = -alongWall;

        Vector3 wallMove = alongWall * wallRunSpeed;
        wallMove.y = velocity.y;

        controller.Move(wallMove * Time.deltaTime);

        if (velocity.y < 0)
            velocity.y = wallRunGravity * Time.deltaTime;
    }


    void WallJump()
    {
        if (!isWallRunning) return;
    
        // On veut sauter dans le sens OPPOSÉ au mur → direction de la normal
        Vector3 jumpDir = Vector3.zero;
    
        if (lastWallNormal != Vector3.zero)
        {
            // Opposé au mur (normal) + vers le haut
            jumpDir = lastWallNormal * wallJumpForceSide + Vector3.up * wallJumpForceUp;
        }
        else
        {
            // fallback si jamais la normal n'est pas set (devrait pas arriver)
            jumpDir = transform.forward * wallJumpForceSide + Vector3.up * wallJumpForceUp;
        }
    
        // Appliquer la composante verticale dans velocity
        velocity.y = jumpDir.y;
    
        // Appliquer la poussée horizontale une fois
        Vector3 horizontal = jumpDir;
        horizontal.y = 0f;
    
        controller.Move(horizontal * Time.deltaTime);
    
        isWallRunning = false;
    }


    void ApplyGravity()
    {
        if (isWallRunning) return;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(new Vector3(0, velocity.y, 0) * Time.deltaTime);
    }

    void ApplyCameraTilt()
    {
        float targetTilt = 0f;
        if (isWallRunning)
            targetTilt = wallSide * -cameraTiltAngle;

        currentCameraTilt = Mathf.Lerp(currentCameraTilt, targetTilt, Time.deltaTime * cameraTiltSpeed);

        if (cameraTransform != null)
        {
            Vector3 e = cameraTransform.localEulerAngles;
            e.z = currentCameraTilt;
            cameraTransform.localEulerAngles = e;
        }
    }
    
    public void ResetVelocity()
    {
        velocity = Vector3.zero;
    }

}
