using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float mouseSensitivity = 140f;
    public float minPitch = -85f;
    public float maxPitch = 85f;

    private float pitch;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        pitch = NormalizeAngle(transform.localEulerAngles.x);
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        ApplyLookInput(mouseX, mouseY);
    }

    public void ApplyLookInput(float mouseX, float mouseY)
    {
        pitch -= mouseY;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);

        if (transform.parent != null)
        {
            transform.parent.Rotate(Vector3.up * mouseX);
        }
    }

    void OnValidate()
    {
        mouseSensitivity = Mathf.Max(1f, mouseSensitivity);

        if (maxPitch < minPitch)
        {
            maxPitch = minPitch;
        }
    }

    private static float NormalizeAngle(float angle)
    {
        if (angle > 180f)
        {
            angle -= 360f;
        }

        return angle;
    }
}
