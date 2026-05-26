using UnityEngine;
using System;
using System.Collections.Generic;

[System.Serializable]
public class InventorySlot
{
    public ItemData item;
    public int quantity;
    public ItemQuality quality;

    public bool IsEmpty => item == null || quantity <= 0;

    public void Clear()
    {
        item = null;
        quantity = 0;
        quality = ItemQuality.Normal;
    }
}

public class InventorySystem : MonoBehaviour
{
    [SerializeField] int slotCount = 36;
    [SerializeField] int hotbarSize = 9;

    public InventorySlot[] Slots { get; private set; }
    public int EquippedIndex { get; private set; }
    public int SlotCount => slotCount;
    public int HotbarSize => hotbarSize;

    public event Action OnInventoryChanged;
    public event Action<int> OnEquippedChanged;

    void Awake()
    {
        Slots = new InventorySlot[slotCount];
        for (int i = 0; i < slotCount; i++)
            Slots[i] = new InventorySlot();
    }

    public ItemData GetEquippedItem()
    {
        if (EquippedIndex < 0 || EquippedIndex >= slotCount) return null;
        return Slots[EquippedIndex].item;
    }

    public void SetEquipped(int index)
    {
        EquippedIndex = Mathf.Clamp(index, 0, hotbarSize - 1);
        OnEquippedChanged?.Invoke(EquippedIndex);
    }

    public bool AddItem(ItemData item, int quantity, ItemQuality quality = ItemQuality.Normal)
    {
        if (item == null || quantity <= 0) return false;

        for (int i = 0; i < slotCount; i++)
        {
            if (Slots[i].item == item && Slots[i].quality == quality && Slots[i].quantity < item.maxStack)
            {
                int canAdd = Mathf.Min(quantity, item.maxStack - Slots[i].quantity);
                Slots[i].quantity += canAdd;
                quantity -= canAdd;
                if (quantity <= 0) break;
            }
        }

        while (quantity > 0)
        {
            int emptySlot = FindEmptySlot();
            if (emptySlot < 0) return false;

            int toAdd = Mathf.Min(quantity, item.maxStack);
            Slots[emptySlot].item = item;
            Slots[emptySlot].quantity = toAdd;
            Slots[emptySlot].quality = quality;
            quantity -= toAdd;
        }

        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool RemoveItem(ItemData item, int quantity)
    {
        int remaining = quantity;
        for (int i = slotCount - 1; i >= 0; i--)
        {
            if (Slots[i].item == item)
            {
                int toRemove = Mathf.Min(remaining, Slots[i].quantity);
                Slots[i].quantity -= toRemove;
                remaining -= toRemove;
                if (Slots[i].quantity <= 0) Slots[i].Clear();
                if (remaining <= 0) break;
            }
        }
        OnInventoryChanged?.Invoke();
        return remaining <= 0;
    }

    public void ConsumeEquipped()
    {
        var slot = Slots[EquippedIndex];
        if (slot.IsEmpty) return;
        slot.quantity--;
        if (slot.quantity <= 0) slot.Clear();
        OnInventoryChanged?.Invoke();
    }

    public int GetItemCount(ItemData item)
    {
        int count = 0;
        for (int i = 0; i < slotCount; i++)
            if (Slots[i].item == item) count += Slots[i].quantity;
        return count;
    }

    public void SwapSlots(int a, int b)
    {
        if (a < 0 || a >= slotCount || b < 0 || b >= slotCount) return;
        (Slots[a], Slots[b]) = (Slots[b], Slots[a]);
        OnInventoryChanged?.Invoke();
    }

    int FindEmptySlot()
    {
        for (int i = 0; i < slotCount; i++)
            if (Slots[i].IsEmpty) return i;
        return -1;
    }
}
