using UnityEngine;

public enum WeatherType { Sunny, Cloudy, Rainy, Stormy, Snowy }

public class WeatherSystem : MonoBehaviour
{
    [SerializeField] ParticleSystem rainParticles;
    [SerializeField] ParticleSystem snowParticles;
    [SerializeField] AudioSource weatherAudio;
    [SerializeField] AudioClip rainSound;
    [SerializeField] AudioClip stormSound;
    [SerializeField] Light sunLight;

    [Header("Settings")]
    [SerializeField] float baseRainChance = 0.2f;
    [SerializeField] float stormChance = 0.05f;

    public WeatherType CurrentWeather { get; private set; } = WeatherType.Sunny;
    public event System.Action<WeatherType> OnWeatherChanged;

    void Start()
    {
        GameManager.Instance.Time.OnDayChanged += GenerateWeather;
        ApplyWeather();
    }

    void OnDestroy()
    {
        if (GameManager.Instance?.Time != null)
            GameManager.Instance.Time.OnDayChanged -= GenerateWeather;
    }

    void GenerateWeather()
    {
        Season season = GameManager.Instance.CurrentSeason;
        float rainChance = baseRainChance;

        switch (season)
        {
            case Season.Spring: rainChance = 0.3f; break;
            case Season.Summer: rainChance = 0.15f; break;
            case Season.Autumn: rainChance = 0.35f; break;
            case Season.Winter: rainChance = 0.4f; break;
        }

        float roll = Random.value;
        if (season == Season.Winter)
        {
            CurrentWeather = roll < rainChance ? WeatherType.Snowy : WeatherType.Cloudy;
        }
        else if (roll < stormChance)
        {
            CurrentWeather = WeatherType.Stormy;
        }
        else if (roll < rainChance)
        {
            CurrentWeather = WeatherType.Rainy;
        }
        else if (roll < rainChance + 0.2f)
        {
            CurrentWeather = WeatherType.Cloudy;
        }
        else
        {
            CurrentWeather = WeatherType.Sunny;
        }

        ApplyWeather();
        OnWeatherChanged?.Invoke(CurrentWeather);

        if (CurrentWeather == WeatherType.Rainy || CurrentWeather == WeatherType.Stormy)
            WaterAllCrops();
    }

    void ApplyWeather()
    {
        if (rainParticles != null)
        {
            if (CurrentWeather == WeatherType.Rainy || CurrentWeather == WeatherType.Stormy)
            {
                var emission = rainParticles.emission;
                emission.rateOverTime = CurrentWeather == WeatherType.Stormy ? 500f : 200f;
                rainParticles.Play();
            }
            else
            {
                rainParticles.Stop();
            }
        }

        if (snowParticles != null)
        {
            if (CurrentWeather == WeatherType.Snowy)
                snowParticles.Play();
            else
                snowParticles.Stop();
        }

        if (weatherAudio != null)
        {
            if (CurrentWeather == WeatherType.Rainy && rainSound != null)
            {
                weatherAudio.clip = rainSound;
                weatherAudio.Play();
            }
            else if (CurrentWeather == WeatherType.Stormy && stormSound != null)
            {
                weatherAudio.clip = stormSound;
                weatherAudio.Play();
            }
            else
            {
                weatherAudio.Stop();
            }
        }

        if (sunLight != null)
        {
            sunLight.intensity = CurrentWeather switch
            {
                WeatherType.Sunny => 1.2f,
                WeatherType.Cloudy => 0.7f,
                WeatherType.Rainy => 0.4f,
                WeatherType.Stormy => 0.3f,
                WeatherType.Snowy => 0.6f,
                _ => 1f
            };
        }
    }

    void WaterAllCrops()
    {
        var farm = GameManager.Instance.Farm;
        if (farm == null) return;
        foreach (var kvp in farm.GetAllTiles())
        {
            if (kvp.Value.state == TileState.Planted)
                farm.WaterTile(kvp.Key);
        }
    }

    public void SetWeather(WeatherType weather)
    {
        CurrentWeather = weather;
        ApplyWeather();
        OnWeatherChanged?.Invoke(CurrentWeather);
    }
}
