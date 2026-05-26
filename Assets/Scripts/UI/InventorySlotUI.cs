using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class InventorySlotUI : MonoBehaviour
{
    [SerializeField] Image itemIcon;
    [SerializeField] TextMeshProUGUI quantityText;
    [SerializeField] Image qualityIndicator;
    [SerializeField] Button button;

    int slotIndex;

    public void Init(int index, Action onClick)
    {
        slotIndex = index;
        button.onClick.AddListener(() => onClick());
    }

    public void UpdateSlot(InventorySlot slot)
    {
        if (slot == null || slot.IsEmpty)
        {
            itemIcon.enabled = false;
            quantityText.text = "";
            if (qualityIndicator != null) qualityIndicator.enabled = false;
            return;
        }

        itemIcon.enabled = true;
        itemIcon.sprite = slot.item.icon;
        quantityText.text = slot.quantity > 1 ? slot.quantity.ToString() : "";

        if (qualityIndicator != null)
        {
            qualityIndicator.enabled = slot.quality != ItemQuality.Normal;
            qualityIndicator.color = slot.quality switch
            {
                ItemQuality.Silver => Color.gray,
                ItemQuality.Gold => Color.yellow,
                ItemQuality.Iridium => new Color(0.6f, 0.2f, 1f),
                _ => Color.clear
            };
        }
    }
}
