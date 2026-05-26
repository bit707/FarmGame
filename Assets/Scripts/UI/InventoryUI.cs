using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] GameObject panel;
    [SerializeField] Transform slotContainer;
    [SerializeField] GameObject slotPrefab;
    [SerializeField] TextMeshProUGUI itemNameText;
    [SerializeField] TextMeshProUGUI itemDescText;
    [SerializeField] KeyCode toggleKey = KeyCode.E;

    InventorySlotUI[] slotUIs;
    int selectedSlot = -1;
    bool isOpen;

    void Start()
    {
        var inv = GameManager.Instance.Inventory;
        slotUIs = new InventorySlotUI[inv.SlotCount];
        for (int i = 0; i < inv.SlotCount; i++)
        {
            var obj = Instantiate(slotPrefab, slotContainer);
            slotUIs[i] = obj.GetComponent<InventorySlotUI>();
            int index = i;
            slotUIs[i].Init(i, () => OnSlotClicked(index));
        }

        inv.OnInventoryChanged += RefreshAll;
        panel.SetActive(false);
        RefreshAll();
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
            Toggle();
    }

    public void Toggle()
    {
        isOpen = !isOpen;
        panel.SetActive(isOpen);
        if (isOpen) RefreshAll();
    }

    void RefreshAll()
    {
        var inv = GameManager.Instance.Inventory;
        for (int i = 0; i < slotUIs.Length; i++)
            slotUIs[i].UpdateSlot(inv.Slots[i]);
    }

    void OnSlotClicked(int index)
    {
        if (selectedSlot >= 0 && selectedSlot != index)
        {
            GameManager.Instance.Inventory.SwapSlots(selectedSlot, index);
            selectedSlot = -1;
        }
        else
        {
            selectedSlot = index;
            ShowItemInfo(index);
        }
    }

    void ShowItemInfo(int index)
    {
        var slot = GameManager.Instance.Inventory.Slots[index];
        if (slot.IsEmpty)
        {
            itemNameText.text = "";
            itemDescText.text = "";
            return;
        }
        itemNameText.text = slot.item.itemName;
        itemDescText.text = slot.item.description;
    }
}
