using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class Billboard : MonoBehaviour
{
    [Header("Billboard Settings")]
    public bool yAxisOnly = true;

    Transform cam;
    Renderer rend;

    public bool IsActive { get; private set; } = true;

    void Start()
    {
        if (Camera.main != null)
            cam = Camera.main.transform;

        rend = GetComponent<Renderer>();

        if (GrassManager.Instance != null)
            GrassManager.Instance.Register(this);
    }

    void OnDestroy()
    {
        if (GrassManager.Instance != null)
            GrassManager.Instance.Unregister(this);
    }

    void LateUpdate()
    {
        if (!IsActive || cam == null) return;

        if (yAxisOnly)
        {
            Vector3 direction = cam.position - transform.position;
            direction.y = 0f;
            if (direction.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(-direction);
            }
        }
        else
        {
            transform.rotation = Quaternion.LookRotation(transform.position - cam.position);
        }
    }

    public void SetActive(bool active)
    {
        IsActive = active;
        if (rend != null)
            rend.enabled = active;
    }
}