using UnityEngine;

// Extends Interactable so the existing detector already finds it and shows its name.
public class Pickup : Interactable
{
    [Header("Pickup")]
    public ItemData item;

    public bool TryPickUp(InventoryManager inventory)
    {
        if (inventory == null)
        {
            Debug.LogWarning("Pickup failed: InteractionDetector has no InventoryManager assigned.", this);
            return false;
        }

        if (item == null)
        {
            Debug.LogWarning($"Pickup failed: {name} has no ItemData assigned.", this);
            return false;
        }

        // InventoryManager.Add logs the success ("Picked up ...").
        if (!inventory.Add(item))
        {
            Debug.Log($"Pickup failed: inventory full, {item.itemName} stays on the ground.");
            return false;
        }

        Destroy(gameObject);
        return true;
    }
}