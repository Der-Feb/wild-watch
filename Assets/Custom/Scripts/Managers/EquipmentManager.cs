using System;
using UnityEngine;

// Put this on Managers. It remembers which inventory item is the active one.
// The state lives here, not on ItemData: ItemData is an asset shared by every copy of the item.
public class EquipmentManager : MonoBehaviour
{
    public InventoryManager inventory;   // optional: found automatically if left empty

    public ItemData Equipped { get; private set; }
    public int EquippedCount => Count(Equipped);

    // Fires when the selection changes AND when the inventory changes (so counts stay fresh).
    public event Action OnChanged;

    void Start()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<InventoryManager>();

        if (inventory == null)
        {
            Debug.LogWarning("EquipmentManager: no InventoryManager found.", this);
            return;
        }

        inventory.OnChanged += OnInventoryChanged;
    }

    void OnDestroy()
    {
        if (inventory != null)
            inventory.OnChanged -= OnInventoryChanged;
    }

    public int Count(ItemData item)
    {
        if (inventory == null || item == null) return 0;

        int n = 0;
        foreach (ItemData i in inventory.Items)
            if (i == item) n++;
        return n;
    }

    public void Equip(ItemData item)
    {
        if (item == null || Count(item) == 0) return;   // cannot equip what you do not have

        Equipped = item;
        Debug.Log($"Equipped {item.itemName} (x{Count(item)})");
        OnChanged?.Invoke();
    }

    public void Unequip()
    {
        if (Equipped == null) return;

        Debug.Log($"Unequipped {Equipped.itemName}");
        Equipped = null;
        OnChanged?.Invoke();
    }

    // Click the equipped item again to unequip it; click another to switch to it.
    // Only one item can be equipped at a time because Equipped is a single field.
    public void Toggle(ItemData item)
    {
        if (item == null) return;

        if (Equipped == item) Unequip();
        else Equip(item);
    }

    void OnInventoryChanged()
    {
        // The equipped item ran out (dropped, used up): clear the selection.
        if (Equipped != null && Count(Equipped) == 0)
            Equipped = null;

        OnChanged?.Invoke();
    }
}