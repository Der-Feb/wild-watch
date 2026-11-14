using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Put this on the row prefab.
public class InventorySlotUI : MonoBehaviour
{
    public TMP_Text nameText;
    public Image iconImage;      // optional

    public void Set(ItemData item)
    {
        if (nameText == null)
            Debug.LogWarning("InventorySlotUI: Name Text is not assigned on the prefab.", this);

        if (nameText != null)
            nameText.text = item.itemName;

        if (iconImage != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = item.icon != null;
        }

        Debug.Log($"Slot set: '{item.itemName}'");
    }
}