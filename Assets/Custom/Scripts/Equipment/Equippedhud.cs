using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Put this on an ALWAYS-ACTIVE object in the top right of the UI canvas.
// Put the visible parts (icon, count) under a child called Frame, which this script shows and hides.
public class EquippedHUD : MonoBehaviour
{
    public EquipmentManager equipment;   // optional: found automatically if left empty

    public GameObject frame;             // hidden when nothing is equipped
    public Image icon;
    public TMP_Text countText;
    public TMP_Text nameText;            // optional, useful while an item has no icon yet

    void Start()
    {
        if (equipment == null)
            equipment = FindFirstObjectByType<EquipmentManager>();

        if (equipment == null)
        {
            Debug.LogWarning("EquippedHUD: no EquipmentManager found.", this);
            return;
        }

        equipment.OnChanged += Refresh;
        Refresh();
    }

    void OnDestroy()
    {
        if (equipment != null)
            equipment.OnChanged -= Refresh;
    }

    void Refresh()
    {
        ItemData item = equipment.Equipped;
        bool has = item != null;

        if (frame != null) frame.SetActive(has);
        if (!has) return;

        if (icon != null)
        {
            icon.sprite = item.icon;
            icon.enabled = item.icon != null;
        }

        if (countText != null) countText.text = $"x{equipment.EquippedCount}";
        if (nameText != null) nameText.text = item.itemName;
    }
}