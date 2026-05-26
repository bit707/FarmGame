using UnityEngine;

public class StarterInventory : MonoBehaviour
{
    [SerializeField] ItemData hoe;
    [SerializeField] ItemData wateringCan;
    [SerializeField] ItemData scythe;
    [SerializeField] ItemData[] starterSeeds;
    [SerializeField] int starterSeedCount = 15;

    void Start()
    {
        var inv = GameManager.Instance.Inventory;
        if (hoe != null) inv.AddItem(hoe, 1);
        if (wateringCan != null) inv.AddItem(wateringCan, 1);
        if (scythe != null) inv.AddItem(scythe, 1);

        if (starterSeeds != null)
        {
            foreach (var seed in starterSeeds)
            {
                if (seed != null) inv.AddItem(seed, starterSeedCount);
            }
        }
    }
}
