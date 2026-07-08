using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class MapConnectionView : MonoBehaviour
{
    [SerializeField] private float thickness = 6f;

    [Header("Colors")]
    [SerializeField] private Color lockedColor =
        new Color(0.25f, 0.25f, 0.25f, 0.7f);

    [SerializeField] private Color availableColor =
        new Color(0.8f, 0.8f, 0.8f, 1f);

    [SerializeField] private Color travelledColor =
        new Color(1f, 0.75f, 0.15f, 1f);

    private RectTransform rectTransform;
    private Image lineImage;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        lineImage = GetComponent<Image>();

        lineImage.raycastTarget = false;
    }

    public void Initialize(Vector2 start, Vector2 end)
    {
        Vector2 direction = end - start;
        float distance = direction.magnitude;

        rectTransform.anchorMin = new Vector2(0.5f, 0f);

        rectTransform.anchorMax = new Vector2(0.5f, 0f);

        rectTransform.pivot = new Vector2(0.5f, 0.5f);

        rectTransform.anchoredPosition = (start + end) / 2f;

        rectTransform.sizeDelta = new Vector2(distance, thickness);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void SetState(bool isAvailable, bool isTravelled)
    {
        if (isTravelled)
        {
            lineImage.color = travelledColor;
        }
        else if (isAvailable)
        {
            lineImage.color = availableColor;
        }
        else
        {
            lineImage.color = lockedColor;
        }
    }
}
