using TMPro;
using UnityEngine;

public class InteractionDetector : MonoBehaviour
{
    [Header("References")]
    public PlayerCamera playerCamera;
    public TMP_Text interactionText;
    public InventoryManager inventory;

    [Header("Settings")]
    public float interactRange = 5f;          // how far names are shown
    public float pickupRange = 1f;            // how close you must be to pick up
    public KeyCode pickupKey = KeyCode.E;
    public LayerMask interactableMask;        // "predators" + "prey" + "pickups"

    void Update()
    {
        if (playerCamera == null || !playerCamera.isFocusing)
        {
            Hide();
            return;
        }

        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, interactableMask))
        {
            Interactable interactable = hit.collider.GetComponentInParent<Interactable>();
            if (interactable != null)
            {
                string label = $"{interactable.DisplayName} ({hit.distance:F1}m)";

                if (interactable is Pickup pickup && hit.distance <= pickupRange)
                {
                    if (inventory != null && inventory.IsFull)
                    {
                        label += "\nInventory full";
                    }
                    else
                    {
                        label += $"\n[{pickupKey}] Pick up";

                        if (Input.GetKeyDown(pickupKey))
                        {
                            pickup.TryPickUp(inventory);
                            Hide();
                            return;
                        }
                    }
                }

                if (interactionText != null)
                {
                    interactionText.gameObject.SetActive(true);
                    interactionText.text = label;
                }
                return;
            }
        }

        Hide();
    }

    void Hide()
    {
        if (interactionText != null) interactionText.gameObject.SetActive(false);
    }
}