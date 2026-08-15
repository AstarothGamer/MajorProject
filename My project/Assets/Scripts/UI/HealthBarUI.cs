using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [Header("Health Fill")]
    [SerializeField] private Image fillImage;
    [SerializeField] private bool smoothFill = true;
    [SerializeField] private float fillSpeed = 8f;

    [Header("Low HP Glow")]
    [SerializeField] private Image lowHpGlowImage;
    [SerializeField] private float lowHpThreshold = 0.25f;

    [SerializeField] private Color glowColor = Color.red;
    [SerializeField] private float glowFrequency = 4f;

    [Range(0f, 1f)]
    [SerializeField] private float minGlowAlpha = 0.2f;

    [Range(0f, 1f)]
    [SerializeField] private float maxGlowAlpha = 1f;

    private float targetFill = 1f;
    private bool lowHpGlowActive;

    private void Awake()
    {
        DisableLowHpGlow();
    }

    private void Update()
    {
        UpdateFill();
        UpdateLowHpGlow();
    }

    private void UpdateFill()
    {
        if (!smoothFill || fillImage == null)
            return;

        fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFill, Time.deltaTime * fillSpeed);
    }

    private void UpdateLowHpGlow()
    {
        if (!lowHpGlowActive || lowHpGlowImage == null)
            return;

        float pulse = Mathf.PingPong(Time.time * glowFrequency, 1f);

        float alpha = Mathf.Lerp(minGlowAlpha, maxGlowAlpha, pulse);

        Color color = glowColor;
        color.a = alpha;

        lowHpGlowImage.color = color;
    }

    public void SetValue(int currentHp, int maxHp)
    {
        if (fillImage == null)
            return;

        if (maxHp <= 0)
        {
            targetFill = 0f;
            fillImage.fillAmount = 0f;
            UpdateLowHpState();
            return;
        }

        targetFill = Mathf.Clamp01((float)currentHp / maxHp);

        if (!smoothFill)
            fillImage.fillAmount = targetFill;

        UpdateLowHpState();
    }

    public void SetValueInstant(int currentHp, int maxHp)
    {
        if (fillImage == null)
            return;

        if (maxHp <= 0)
        {
            targetFill = 0f;
            fillImage.fillAmount = 0f;
            UpdateLowHpState();
            return;
        }

        targetFill = Mathf.Clamp01((float)currentHp / maxHp);
        fillImage.fillAmount = targetFill;

        UpdateLowHpState();
    }

    private void UpdateLowHpState()
    {
        if (targetFill > 0f && targetFill <= lowHpThreshold)
        {
            EnableLowHpGlow();
        }
        else
        {
            DisableLowHpGlow();
        }
    }

    private void EnableLowHpGlow()
    {
        if (lowHpGlowImage == null)
            return;

        lowHpGlowActive = true;
        lowHpGlowImage.enabled = true;

        Color color = glowColor;
        color.a = maxGlowAlpha;
        lowHpGlowImage.color = color;
    }

    private void DisableLowHpGlow()
    {
        lowHpGlowActive = false;

        if (lowHpGlowImage == null)
            return;

        lowHpGlowImage.enabled = false;
    }
}
