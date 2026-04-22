using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 5f;
    public float sprintSpeed = 8f;
    public float acceleration = 12f;
    public float deceleration = 14f;
    public float airControl = 0.4f;

    public float gravity = -20f;
    public float jumpHeight = 1.5f;

    private CharacterController controller;
    private Vector3 horizontalVelocity;
    private float verticalVelocity;
    private bool isGrounded;

    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    void Update()
    {
        isGrounded = controller.isGrounded;

        if (isGrounded && verticalVelocity < 0)
        {
            verticalVelocity = -2f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 inputDir = transform.right * x + transform.forward * z;
        inputDir = Vector3.ClampMagnitude(inputDir, 1f);

        float targetSpeed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
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
}