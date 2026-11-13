using System;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    public int capacity = 12;

    readonly List<ItemData> items = new List<ItemData>();

    public IReadOnlyList<ItemData> Items => items;
    public bool IsFull => items.Count >= capacity;

    // The inventory UI will subscribe to this later.
    public event Action OnChanged;

    public bool Add(ItemData item)
    {
        if (item == null || IsFull) return false;

        items.Add(item);
        OnChanged?.Invoke();
        Debug.Log($"Picked up {item.itemName} ({items.Count}/{capacity})");
        return true;
    }

    public bool Remove(ItemData item)
    {
        bool removed = items.Remove(item);
        if (removed) OnChanged?.Invoke();
        return removed;
    }
}