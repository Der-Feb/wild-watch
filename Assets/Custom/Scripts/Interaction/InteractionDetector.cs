using TMPro;
using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    [Header("References")]
    public PlayerCamera playerCamera;
    public TMP_Text interactionText;

    [Header("Settings")]
    public float interactRange = 5f;
    public LayerMask interactableMask; // set to "predators" + "prey"

    void Update()
    {
        if (playerCamera == null || !playerCamera.isFocusing)
        {
            if (interactionText != null) interactionText.gameObject.SetActive(false);
            return;
        }

        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableMask))
        {
            Interactable interactable = hit.collider.GetComponentInParent<Interactable>();
            if (interactable != null)
            {
                if (interactionText != null)
                {
                    interactionText.gameObject.SetActive(true);
                    interactionText.text = $"{interactable.DisplayName} ({hit.distance:F1}m)";
                }
                return;
            }
        }

        if (interactionText != null) interactionText.gameObject.SetActive(false);
    }
}