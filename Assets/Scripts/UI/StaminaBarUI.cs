using UnityEngine;
using UnityEngine.UI;

public class StaminaBarUI : MonoBehaviour
{
    [SerializeField] Image fillImage;
    [SerializeField] Gradient colorGradient;

    void Start()
    {
        var stamina = FindObjectOfType<PlayerStamina>();
        if (stamina != null)
            stamina.OnStaminaChanged += UpdateBar;
        UpdateBar(1f);
    }

    void UpdateBar(float percent)
    {
        if (fillImage == null) return;
        fillImage.fillAmount = percent;
        if (colorGradient != null)
            fillImage.color = colorGradient.Evaluate(percent);
    }
}
