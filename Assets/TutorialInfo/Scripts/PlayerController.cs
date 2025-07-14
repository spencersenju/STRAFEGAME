using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 4f;
    public float sprintSpeed = 8f;
    public float gravity = -20f;
    public float jumpHeight = 1.5f;

    [Header("Dash")]
    public float dashForce = 20f;
    public float dashDuration = 0.25f;
    public float dashCooldown = 1f;
    private bool isDashing = false;

    [Header("Slide")]
    public float slideSpeed = 12f;
    public float slideDuration = 0.7f;
    private bool isSliding;
    private Vector3 originalCamPos;
    public float slideCamHeight = 0.6f;
    public float slideCamLerpSpeed = 8f;

    [Header("Double Jump")]
    private bool canDoubleJump = false;

    [Header("Wall Run")]
    public float wallRunForce = 8f;
    public float wallRunDuration = 1.2f;
    public float wallGravity = -2f;
    private bool isWallRunning;
    private float wallRunTimer;
    private Vector3 wallNormal;
    public float wallCamTilt = 15f;
    public float wallCamTiltSpeed = 8f;

    [Header("Camera & Look")]
    public Transform cameraTransform;    // Main Camera
    public Transform cameraHolder;       // Empty parent
    public Camera playerCamera;
    public float mouseSensitivity = 100f;
    public float normalFOV = 75f;
    public float sprintFOV = 90f;
    public float fovLerpSpeed = 10f;

    [Header("Dash Trail")]
    public TrailRenderer dashTrail;

    private CharacterController controller;
    private Vector3 velocity;
    private Vector3 dashVelocity = Vector3.zero;
    private float xRotation = 0f;
    private bool isGrounded;
    private bool canDash = true;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;
        if (playerCamera == null)
            playerCamera = Camera.main;
        if (cameraHolder == null)
            cameraHolder = cameraTransform.parent;

        playerCamera.fieldOfView = normalFOV;
        originalCamPos = cameraTransform.localPosition;

        if (dashTrail != null)
            dashTrail.emitting = false;
    }

    void Update()
    {
        HandleMouseLook();
        CheckGround();
        HandleMovement();
        HandleJump();
        HandleDash();
        HandleSlide();
        HandleWallRun();
        HandleFOV();
        HandleCameraTiltAndSquash();
    }

    void CheckGround()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded)
        {
            canDoubleJump = true;
            if (velocity.y < 0) velocity.y = -2f;
        }
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cameraHolder.localRotation = Quaternion.Euler(xRotation, 0f, 0f); // vertical look
        transform.Rotate(Vector3.up * mouseX); // horizontal look
    }

    void HandleMovement()
    {
        if (isSliding || isWallRunning || isDashing) return;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;

        controller.Move(move * speed * Time.deltaTime);

        velocity.y += (isWallRunning ? wallGravity : gravity) * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
            else if (canDoubleJump && !isWallRunning)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                canDoubleJump = false;
            }
        }
    }

    void HandleFOV()
    {
        float targetFOV = Input.GetKey(KeyCode.LeftShift) ? sprintFOV : normalFOV;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, targetFOV, fovLerpSpeed * Time.deltaTime);
    }

    void HandleDash()
    {
        if (Input.GetKeyDown(KeyCode.E) && canDash && !isSliding && !isDashing)
        {
            StartCoroutine(Dash());
        }
    }

    IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        if (dashTrail != null) dashTrail.emitting = true;

        float elapsed = 0f;
        Vector3 startVelocity = transform.forward * dashForce;

        while (elapsed < dashDuration)
        {
            float t = elapsed / dashDuration;
            dashVelocity = Vector3.Lerp(startVelocity, Vector3.zero, t);
            controller.Move(dashVelocity * Time.deltaTime);

            elapsed += Time.deltaTime;
            yield return null;
        }

        dashVelocity = Vector3.zero;
        isDashing = false;
        if (dashTrail != null) dashTrail.emitting = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    void HandleSlide()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && isGrounded && !isSliding && Input.GetAxis("Vertical") > 0)
        {
            StartCoroutine(Slide());
        }
    }

    IEnumerator Slide()
    {
        isSliding = true;
        float slideTime = 0f;

        while (slideTime < slideDuration)
        {
            Vector3 slideDir = transform.forward * slideSpeed;
            controller.Move(slideDir * Time.deltaTime);
            slideTime += Time.deltaTime;
            yield return null;
        }

        isSliding = false;
    }

    void HandleWallRun()
    {
        if (isGrounded || Input.GetAxis("Vertical") <= 0)
        {
            isWallRunning = false;
            return;
        }

        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.right, out hit, 1f) && hit.collider.CompareTag("WallRun"))
        {
            StartWallRun(hit.normal);
        }
        else if (Physics.Raycast(transform.position, -transform.right, out hit, 1f) && hit.collider.CompareTag("WallRun"))
        {
            StartWallRun(hit.normal);
        }
        else
        {
            isWallRunning = false;
        }

        if (isWallRunning)
        {
            controller.Move(transform.forward * wallRunForce * Time.deltaTime);
        }
    }

    void StartWallRun(Vector3 normal)
    {
        isWallRunning = true;
        wallNormal = normal;
        wallRunTimer += Time.deltaTime;
        velocity.y = 0f;

        if (wallRunTimer >= wallRunDuration)
        {
            isWallRunning = false;
            wallRunTimer = 0f;
        }
    }

    void HandleCameraTiltAndSquash()
    {
        // Tilt (Z axis) only on Main Camera (cameraTransform)
        float targetTilt = 0f;
        if (isWallRunning)
        {
            if (Physics.Raycast(transform.position, transform.right, 1f))
            targetTilt = wallCamTilt;  // now tilts AWAY from wall
            else if (Physics.Raycast(transform.position, -transform.right, 1f))
            targetTilt = -wallCamTilt;

        }

        Quaternion camTiltRot = Quaternion.Euler(0f, 0f, targetTilt);
        cameraTransform.localRotation = Quaternion.Slerp(
            cameraTransform.localRotation,
            camTiltRot,
            wallCamTiltSpeed * Time.deltaTime
        );

        // Slide camera squash (Y position)
        Vector3 targetPos = isSliding
            ? new Vector3(originalCamPos.x, slideCamHeight, originalCamPos.z)
            : originalCamPos;

        cameraTransform.localPosition = Vector3.Lerp(
            cameraTransform.localPosition,
            targetPos,
            slideCamLerpSpeed * Time.deltaTime
        );
    }
}
