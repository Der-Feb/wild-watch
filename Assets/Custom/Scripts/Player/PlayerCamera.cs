using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;

    [Header("Camera Offsets")]
    public float zOffset = 0f; 
    public float yOffset = 0f;

    float xRotation = 0f;
    Vector3 baseLocalPosition;

    void Start()
    {
        // Lock the cursor to the middle of the screen and hide it
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Remember wherever you manually placed the camera in the editor as the base
        baseLocalPosition = transform.localPosition;
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Control rotation around x axis (Look up and down)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 75f);

        // Apply vertical rotation to the camera itself
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotate the player body horizontally around the y axis
        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }

        // Take the base position where you put the camera and add your extra offsets to it
        Vector3 targetOffset = new Vector3(0f, yOffset, -zOffset);
        transform.localPosition = baseLocalPosition + targetOffset;
    }
}