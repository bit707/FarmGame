using UnityEngine;

[CreateAssetMenu(fileName = "NewCrop", menuName = "Farm/Crop Data")]
public class CropData : ScriptableObject
{
    public string cropName;
    public Sprite icon;
    public int daysToGrow = 5;
    public int regrowDays = 0;
    public Season[] growSeasons;
    public int seedPrice = 20;
    public ItemData harvestItem;
    public GameObject[] growthStagePrefabs;
    public ParticleSystem harvestEffect;

    [Header("Quality")]
    [Range(0f, 1f)] public float silverChance = 0.2f;
    [Range(0f, 1f)] public float goldChance = 0.05f;

    public bool CanGrowInSeason(Season season)
    {
        if (growSeasons == null || growSeasons.Length == 0) return true;
        foreach (var s in growSeasons)
            if (s == season) return true;
        return false;
    }
}
