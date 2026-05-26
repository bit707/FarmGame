using UnityEngine;
using UnityEditor;

public class FarmGameDataSetup : EditorWindow
{
    [MenuItem("FarmGame/生成初始数据")]
    public static void CreateStarterData()
    {
        CreateFolders();
        CreateTools();
        CreateCrops();
        CreateItemDatabase();
        CreateCropDatabase();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("完成", "初始数据已生成！\n\n查看 Assets/ScriptableObjects/ 文件夹。", "好的");
    }

    static void CreateFolders()
    {
        EnsureFolder("Assets/ScriptableObjects");
        EnsureFolder("Assets/ScriptableObjects/Items");
        EnsureFolder("Assets/ScriptableObjects/Crops");
    }

    static void CreateTools()
    {
        CreateTool("Hoe", "锄头 - 翻耕土地", ItemType.Hoe, new Color(0.6f, 0.4f, 0.2f));
        CreateTool("WateringCan", "水壶 - 浇灌作物", ItemType.WateringCan, new Color(0.3f, 0.5f, 0.8f));
        CreateTool("Scythe", "镰刀 - 收割作物", ItemType.Scythe, new Color(0.7f, 0.7f, 0.7f));
        CreateTool("Axe", "斧头 - 砍伐树木", ItemType.Axe, new Color(0.5f, 0.3f, 0.1f));
        CreateTool("Pickaxe", "镐子 - 敲碎石头", ItemType.Pickaxe, new Color(0.4f, 0.4f, 0.5f));
    }

    static void CreateTool(string name, string desc, ItemType type, Color color)
    {
        var item = ScriptableObject.CreateInstance<ItemData>();
        item.itemName = name;
        item.description = desc;
        item.itemType = type;
        item.maxStack = 1;
        item.canSell = false;
        item.icon = CreateColorSprite(color, name + "_icon");
        AssetDatabase.CreateAsset(item, $"Assets/ScriptableObjects/Items/{name}.asset");
    }

    static void CreateCrops()
    {
        CreateCropSet("Turnip", "萝卜", 4, 40, 60, new[] { Season.Spring },
            new Color(0.9f, 0.9f, 0.8f), new Color(0.8f, 0.3f, 0.5f));
        CreateCropSet("Potato", "土豆", 6, 50, 80, new[] { Season.Spring },
            new Color(0.7f, 0.6f, 0.3f), new Color(0.6f, 0.5f, 0.2f));
        CreateCropSet("Tomato", "番茄", 8, 60, 100, new[] { Season.Summer },
            new Color(0.9f, 0.2f, 0.1f), new Color(0.2f, 0.6f, 0.2f), 3);
        CreateCropSet("Corn", "玉米", 10, 80, 150, new[] { Season.Summer },
            new Color(0.9f, 0.8f, 0.2f), new Color(0.3f, 0.7f, 0.2f), 4);
        CreateCropSet("Pumpkin", "南瓜", 12, 100, 250, new[] { Season.Autumn },
            new Color(0.9f, 0.5f, 0.1f), new Color(0.2f, 0.5f, 0.1f));
        CreateCropSet("Carrot", "胡萝卜", 5, 30, 50, new[] { Season.Autumn },
            new Color(0.9f, 0.5f, 0.1f), new Color(0.3f, 0.6f, 0.2f));
    }

    static void CreateCropSet(string name, string displayName, int days, int seedPrice, int sellPrice,
        Season[] seasons, Color cropColor, Color seedColor, int regrow = 0)
    {
        // harvest item
        var harvestItem = ScriptableObject.CreateInstance<ItemData>();
        harvestItem.itemName = displayName;
        harvestItem.description = $"收获的{displayName}，可以出售。";
        harvestItem.itemType = ItemType.Crop;
        harvestItem.maxStack = 99;
        harvestItem.sellPrice = sellPrice;
        harvestItem.canSell = true;
        harvestItem.icon = CreateColorSprite(cropColor, name + "_crop_icon");
        AssetDatabase.CreateAsset(harvestItem, $"Assets/ScriptableObjects/Items/{name}_Crop.asset");

        // seed item
        var seedItem = ScriptableObject.CreateInstance<ItemData>();
        seedItem.itemName = $"{displayName}种子";
        seedItem.description = $"{displayName}的种子，种在翻耕的土地上。";
        seedItem.itemType = ItemType.Seed;
        seedItem.maxStack = 99;
        seedItem.buyPrice = seedPrice;
        seedItem.sellPrice = seedPrice / 2;
        seedItem.canSell = true;
        seedItem.icon = CreateColorSprite(seedColor, name + "_seed_icon");

        // crop data
        var crop = ScriptableObject.CreateInstance<CropData>();
        crop.cropName = displayName;
        crop.daysToGrow = days;
        crop.regrowDays = regrow;
        crop.growSeasons = seasons;
        crop.seedPrice = seedPrice;
        crop.harvestItem = harvestItem;
        crop.icon = harvestItem.icon;
        AssetDatabase.CreateAsset(crop, $"Assets/ScriptableObjects/Crops/{name}.asset");

        seedItem.cropData = crop;
        AssetDatabase.CreateAsset(seedItem, $"Assets/ScriptableObjects/Items/{name}_Seed.asset");
    }

    static void CreateItemDatabase()
    {
        var db = ScriptableObject.CreateInstance<ItemDatabase>();
        string[] guids = AssetDatabase.FindAssets("t:ItemData", new[] { "Assets/ScriptableObjects/Items" });
        foreach (var guid in guids)
        {
            var item = AssetDatabase.LoadAssetAtPath<ItemData>(AssetDatabase.GUIDToAssetPath(guid));
            if (item != null) db.items.Add(item);
        }
        AssetDatabase.CreateAsset(db, "Assets/ScriptableObjects/ItemDatabase.asset");
    }

    static void CreateCropDatabase()
    {
        var db = ScriptableObject.CreateInstance<CropDatabase>();
        string[] guids = AssetDatabase.FindAssets("t:CropData", new[] { "Assets/ScriptableObjects/Crops" });
        foreach (var guid in guids)
        {
            var crop = AssetDatabase.LoadAssetAtPath<CropData>(AssetDatabase.GUIDToAssetPath(guid));
            if (crop != null) db.crops.Add(crop);
        }
        AssetDatabase.CreateAsset(db, "Assets/ScriptableObjects/CropDatabase.asset");
    }

    static Sprite CreateColorSprite(Color color, string name)
    {
        var tex = new Texture2D(32, 32);
        var pixels = new Color[32 * 32];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = color;
        tex.SetPixels(pixels);
        tex.Apply();

        string path = $"Assets/Art/Textures/{name}.png";
        EnsureFolder("Assets/Art/Textures");
        System.IO.File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path);

        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spritePixelsPerUnit = 32;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static void EnsureFolder(string path)
    {
        string[] parts = path.Replace("\\", "/").Split('/');
        string current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
