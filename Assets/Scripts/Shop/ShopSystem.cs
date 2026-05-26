using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ShopItem
{
    public ItemData item;
    public int price;
    public Season[] availableSeasons;
    public int stock = -1; // -1 = unlimited

    public bool IsAvailable(Season season)
    {
        if (availableSeasons == null || availableSeasons.Length == 0) return true;
        foreach (var s in availableSeasons)
            if (s == season) return true;
        return false;
    }
}

public class ShopSystem : MonoBehaviour
{
    [SerializeField] List<ShopItem> shopInventory = new List<ShopItem>();

    public event System.Action OnShopUpdated;

    public List<ShopItem> GetAvailableItems()
    {
        Season current = GameManager.Instance.CurrentSeason;
        var available = new List<ShopItem>();
        foreach (var item in shopInventory)
        {
            if (item.IsAvailable(current) && item.stock != 0)
                available.Add(item);
        }
        return available;
    }

    public bool BuyItem(ShopItem shopItem, int quantity = 1)
    {
        int totalCost = shopItem.price * quantity;
        if (!GameManager.Instance.SpendGold(totalCost)) return false;

        GameManager.Instance.Inventory.AddItem(shopItem.item, quantity);

        if (shopItem.stock > 0)
        {
            shopItem.stock -= quantity;
            OnShopUpdated?.Invoke();
        }
        return true;
    }

    public bool SellItem(int inventorySlot)
    {
        var slot = GameManager.Instance.Inventory.Slots[inventorySlot];
        if (slot.IsEmpty || !slot.item.canSell) return false;

        int price = CalculateSellPrice(slot.item, slot.quality);
        GameManager.Instance.AddGold(price);
        GameManager.Instance.Inventory.RemoveItem(slot.item, 1);
        return true;
    }

    int CalculateSellPrice(ItemData item, ItemQuality quality)
    {
        float multiplier = quality switch
        {
            ItemQuality.Silver => 1.25f,
            ItemQuality.Gold => 1.5f,
            ItemQuality.Iridium => 2f,
            _ => 1f
        };
        return Mathf.RoundToInt(item.sellPrice * multiplier);
    }
}
