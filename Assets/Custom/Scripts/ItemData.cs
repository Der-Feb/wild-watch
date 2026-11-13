using UnityEngine;

[CreateAssetMenu(menuName = "Wild Watch/Item", fileName = "NewItem")]
public class ItemData : ScriptableObject
{
    public string itemName = "Item";
    public Sprite icon;               // used by the inventory UI later
    public GameObject worldPrefab;    // what to spawn when the item is dropped (later)
}