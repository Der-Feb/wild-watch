using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;

    [Header("Camera Offsets")]
    public float zOffset = 0f;
    public float yOffset = 0f;

    [Header("Collision Avoidance")]
    public LayerMask obstacleMask;       // set this to "world" in the Inspector
    public float collisionRadius = 0.3f; // thickness of the sphere cast
    public float collisionBuffer = 0.2f; // small gap to keep camera from touching the wall

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

        HandleCameraCollision();
    }

    void HandleCameraCollision()
    {
        // Take the base position where you put the camera and add your extra offsets to it
        Vector3 targetOffset = new Vector3(0f, yOffset, -zOffset);
        Vector3 desiredLocalPos = baseLocalPosition + targetOffset;

        // Work out where that position is in WORLD space, since Physics casts need world coordinates
        Vector3 pivotWorldPos = playerBody.TransformPoint(baseLocalPosition + new Vector3(0f, yOffset, 0f));
        Vector3 desiredWorldPos = playerBody.TransformPoint(desiredLocalPos);

        Vector3 direction = desiredWorldPos - pivotWorldPos;
        float distance = direction.magnitude;

        float finalDistance = distance;

        if (distance > 0.001f)
        {
            RaycastHit hit;
            if (Physics.SphereCast(pivotWorldPos, collisionRadius, direction.normalized, out hit, distance, obstacleMask))
            {
                // Something is in the way — pull the camera in to just before the hit point
                finalDistance = hit.distance - collisionBuffer;
                finalDistance = Mathf.Max(finalDistance, 0f); // never go negative/behind the pivot
            }
        }

        // Convert the (possibly shortened) distance back into a local position along the same direction
        Vector3 finalWorldPos = pivotWorldPos + direction.normalized * finalDistance;
        transform.position = finalWorldPos;
    }
}
