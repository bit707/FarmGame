using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CropDatabase", menuName = "Farm/Crop Database")]
public class CropDatabase : ScriptableObject
{
    public List<CropData> crops = new List<CropData>();

    Dictionary<string, CropData> lookup;

    public CropData GetCrop(string id)
    {
        if (lookup == null) BuildLookup();
        return lookup.TryGetValue(id, out var crop) ? crop : null;
    }

    void BuildLookup()
    {
        lookup = new Dictionary<string, CropData>();
        foreach (var crop in crops)
            if (crop != null) lookup[crop.name] = crop;
    }
}
