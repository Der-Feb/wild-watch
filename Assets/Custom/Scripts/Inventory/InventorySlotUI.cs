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

    public void Set(ItemData item, int count, bool equipped, Action<ItemData> onClick)
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

        if (equippedMarker != null)
            equippedMarker.SetActive(equipped);

        if (button == null) button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClick?.Invoke(item));
        }
    }
}