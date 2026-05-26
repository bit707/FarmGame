using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Farm/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemData> items = new List<ItemData>();

    Dictionary<string, ItemData> lookup;

    public ItemData GetItem(string id)
    {
        if (lookup == null) BuildLookup();
        return lookup.TryGetValue(id, out var item) ? item : null;
    }

    void BuildLookup()
    {
        lookup = new Dictionary<string, ItemData>();
        foreach (var item in items)
            if (item != null) lookup[item.name] = item;
    }
}
