using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ShopItemUI : MonoBehaviour
{
    [SerializeField] Image icon;
    [SerializeField] TextMeshProUGUI nameText;
    [SerializeField] TextMeshProUGUI priceText;
    [SerializeField] Button buyButton;

    ShopItem shopItem;
    Action<ShopItem> onBuy;

    public void Setup(ShopItem item, Action<ShopItem> buyCallback)
    {
        shopItem = item;
        onBuy = buyCallback;

        if (icon != null && item.item.icon != null)
            icon.sprite = item.item.icon;
        if (nameText != null)
            nameText.text = item.item.itemName;
        if (priceText != null)
            priceText.text = $"{item.price}G";

        buyButton.onClick.AddListener(OnBuyClicked);
        UpdateInteractable();
    }

    void OnBuyClicked()
    {
        onBuy?.Invoke(shopItem);
        UpdateInteractable();
    }

    void UpdateInteractable()
    {
        buyButton.interactable = GameManager.Instance.Gold >= shopItem.price;
    }
}
