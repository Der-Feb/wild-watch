using TMPro;
using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    [Header("References")]
    public PlayerCamera playerCamera; // to check isFocusing
    public TMP_Text interactionText;      // swap to TMP_Text if you're using TextMeshPro

    [Header("Settings")]
    public float interactRange = 5f;
    public LayerMask interactableMask; // optional: restrict what the ray can hit

    void Update()
    {
        if (playerCamera == null || !playerCamera.isFocusing)
        {
            if (interactionText != null)
                interactionText.gameObject.SetActive(false);
            return;
        }

        RaycastHit hit;
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out hit, interactRange, interactableMask))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();

            if (interactable != null)
            {
                float distance = hit.distance;

                if (interactionText != null)
                {
                    interactionText.gameObject.SetActive(true);
                    interactionText.text = $"{interactable.DisplayName} ({distance:F1}m)";
                }

                return;
            }
        }

        // Nothing valid hit — hide the text
        if (interactionText != null)
            interactionText.gameObject.SetActive(false);
    }
}