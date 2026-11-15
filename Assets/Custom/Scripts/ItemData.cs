using UnityEngine;

[CreateAssetMenu(menuName = "Wild Watch/Item", fileName = "NewItem")]
public class ItemData : ScriptableObject
{
    public string itemName = "Item";
    public Sprite icon;               // used by the inventory UI
    public GameObject worldPrefab;    // the BIG display model spawned inside a dropped station

    [Tooltip("Ammo is carried and counted, never equipped, and has its own carry limit.")]
    public bool isAmmo;

    public bool CanEquip => !isAmmo;
}