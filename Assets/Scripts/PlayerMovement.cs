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
    public float wallJumpForceSide = 10f;
    public float wallJumpForceUp = 8f;
    public float wallJumpForwardBoost = 3f;
    public LayerMask wallMask;

    [Header("Camera")]
    public Transform cameraTransform;
    public float cameraTiltAngle = 10f;
    public float cameraTiltSpeed = 8f;

    [Header("Impulse")]
    public float impulseDamp = 5f; // plus haut = l'impulsion se dissipe plus vite

    private CharacterController controller;
    private Vector3 velocity;           // composante verticale
    private Vector3 externalImpulse;    // impulsions horizontales (walljump, etc.)
    private bool isGrounded;
    private bool isWallRunning;
    private bool isNearWall;
    private float currentCameraTilt = 0f;
    private int wallSide = 0;           // -1 left, 1 right
    private Vector3 lastWallNormal = Vector3.zero;

    // lock du contrôle vers le mur après un walljump
    private float wallJumpControlLockTime = 0.3f;
    private float wallJumpControlTimer = 0f;

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
        DampenImpulse();

        // décrémente le timer de lock
        if (wallJumpControlTimer > 0f)
            wallJumpControlTimer -= Time.deltaTime;
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

        // 🔒 Pendant un court moment après un walljump, on bloque l'input VERS le mur
        if (wallJumpControlTimer > 0f && lastWallNormal != Vector3.zero && moveDir.sqrMagnitude > 0.0001f)
        {
            Vector3 towardsWall = -lastWallNormal.normalized; // vers le mur
            float dot = Vector3.Dot(moveDir, towardsWall);

            if (dot > 0f)
            {
                moveDir -= towardsWall * dot;
                moveDir.Normalize();
            }
        }

        float speed = moveSpeed;

        if (!isGrounded && !isWallRunning)
            speed *= airControl;

        // mouvement de base + impulsion externe (walljump etc.)
        Vector3 horizontal = moveDir * speed + externalImpulse;

        // combine avec la vitesse verticale
        Vector3 finalMove = new Vector3(horizontal.x, velocity.y, horizontal.z);
        controller.Move(finalMove * Time.deltaTime);

        // petit reset au sol
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                Jump();
            }
            else if (isWallRunning || isNearWall)
            {
                // 👉 projection si wallrun + jump OU si on glisse le long du mur + jump
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
        isNearWall = false;

        if (isGrounded) return;

        float v = Input.GetAxisRaw("Vertical");

        RaycastHit hit;

        // Check mur à gauche
        if (Physics.Raycast(transform.position, -transform.right, out hit, wallCheckDistance, wallMask))
        {
            isNearWall = true;
            lastWallNormal = hit.normal;
            wallSide = -1;

            // on ne lance le wallrun que si on avance
            if (v > 0.1f)
            {
                StartWallRun(-1, hit.normal);
                return;
            }
        }
        // Check mur à droite
        else if (Physics.Raycast(transform.position, transform.right, out hit, wallCheckDistance, wallMask))
        {
            isNearWall = true;
            lastWallNormal = hit.normal;
            wallSide = 1;

            if (v > 0.1f)
            {
                StartWallRun(1, hit.normal);
                return;
            }
        }
    }

    void StartWallRun(int side, Vector3 wallNormal)
    {
        isWallRunning = true;
        wallSide = side;
        lastWallNormal = wallNormal;

        // direction le long du mur
        Vector3 alongWall = Vector3.Cross(wallNormal, Vector3.up);
        if (Vector3.Dot(alongWall, transform.forward) < 0f)
            alongWall = -alongWall;

        // on force le déplacement le long du mur via l'impulsion horizontale
        Vector3 wallMove = alongWall * wallRunSpeed;
        externalImpulse = wallMove;

        // gravité réduite pendant le wallrun
        if (velocity.y < 0)
            velocity.y = wallRunGravity;
    }

    void WallJump()
    {
        // direction opposée au mur
        Vector3 awayFromWall;
        if (lastWallNormal != Vector3.zero)
            awayFromWall = lastWallNormal.normalized;
        else
            awayFromWall = -transform.forward;

        // poussée dans la direction de la caméra (avant)
        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;
        forward.Normalize();

        // impulsion horizontale : loin du mur + un peu vers l'avant
        Vector3 jumpHorizontal = awayFromWall * wallJumpForceSide
                                 + forward * wallJumpForwardBoost;

        externalImpulse = jumpHorizontal;

        // impulsion verticale
        velocity.y = wallJumpForceUp;

        isWallRunning = false;

        // 🔒 lock temporaire du contrôle vers le mur
        wallJumpControlTimer = wallJumpControlLockTime;
    }

    void ApplyGravity()
    {
        // si wallrun actif, on ne rajoute pas de gravité ici
        if (isWallRunning) return;

        velocity.y += gravity * Time.deltaTime;
    }

    void DampenImpulse()
    {
        // dissipation progressive de l'impulsion horizontale
        externalImpulse = Vector3.Lerp(externalImpulse, Vector3.zero, impulseDamp * Time.deltaTime);
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

    // utilisé par le GameManager quand tu respawn
    public void ResetVelocity()
    {
        velocity = Vector3.zero;
        externalImpulse = Vector3.zero;
        wallJumpControlTimer = 0f;
    }
}
