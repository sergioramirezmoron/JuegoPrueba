using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private const float GroundedStickForce = -2f;
    private const string CameraPivotName = "CameraPivot";

    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float acceleration = 12f;
    public float deceleration = 14f;
    public float airControl = 0.4f;

    public float gravity = -20f;
    public float jumpHeight = 1.5f;

    public float controllerHeight = 1.84f;
    public float controllerRadius = 0.32f;
    public float cameraHeight = 1.68f;

    public GameObject interactText;
    public Transform cameraPivot;

    private CharacterController controller;
    private PlayerVitals playerVitals;
    private Vector3 horizontalVelocity;
    private float verticalVelocity;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerVitals = GetComponent<PlayerVitals>();
        ResolveCameraPivot();
        ApplyCharacterSetup();

        if (interactText != null)
        {
            interactText.SetActive(false);
        }
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = GroundedStickForce;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 inputDir = transform.right * x + transform.forward * z;
        inputDir = Vector3.ClampMagnitude(inputDir, 1f);

        if (playerVitals == null)
        {
            playerVitals = GetComponent<PlayerVitals>();
        }

        float speedMultiplier = playerVitals != null ? playerVitals.GetMovementMultiplier() : 1f;
        float targetSpeed = (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed) * speedMultiplier;
        Vector3 targetVelocity = inputDir * targetSpeed;

        float smooth = isGrounded ? acceleration : acceleration * airControl;

        if (inputDir.magnitude > 0.01f)
        {
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, targetVelocity, smooth * Time.deltaTime);
        }
        else
        {
            horizontalVelocity = Vector3.Lerp(horizontalVelocity, Vector3.zero, deceleration * Time.deltaTime);
        }

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;

        Vector3 finalMove = horizontalVelocity;
        finalMove.y = verticalVelocity;

        controller.Move(finalMove * Time.deltaTime);
    }

    public void ApplyCharacterSetup()
    {
        if (controller == null)
        {
            controller = GetComponent<CharacterController>();
        }

        if (controller == null)
        {
            return;
        }

        controller.height = controllerHeight;
        controller.radius = controllerRadius;
        controller.center = new Vector3(0f, controllerHeight * 0.5f, 0f);

        ResolveCameraPivot();

        if (cameraPivot != null)
        {
            Vector3 pivotPosition = cameraPivot.localPosition;
            pivotPosition.x = 0f;
            pivotPosition.y = cameraHeight;
            pivotPosition.z = 0f;
            cameraPivot.localPosition = pivotPosition;
        }
    }

    void OnValidate()
    {
        controllerHeight = Mathf.Max(1.4f, controllerHeight);
        controllerRadius = Mathf.Clamp(controllerRadius, 0.2f, 0.6f);
        cameraHeight = Mathf.Clamp(cameraHeight, 1.2f, controllerHeight - 0.05f);

        ApplyCharacterSetup();
    }

    private void ResolveCameraPivot()
    {
        if (cameraPivot != null)
        {
            return;
        }

        Transform namedPivot = transform.Find(CameraPivotName);
        if (namedPivot != null)
        {
            cameraPivot = namedPivot;
            return;
        }

        if (Camera.main != null && Camera.main.transform.parent != null && Camera.main.transform.parent.IsChildOf(transform))
        {
            cameraPivot = Camera.main.transform.parent;
        }
    }
}
