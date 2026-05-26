using UnityEngine;

public enum ItemType { Seed, Crop, Tool, Hoe, WateringCan, Scythe, Axe, Pickaxe, Material, Food, Misc }
public enum ItemQuality { Normal, Silver, Gold, Iridium }

[CreateAssetMenu(fileName = "NewItem", menuName = "Farm/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public string description;
    public Sprite icon;
    public ItemType itemType;
    public int maxStack = 99;
    public int sellPrice;
    public int buyPrice;
    public bool canSell = true;
    public CropData cropData;
    public GameObject worldPrefab;
}
