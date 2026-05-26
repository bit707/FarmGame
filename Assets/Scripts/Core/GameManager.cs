using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public TimeSystem Time { get; private set; }
    public WeatherSystem Weather { get; private set; }
    public InventorySystem Inventory { get; private set; }
    public ShopSystem Shop { get; private set; }
    public FarmGrid Farm { get; private set; }
    public SaveSystem Save { get; private set; }

    public int Gold { get; set; } = 500;
    public int Day => Time.CurrentDay;
    public Season CurrentSeason => Time.CurrentSeason;

    public event System.Action<int> OnGoldChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        Time = GetComponentInChildren<TimeSystem>();
        Weather = GetComponentInChildren<WeatherSystem>();
        Inventory = GetComponentInChildren<InventorySystem>();
        Shop = GetComponentInChildren<ShopSystem>();
        Farm = FindObjectOfType<FarmGrid>();
        Save = GetComponentInChildren<SaveSystem>();
    }

    public void AddGold(int amount)
    {
        Gold += amount;
        OnGoldChanged?.Invoke(Gold);
    }

    public bool SpendGold(int amount)
    {
        if (Gold < amount) return false;
        Gold -= amount;
        OnGoldChanged?.Invoke(Gold);
        return true;
    }
}
