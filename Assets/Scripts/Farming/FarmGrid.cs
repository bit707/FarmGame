using UnityEngine;
using System.Collections.Generic;

public enum TileState { Empty, Tilled, Planted, Watered }

[System.Serializable]
public class FarmTile
{
    public Vector3Int position;
    public TileState state;
    public CropData cropData;
    public int growthDay;
    public bool wateredToday;
    public GameObject visualObject;
}

public class FarmGrid : MonoBehaviour
{
    [SerializeField] int gridWidth = 20;
    [SerializeField] int gridHeight = 20;
    [SerializeField] float tileSize = 1f;
    [SerializeField] Transform gridOrigin;

    [Header("Tile Visuals")]
    [SerializeField] GameObject tilledSoilPrefab;
    [SerializeField] GameObject wateredSoilPrefab;
    [SerializeField] Material dirtMaterial;
    [SerializeField] Material tilledMaterial;
    [SerializeField] Material wateredMaterial;

    Dictionary<Vector3Int, FarmTile> tiles = new Dictionary<Vector3Int, FarmTile>();
    Dictionary<Vector3Int, CropVisual> cropVisuals = new Dictionary<Vector3Int, CropVisual>();

    void Start()
    {
        if (gridOrigin == null) gridOrigin = transform;
        GameManager.Instance.Time.OnDayChanged += OnNewDay;
    }

    void OnDestroy()
    {
        if (GameManager.Instance?.Time != null)
            GameManager.Instance.Time.OnDayChanged -= OnNewDay;
    }

    public Vector3Int WorldToTile(Vector3 worldPos)
    {
        Vector3 local = worldPos - gridOrigin.position;
        int x = Mathf.FloorToInt(local.x / tileSize);
        int z = Mathf.FloorToInt(local.z / tileSize);
        return new Vector3Int(x, 0, z);
    }

    public Vector3 TileToWorld(Vector3Int tile)
    {
        return gridOrigin.position + new Vector3(
            tile.x * tileSize + tileSize * 0.5f,
            0f,
            tile.z * tileSize + tileSize * 0.5f
        );
    }

    public bool IsValidTile(Vector3Int tile)
    {
        return tile.x >= 0 && tile.x < gridWidth && tile.z >= 0 && tile.z < gridHeight;
    }

    public void TillSoil(Vector3Int tile)
    {
        if (!IsValidTile(tile)) return;
        if (tiles.ContainsKey(tile) && tiles[tile].state != TileState.Empty) return;

        var farmTile = new FarmTile { position = tile, state = TileState.Tilled };
        tiles[tile] = farmTile;
        UpdateTileVisual(tile);
    }

    public void WaterTile(Vector3Int tile)
    {
        if (!tiles.ContainsKey(tile)) return;
        var farmTile = tiles[tile];
        if (farmTile.state == TileState.Empty) return;

        farmTile.wateredToday = true;
        UpdateTileVisual(tile);
    }

    public bool PlantCrop(Vector3Int tile, CropData crop)
    {
        if (!tiles.ContainsKey(tile)) return false;
        var farmTile = tiles[tile];
        if (farmTile.state != TileState.Tilled) return false;
        if (crop == null) return false;

        Season currentSeason = GameManager.Instance.CurrentSeason;
        if (!crop.CanGrowInSeason(currentSeason)) return false;

        farmTile.state = TileState.Planted;
        farmTile.cropData = crop;
        farmTile.growthDay = 0;
        tiles[tile] = farmTile;

        SpawnCropVisual(tile, crop);
        return true;
    }

    public ItemData HarvestCrop(Vector3Int tile)
    {
        if (!tiles.ContainsKey(tile)) return null;
        var farmTile = tiles[tile];
        if (farmTile.state != TileState.Planted) return null;
        if (farmTile.growthDay < farmTile.cropData.daysToGrow) return null;

        ItemData harvest = farmTile.cropData.harvestItem;
        int bonusChance = farmTile.cropData.regrowDays > 0 ? 1 : 0;

        if (farmTile.cropData.regrowDays > 0)
        {
            farmTile.growthDay = farmTile.cropData.daysToGrow - farmTile.cropData.regrowDays;
            UpdateCropVisual(tile);
        }
        else
        {
            RemoveCropVisual(tile);
            farmTile.state = TileState.Tilled;
            farmTile.cropData = null;
            farmTile.growthDay = 0;
        }

        tiles[tile] = farmTile;
        return harvest;
    }

    void OnNewDay()
    {
        var tileList = new List<Vector3Int>(tiles.Keys);
        foreach (var pos in tileList)
        {
            var tile = tiles[pos];
            if (tile.state == TileState.Planted && tile.wateredToday)
            {
                tile.growthDay++;
                UpdateCropVisual(pos);
            }
            tile.wateredToday = false;
            tiles[pos] = tile;
            UpdateTileVisual(pos);
        }
    }

    void UpdateTileVisual(Vector3Int pos)
    {
        var tile = tiles[pos];
        if (tile.visualObject == null)
        {
            GameObject prefab = tilledSoilPrefab;
            if (prefab != null)
            {
                tile.visualObject = Instantiate(prefab, TileToWorld(pos), Quaternion.identity, transform);
                tiles[pos] = tile;
            }
        }

        if (tile.visualObject != null)
        {
            var renderer = tile.visualObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = tile.wateredToday ? wateredMaterial : tilledMaterial;
            }
        }
    }

    void SpawnCropVisual(Vector3Int pos, CropData crop)
    {
        if (crop.growthStagePrefabs == null || crop.growthStagePrefabs.Length == 0) return;
        var obj = Instantiate(crop.growthStagePrefabs[0], TileToWorld(pos), Quaternion.identity, transform);
        cropVisuals[pos] = new CropVisual { gameObject = obj, currentStage = 0 };
    }

    void UpdateCropVisual(Vector3Int pos)
    {
        if (!cropVisuals.ContainsKey(pos) || !tiles.ContainsKey(pos)) return;
        var tile = tiles[pos];
        if (tile.cropData == null) return;

        float progress = (float)tile.growthDay / tile.cropData.daysToGrow;
        int stageCount = tile.cropData.growthStagePrefabs.Length;
        int stage = Mathf.Clamp(Mathf.FloorToInt(progress * stageCount), 0, stageCount - 1);

        var visual = cropVisuals[pos];
        if (visual.currentStage != stage)
        {
            Destroy(visual.gameObject);
            visual.gameObject = Instantiate(tile.cropData.growthStagePrefabs[stage], TileToWorld(pos), Quaternion.identity, transform);
            visual.currentStage = stage;
            cropVisuals[pos] = visual;
        }
    }

    void RemoveCropVisual(Vector3Int pos)
    {
        if (cropVisuals.ContainsKey(pos))
        {
            Destroy(cropVisuals[pos].gameObject);
            cropVisuals.Remove(pos);
        }
    }

    public Dictionary<Vector3Int, FarmTile> GetAllTiles() => tiles;
    public void SetTiles(Dictionary<Vector3Int, FarmTile> data) => tiles = data;
}

struct CropVisual
{
    public GameObject gameObject;
    public int currentStage;
}
