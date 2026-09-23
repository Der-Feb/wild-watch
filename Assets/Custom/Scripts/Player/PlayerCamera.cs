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

    [Header("Scroll Zoom (while focusing)")]
    public float maxZoomFOV = 15f;       // lowest FOV = most zoomed in
    public float zoomScrollSpeed = 5f;   // how fast scroll changes zoom
    [Range(0f, 1f)] public float zoomLevel = 0f; // 0 = focusedFOV, 1 = maxZoomFOV

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

        HandleScrollZoom();
        HandleFocusTransition();
        HandleCameraCollision();
    }

    void HandleScrollZoom()
    {
        if (!isFocusing)
            return; // only allow scroll-zoom while focusing

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Mathf.Abs(scroll) > 0.0001f)
        {
            zoomLevel += scroll * zoomScrollSpeed;
            zoomLevel = Mathf.Clamp01(zoomLevel);
        }
    }

    void HandleFocusTransition()
    {
        float targetZ = isFocusing ? focusedZOffset : zOffset;
        float targetY = isFocusing ? focusedYOffset : yOffset;

        // Blend between focusedFOV and maxZoomFOV based on zoomLevel while focusing
        float focusFOVWithZoom = Mathf.Lerp(focusedFOV, maxZoomFOV, zoomLevel);
        float targetFOV = isFocusing ? focusFOVWithZoom : normalFOV;

        currentZOffset = Mathf.Lerp(currentZOffset, targetZ, focusTransitionSpeed * Time.deltaTime);
        currentYOffset = Mathf.Lerp(currentYOffset, targetY, focusTransitionSpeed * Time.deltaTime);

        if (cam != null)
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, targetFOV, focusTransitionSpeed * Time.deltaTime);

        // Reset zoom level once you leave focus mode, so next focus starts unzoomed
        if (!isFocusing && zoomLevel > 0f)
        {
            zoomLevel = 0f;
        }
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