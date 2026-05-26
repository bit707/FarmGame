using UnityEngine;
using System;

public class PlayerStamina : MonoBehaviour
{
    [SerializeField] float maxStamina = 100f;
    [SerializeField] float staminaRegenRate = 0.5f;
    [SerializeField] float toolCost = 5f;

    public float CurrentStamina { get; private set; }
    public float MaxStamina => maxStamina;
    public float StaminaPercent => CurrentStamina / maxStamina;
    public bool IsExhausted => CurrentStamina <= 0f;

    public event Action<float> OnStaminaChanged;

    void Awake()
    {
        CurrentStamina = maxStamina;
    }

    void Start()
    {
        GameManager.Instance.Time.OnDayChanged += RestoreStamina;
    }

    public bool UseStamina(float amount)
    {
        if (CurrentStamina < amount) return false;
        CurrentStamina -= amount;
        OnStaminaChanged?.Invoke(StaminaPercent);
        return true;
    }

    public bool UseTool()
    {
        return UseStamina(toolCost);
    }

    void RestoreStamina()
    {
        CurrentStamina = maxStamina;
        OnStaminaChanged?.Invoke(StaminaPercent);
    }

    void Update()
    {
        if (CurrentStamina < maxStamina && !IsExhausted)
        {
            CurrentStamina = Mathf.Min(maxStamina, CurrentStamina + staminaRegenRate * Time.deltaTime);
            OnStaminaChanged?.Invoke(StaminaPercent);
        }
    }
}
