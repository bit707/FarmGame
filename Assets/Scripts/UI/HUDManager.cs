using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    [Header("Time & Date")]
    [SerializeField] TextMeshProUGUI timeText;
    [SerializeField] TextMeshProUGUI dateText;
    [SerializeField] TextMeshProUGUI seasonText;
    [SerializeField] Image weatherIcon;

    [Header("Gold")]
    [SerializeField] TextMeshProUGUI goldText;

    [Header("Hotbar")]
    [SerializeField] Transform hotbarContainer;
    [SerializeField] GameObject hotbarSlotPrefab;

    [Header("Weather Icons")]
    [SerializeField] Sprite sunnyIcon;
    [SerializeField] Sprite cloudyIcon;
    [SerializeField] Sprite rainyIcon;
    [SerializeField] Sprite stormyIcon;
    [SerializeField] Sprite snowyIcon;

    HotbarSlotUI[] hotbarSlots;

    void Start()
    {
        var gm = GameManager.Instance;
        gm.Time.OnHourChanged += UpdateTime;
        gm.Time.OnDayChanged += UpdateDate;
        gm.Time.OnSeasonChanged += _ => UpdateSeason();
        gm.OnGoldChanged += UpdateGold;
        gm.Inventory.OnInventoryChanged += UpdateHotbar;
        gm.Inventory.OnEquippedChanged += HighlightSlot;
        gm.Weather.OnWeatherChanged += UpdateWeatherIcon;

        InitHotbar();
        UpdateAll();
    }

    void InitHotbar()
    {
        int size = GameManager.Instance.Inventory.HotbarSize;
        hotbarSlots = new HotbarSlotUI[size];
        for (int i = 0; i < size; i++)
        {
            var obj = Instantiate(hotbarSlotPrefab, hotbarContainer);
            hotbarSlots[i] = obj.GetComponent<HotbarSlotUI>();
            hotbarSlots[i].Init(i);
        }
    }

    void UpdateAll()
    {
        UpdateTime();
        UpdateDate();
        UpdateSeason();
        UpdateGold(GameManager.Instance.Gold);
        UpdateHotbar();
        UpdateWeatherIcon(GameManager.Instance.Weather.CurrentWeather);
    }

    void UpdateTime()
    {
        if (timeText != null)
            timeText.text = GameManager.Instance.Time.GetTimeString();
    }

    void UpdateDate()
    {
        if (dateText != null)
            dateText.text = $"Day {GameManager.Instance.Time.CurrentDay}";
    }

    void UpdateSeason()
    {
        if (seasonText != null)
            seasonText.text = GameManager.Instance.Time.CurrentSeason.ToString();
    }

    void UpdateGold(int amount)
    {
        if (goldText != null)
            goldText.text = $"{amount}G";
    }

    void UpdateHotbar()
    {
        var inv = GameManager.Instance.Inventory;
        for (int i = 0; i < hotbarSlots.Length; i++)
            hotbarSlots[i].UpdateSlot(inv.Slots[i]);
    }

    void HighlightSlot(int index)
    {
        for (int i = 0; i < hotbarSlots.Length; i++)
            hotbarSlots[i].SetSelected(i == index);
    }

    void UpdateWeatherIcon(WeatherType weather)
    {
        if (weatherIcon == null) return;
        weatherIcon.sprite = weather switch
        {
            WeatherType.Sunny => sunnyIcon,
            WeatherType.Cloudy => cloudyIcon,
            WeatherType.Rainy => rainyIcon,
            WeatherType.Stormy => stormyIcon,
            WeatherType.Snowy => snowyIcon,
            _ => sunnyIcon
        };
    }

    void Update()
    {
        for (int i = 0; i < 9; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
                GameManager.Instance.Inventory.SetEquipped(i);
        }
    }
}
