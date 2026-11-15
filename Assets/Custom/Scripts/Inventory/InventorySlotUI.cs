using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Put this on the root of the row prefab. The root also needs a Button.
[DisallowMultipleComponent]
public class InventorySlotUI : MonoBehaviour
{
    public TMP_Text nameText;
    public Image iconImage;              // optional
    public Button button;                // the row itself; found automatically if left empty
    public GameObject equippedMarker;    // optional: shown while this item is equipped
    public Button dropButton;            // optional: a small "Drop" button inside the row (NOT the row's own button)

    // onDrop is optional so the existing InventoryUI call keeps compiling.
    public void Set(ItemData item, int count, bool equipped, Action<ItemData> onClick, Action<ItemData> onDrop = null)
    {
        if (nameText == null)
            Debug.LogWarning("InventorySlotUI: Name Text is not assigned on the prefab.", this);
        else
            nameText.text = count > 1 ? $"{item.itemName} x{count}" : item.itemName;

        if (iconImage != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = item.icon != null;
        }

        // Ammo is never "equipped", so it never shows the marker.
        if (equippedMarker != null)
            equippedMarker.SetActive(equipped && !item.isAmmo);

        if (button == null) button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            // Ammo rows ignore clicks: they can't be made the active item.
            if (!item.isAmmo)
                button.onClick.AddListener(() => onClick?.Invoke(item));
        }

        if (dropButton != null)
        {
            dropButton.onClick.RemoveAllListeners();
            dropButton.gameObject.SetActive(onDrop != null);
            if (onDrop != null)
                dropButton.onClick.AddListener(() => onDrop(item));
        }
    }
}