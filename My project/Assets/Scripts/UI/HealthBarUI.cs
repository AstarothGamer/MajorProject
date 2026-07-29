using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    [SerializeField] private bool smoothFill = true;
    [SerializeField] private float fillSpeed = 8f;

    private float targetFill = 1f;

    private void Update()
    {
        if (!smoothFill || fillImage == null)
            return;

        fillImage.fillAmount = Mathf.Lerp(fillImage.fillAmount, targetFill, Time.deltaTime * fillSpeed);
    }

    public void SetValue(int currentHp, int maxHp)
    {
        if (fillImage == null)
            return;

        if (maxHp <= 0)
        {
            targetFill = 0f;
            fillImage.fillAmount = 0f;
            return;
        }

        targetFill = Mathf.Clamp01((float)currentHp / maxHp);

        if (!smoothFill)
            fillImage.fillAmount = targetFill;
    }

    public void SetValueInstant(int currentHp, int maxHp)
    {
        if (fillImage == null)
            return;

        if (maxHp <= 0)
        {
            targetFill = 0f;
            fillImage.fillAmount = 0f;
            return;
        }

        targetFill = Mathf.Clamp01((float)currentHp / maxHp);
        fillImage.fillAmount = targetFill;
    }
}
