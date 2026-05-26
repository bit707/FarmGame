using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HotbarSlotUI : MonoBehaviour
{
    [SerializeField] Image itemIcon;
    [SerializeField] TextMeshProUGUI quantityText;
    [SerializeField] Image selectionBorder;
    [SerializeField] Image background;

    int slotIndex;

    public void Init(int index)
    {
        slotIndex = index;
        SetSelected(false);
    }

    public void UpdateSlot(InventorySlot slot)
    {
        if (slot == null || slot.IsEmpty)
        {
            itemIcon.enabled = false;
            quantityText.text = "";
            return;
        }

        itemIcon.enabled = true;
        itemIcon.sprite = slot.item.icon;
        quantityText.text = slot.quantity > 1 ? slot.quantity.ToString() : "";
    }

    public void SetSelected(bool selected)
    {
        if (selectionBorder != null)
            selectionBorder.enabled = selected;
        if (background != null)
            background.color = selected ? new Color(1f, 0.9f, 0.5f, 0.8f) : new Color(0.2f, 0.2f, 0.2f, 0.8f);
    }
}
