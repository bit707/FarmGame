using UnityEngine;
using System.IO;
using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int gold;
    public int day;
    public int hour;
    public int minute;
    public int year;
    public Season season;
    public WeatherType weather;
    public Vector3Serializable playerPosition;
    public List<InventorySlotSave> inventory = new List<InventorySlotSave>();
    public List<FarmTileSave> farmTiles = new List<FarmTileSave>();
}

[System.Serializable]
public class Vector3Serializable
{
    public float x, y, z;
    public Vector3Serializable(Vector3 v) { x = v.x; y = v.y; z = v.z; }
    public Vector3 ToVector3() => new Vector3(x, y, z);
}

[System.Serializable]
public class InventorySlotSave
{
    public string itemId;
    public int quantity;
    public ItemQuality quality;
}

[System.Serializable]
public class FarmTileSave
{
    public int x, z;
    public TileState state;
    public string cropId;
    public int growthDay;
    public bool watered;
}

public class SaveSystem : MonoBehaviour
{
    [SerializeField] ItemDatabase itemDatabase;
    [SerializeField] CropDatabase cropDatabase;

    string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    public void SaveGame()
    {
        var gm = GameManager.Instance;
        var data = new SaveData
        {
            gold = gm.Gold,
            day = gm.Time.CurrentDay,
            hour = gm.Time.CurrentHour,
            minute = gm.Time.CurrentMinute,
            year = gm.Time.CurrentYear,
            season = gm.Time.CurrentSeason,
            weather = gm.Weather.CurrentWeather,
        };

        var player = FindObjectOfType<PlayerController>();
        if (player != null)
            data.playerPosition = new Vector3Serializable(player.Position);

        for (int i = 0; i < gm.Inventory.SlotCount; i++)
        {
            var slot = gm.Inventory.Slots[i];
            data.inventory.Add(new InventorySlotSave
            {
                itemId = slot.IsEmpty ? "" : slot.item.name,
                quantity = slot.quantity,
                quality = slot.quality
            });
        }

        foreach (var kvp in gm.Farm.GetAllTiles())
        {
            data.farmTiles.Add(new FarmTileSave
            {
                x = kvp.Key.x,
                z = kvp.Key.z,
                state = kvp.Value.state,
                cropId = kvp.Value.cropData != null ? kvp.Value.cropData.name : "",
                growthDay = kvp.Value.growthDay,
                watered = kvp.Value.wateredToday
            });
        }

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"Game saved to {SavePath}");
    }

    public void LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("No save file found.");
            return;
        }

        string json = File.ReadAllText(SavePath);
        var data = JsonUtility.FromJson<SaveData>(json);
        var gm = GameManager.Instance;

        gm.Gold = data.gold;
        gm.Time.SetTime(data.day, data.hour, data.season, data.year);
        gm.Weather.SetWeather(data.weather);

        var player = FindObjectOfType<PlayerController>();
        if (player != null && data.playerPosition != null)
        {
            var cc = player.GetComponent<CharacterController>();
            cc.enabled = false;
            player.transform.position = data.playerPosition.ToVector3();
            cc.enabled = true;
        }

        for (int i = 0; i < data.inventory.Count && i < gm.Inventory.SlotCount; i++)
        {
            var slotData = data.inventory[i];
            if (string.IsNullOrEmpty(slotData.itemId))
            {
                gm.Inventory.Slots[i].Clear();
                continue;
            }
            var item = itemDatabase.GetItem(slotData.itemId);
            if (item != null)
            {
                gm.Inventory.Slots[i].item = item;
                gm.Inventory.Slots[i].quantity = slotData.quantity;
                gm.Inventory.Slots[i].quality = slotData.quality;
            }
        }

        Debug.Log("Game loaded.");
    }

    public bool HasSave() => File.Exists(SavePath);
}
