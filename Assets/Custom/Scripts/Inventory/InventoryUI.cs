using TMPro;
using UnityEngine;

// Put this on the InventoryPanel. It only listens while the panel is open.
public class InventoryUI : MonoBehaviour
{
    public InventoryManager inventory;     // optional: found automatically if left empty
    public Transform content;              // object with a Vertical Layout Group
    public InventorySlotUI slotPrefab;     // one row
    public TMP_Text emptyText;             // optional "Nothing here yet"

    void OnEnable()
    {
        if (inventory == null)
            inventory = FindFirstObjectByType<InventoryManager>();

        if (inventory == null)
        {
            Debug.LogWarning("InventoryUI: no InventoryManager found.", this);
            return;
        }

        inventory.OnChanged += Rebuild;
        Rebuild();   // always fresh when the panel opens
    }

    void OnDisable()
    {
        if (inventory != null)
            inventory.OnChanged -= Rebuild;
    }

    void Rebuild()
    {
        if (content == null || slotPrefab == null) return;

        for (int i = content.childCount - 1; i >= 0; i--)
            Destroy(content.GetChild(i).gameObject);

        foreach (ItemData item in inventory.Items)
        {
            InventorySlotUI slot = Instantiate(slotPrefab, content);
            slot.Set(item);
        }

        if (emptyText != null)
            emptyText.gameObject.SetActive(inventory.Items.Count == 0);
    }
}