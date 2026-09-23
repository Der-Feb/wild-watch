using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public float mouseSensitivity = 100f;
    public Transform playerBody;

    [Header("Normal (Third-Person) Offsets")]
    public float zOffset = 1.5f;
    public float yOffset = 1.5f;

    [Header("Focused (First-Person) Offsets")]
    public float focusedZOffset = 0f;
    public float focusedYOffset = 0.3f;
    public float focusTransitionSpeed = 8f;

    [Header("Field of View")]
    public Camera cam;
    public float normalFOV = 60f;
    public float focusedFOV = 40f;

    [Header("Collision Avoidance")]
    public LayerMask obstacleMask;
    public float collisionRadius = 0.3f;
    public float collisionBuffer = 0.2f;

    [HideInInspector] public bool isFocusing = false;

    float xRotation = 0f;
    Vector3 baseLocalPosition;
    float currentZOffset;
    float currentYOffset;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        baseLocalPosition = transform.localPosition;

        // Start at the normal third-person offset
        currentZOffset = zOffset;
        currentYOffset = yOffset;

        if (cam == null) cam = GetComponent<Camera>();
    }

    void Update()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 75f);
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        if (playerBody != null)
        {
            playerBody.Rotate(Vector3.up * mouseX);
        }

        HandleFocusTransition();
        HandleCameraCollision();
    }

    void HandleFocusTransition()
    {
        float targetZ = isFocusing ? focusedZOffset : zOffset;
        float targetY = isFocusing ? focusedYOffset : yOffset;
        float targetFOV = isFocusing ? focusedFOV : normalFOV;

        currentZOffset = Mathf.Lerp(currentZOffset, targetZ, focusTransitionSpeed * Time.deltaTime);
        currentYOffset = Mathf.Lerp(currentYOffset, targetY, focusTransitionSpeed * Time.deltaTime);

        if (cam != null)
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, focusTransitionSpeed * Time.deltaTime);
    }

    void HandleCameraCollision()
    {
        Vector3 targetOffset = new Vector3(0f, currentYOffset, -currentZOffset);
        Vector3 desiredLocalPos = baseLocalPosition + targetOffset;

        Vector3 pivotWorldPos = playerBody.TransformPoint(baseLocalPosition + new Vector3(0f, currentYOffset, 0f));
        Vector3 desiredWorldPos = playerBody.TransformPoint(desiredLocalPos);

        Vector3 direction = desiredWorldPos - pivotWorldPos;
        float distance = direction.magnitude;
        float finalDistance = distance;

        if (distance > 0.001f)
        {
            RaycastHit hit;
            if (Physics.SphereCast(pivotWorldPos, collisionRadius, direction.normalized, out hit, distance, obstacleMask))
            {
                finalDistance = Mathf.Max(hit.distance - collisionBuffer, 0f);
            }
        }

        Vector3 finalWorldPos = pivotWorldPos + direction.normalized * finalDistance;
        transform.position = finalWorldPos;
    }
}