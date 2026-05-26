using UnityEngine;
using System;

public enum Season { Spring, Summer, Autumn, Winter }

public class TimeSystem : MonoBehaviour
{
    [SerializeField] float secondsPerGameMinute = 0.5f;
    [SerializeField] int daysPerSeason = 28;
    [SerializeField] Light directionalLight;

    public int CurrentDay { get; private set; } = 1;
    public int CurrentYear { get; private set; } = 1;
    public Season CurrentSeason { get; private set; } = Season.Spring;
    public int CurrentHour { get; private set; } = 6;
    public int CurrentMinute { get; private set; }
    public bool IsDaytime => CurrentHour >= 6 && CurrentHour < 20;

    public event Action OnHourChanged;
    public event Action OnDayChanged;
    public event Action<Season> OnSeasonChanged;

    float timer;
    bool paused;

    [SerializeField] Gradient dayNightGradient;
    [SerializeField] AnimationCurve lightIntensityCurve;

    public void Pause() => paused = true;
    public void Resume() => paused = false;

    void Update()
    {
        if (paused) return;

        timer += UnityEngine.Time.deltaTime;
        if (timer >= secondsPerGameMinute)
        {
            timer -= secondsPerGameMinute;
            AdvanceMinute();
        }

        UpdateLighting();
    }

    void AdvanceMinute()
    {
        CurrentMinute++;
        if (CurrentMinute >= 60)
        {
            CurrentMinute = 0;
            CurrentHour++;
            OnHourChanged?.Invoke();

            if (CurrentHour >= 24)
            {
                CurrentHour = 0;
                AdvanceDay();
            }
        }
    }

    void AdvanceDay()
    {
        CurrentDay++;
        if (CurrentDay > daysPerSeason)
        {
            CurrentDay = 1;
            AdvanceSeason();
        }
        OnDayChanged?.Invoke();
    }

    void AdvanceSeason()
    {
        CurrentSeason = (Season)(((int)CurrentSeason + 1) % 4);
        if (CurrentSeason == Season.Spring) CurrentYear++;
        OnSeasonChanged?.Invoke(CurrentSeason);
    }

    void UpdateLighting()
    {
        if (directionalLight == null) return;

        float timeNormalized = (CurrentHour + CurrentMinute / 60f) / 24f;

        if (dayNightGradient != null)
            directionalLight.color = dayNightGradient.Evaluate(timeNormalized);

        if (lightIntensityCurve != null)
            directionalLight.intensity = lightIntensityCurve.Evaluate(timeNormalized);

        float sunAngle = Mathf.Lerp(-90f, 270f, timeNormalized);
        directionalLight.transform.rotation = Quaternion.Euler(sunAngle, -30f, 0f);
    }

    public void SetTime(int day, int hour, Season season, int year)
    {
        CurrentDay = day;
        CurrentHour = hour;
        CurrentMinute = 0;
        CurrentSeason = season;
        CurrentYear = year;
    }

    public string GetTimeString() => $"{CurrentHour:D2}:{CurrentMinute:D2}";
    public string GetDateString() => $"Year {CurrentYear} - {CurrentSeason} Day {CurrentDay}";
}
