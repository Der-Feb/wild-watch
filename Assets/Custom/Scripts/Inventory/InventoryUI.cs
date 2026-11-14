using System.Collections.Generic;
using TMPro;
using UnityEngine;

// Put this on the InventoryPanel. It only listens while the panel is open.
public class InventoryUI : MonoBehaviour
{
    public InventoryManager inventory;     // optional: found automatically if left empty
    public EquipmentManager equipment;     // optional: found automatically if left empty
    public Transform content;              // object with a Vertical Layout Group
    public InventorySlotUI slotPrefab;     // one row
    public TMP_Text emptyText;             // optional "Nothing here yet"

    void OnEnable()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<InventoryManager>();
        if (equipment == null)
            equipment = FindFirstObjectByType<EquipmentManager>();

        if (inventory == null)
        {
            Debug.LogWarning("InventoryUI: no InventoryManager found.", this);
            return;
        }

        inventory.OnChanged += Rebuild;
        if (equipment != null) equipment.OnChanged += Rebuild;

        Rebuild();   // always fresh when the panel opens
    }

    void OnDisable()
    {
        if (inventory != null) inventory.OnChanged -= Rebuild;
        if (equipment != null) equipment.OnChanged -= Rebuild;
    }

    void Rebuild()
    {
        if (content == null || slotPrefab == null) return;

        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);

        // One row per kind of item, with how many of it you hold.
        var counts = new Dictionary<ItemData, int>();
        var order = new List<ItemData>();

        foreach (ItemData item in inventory.Items)
        {
            if (item == null) continue;

            if (!counts.ContainsKey(item))
            {
                counts[item] = 0;
                order.Add(item);
            }
            counts[item]++;
        }

        foreach (ItemData item in order)
        {
            InventorySlotUI slot = Instantiate(slotPrefab, content);
            bool equipped = equipment != null && equipment.Equipped == item;
            slot.Set(item, counts[item], equipped, OnSlotClicked);
        }

        if (emptyText != null)
            emptyText.gameObject.SetActive(order.Count == 0);
    }

    void OnSlotClicked(ItemData item)
    {
        if (equipment != null)
            equipment.Toggle(item);
    }
}