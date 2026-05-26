using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] float interactRange = 2f;
    [SerializeField] LayerMask farmLayer;
    [SerializeField] LayerMask interactableLayer;
    [SerializeField] Transform toolPoint;
    [SerializeField] GameObject tileHighlight;

    FarmGrid farmGrid;
    Camera mainCam;
    Vector3Int currentTile;
    bool hasTileTarget;

    void Start()
    {
        farmGrid = FindObjectOfType<FarmGrid>();
        mainCam = Camera.main;
    }

    void Update()
    {
        UpdateTileHighlight();

        if (Input.GetMouseButtonDown(0) && hasTileTarget)
            DoInteract();
    }

    void UpdateTileHighlight()
    {
        Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 50f, farmLayer))
        {
            Vector3Int tile = farmGrid.WorldToTile(hit.point);
            if (farmGrid.IsValidTile(tile) && Vector3.Distance(transform.position, hit.point) <= interactRange)
            {
                currentTile = tile;
                hasTileTarget = true;
                if (tileHighlight != null)
                {
                    tileHighlight.SetActive(true);
                    tileHighlight.transform.position = farmGrid.TileToWorld(tile) + Vector3.up * 0.01f;
                }
                return;
            }
        }
        hasTileTarget = false;
        if (tileHighlight != null) tileHighlight.SetActive(false);
    }

    void DoInteract()
    {
        var inventory = GameManager.Instance.Inventory;
        var equipped = inventory.GetEquippedItem();
        if (equipped == null) return;

        switch (equipped.itemType)
        {
            case ItemType.Hoe:
                farmGrid.TillSoil(currentTile);
                break;
            case ItemType.WateringCan:
                farmGrid.WaterTile(currentTile);
                break;
            case ItemType.Seed:
                if (farmGrid.PlantCrop(currentTile, equipped.cropData))
                    inventory.ConsumeEquipped();
                break;
            case ItemType.Scythe:
                var harvest = farmGrid.HarvestCrop(currentTile);
                if (harvest != null)
                    inventory.AddItem(harvest, 1);
                break;
        }
    }
}
