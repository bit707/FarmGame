using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopUI : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] Transform itemListContainer;
    [SerializeField] GameObject shopItemPrefab;
    [SerializeField] TextMeshProUGUI playerGoldText;
    [SerializeField] KeyCode closeKey = KeyCode.Escape;

    bool isOpen;
    List<GameObject> spawnedItems = new List<GameObject>();

    void Update()
    {
        if (isOpen && Input.GetKeyDown(closeKey))
            Close();
    }

    public void Open()
    {
        isOpen = true;
        panel.SetActive(true);
        RefreshShop();
        GameManager.Instance.Time.Pause();
    }

    public void Close()
    {
        isOpen = false;
        panel.SetActive(false);
        GameManager.Instance.Time.Resume();
    }

    void RefreshShop()
    {
        foreach (var obj in spawnedItems) Destroy(obj);
        spawnedItems.Clear();

        var items = GameManager.Instance.Shop.GetAvailableItems();
        foreach (var shopItem in items)
        {
            var obj = Instantiate(shopItemPrefab, itemListContainer);
            var ui = obj.GetComponent<ShopItemUI>();
            ui.Setup(shopItem, OnBuy);
            spawnedItems.Add(obj);
        }

        UpdateGoldDisplay();
    }

    void OnBuy(ShopItem item)
    {
        if (GameManager.Instance.Shop.BuyItem(item))
            UpdateGoldDisplay();
    }

    void UpdateGoldDisplay()
    {
        if (playerGoldText != null)
            playerGoldText.text = $"{GameManager.Instance.Gold}G";
    }
}
