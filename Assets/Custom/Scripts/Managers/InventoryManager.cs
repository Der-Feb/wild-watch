using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [Header("Limits")]
    public int capacity = 12;        // non-ammo items (guns etc.)
    public int ammoCapacity = 60;    // all ammo together; ammo never eats the 12 slots

    [Header("Dropping")]
    public ItemStation dropStationPrefab;  // generic station: ItemStation + trigger collider + empty Visual pivot + LabelAnchor
    public Transform dropPoint;            // the Player (or a child of it); its forward is the drop direction
    public float dropDistance = 1.5f;      // keeps the new station out of the player
    public LayerMask groundMask;           // your `ground` layer; leave at Nothing to skip ground snapping
    public int ammoDropChunk = 10;         // one Drop click on an ammo row drops this many

    readonly List<ItemData> items = new List<ItemData>();
    int ammoCount;

    public IReadOnlyList<ItemData> Items => items;

    public int ItemCount => items.Count - ammoCount;   // non-ammo only
    public int AmmoCount => ammoCount;
    public bool IsFull => ItemCount >= capacity;
    public bool IsAmmoFull => ammoCount >= ammoCapacity;

    // The inventory UI subscribes to this.
    public event Action OnChanged;
    // Short player-facing messages ("Inventory full"). InventoryNotice subscribes to this.
    public event Action<string> OnNotice;

    public int GetCount(ItemData item)
    {
        int n = 0;
        for (int i = 0; i < items.Count; i++)
            if (items[i] == item) n++;
        return n;
    }

    public bool Add(ItemData item)
    {
        if (item == null) return false;

        bool full = item.isAmmo ? IsAmmoFull : IsFull;
        if (full)
        {
            OnNotice?.Invoke(item.isAmmo ? "Ammo full" : "Inventory full");
            return false;
        }

        items.Add(item);
        if (item.isAmmo) ammoCount++;

        OnChanged?.Invoke();
        Debug.Log($"Picked up {item.itemName} ({ItemCount}/{capacity} items, {ammoCount}/{ammoCapacity} ammo)");
        return true;
    }

    public bool Remove(ItemData item)
    {
        if (!RemoveInternal(item)) return false;
        OnChanged?.Invoke();
        return true;
    }

    // Shooting: if (!inventory.Remove(bullet)) return;  -> never goes negative.

    bool RemoveInternal(ItemData item)
    {
        bool removed = items.Remove(item);
        if (removed && item.isAmmo) ammoCount--;
        return removed;
    }

    // Button on an inventory row: 1 item, or a chunk of ammo.
    public void DropOne(ItemData item)
    {
        if (item == null) return;
        Drop(item, item.isAmmo ? ammoDropChunk : 1);
    }

    public void Drop(ItemData item, int amount)
    {
        if (item == null) return;

        amount = Mathf.Min(amount, GetCount(item));
        if (amount <= 0) return;

        if (dropStationPrefab == null || dropPoint == null)
        {
            Debug.LogWarning("InventoryManager: assign Drop Station Prefab and Drop Point to allow dropping.", this);
            OnNotice?.Invoke("Can't drop here");
            return;
        }

        Vector3 pos = dropPoint.position + dropPoint.forward * dropDistance;
        if (groundMask.value != 0 &&
            Physics.Raycast(pos + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 10f, groundMask))
            pos = hit.point;   // sit on the ground, don't float

        for (int i = 0; i < amount; i++)
            RemoveInternal(item);

        ItemStation station = Instantiate(dropStationPrefab, pos, Quaternion.identity);
        station.Setup(item, amount);

        OnChanged?.Invoke();
    }
}