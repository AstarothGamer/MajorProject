using UnityEngine;

public class FloatingCombatTextManager : MonoBehaviour
{
    public static FloatingCombatTextManager Instance { get; private set; }

    [Header("References")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform canvasRect;
    [SerializeField] private FloatingCombatText textPrefab;

    [Header("Offsets")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 2f, 0f);

    private Camera mainCamera;

    private void Awake()
    {
        Instance = this;

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        if (canvasRect == null && canvas != null)
            canvasRect = canvas.GetComponent<RectTransform>();

        mainCamera = Camera.main;
    }

    public void SpawnText(string message, Vector3 worldPosition, Color color)
    {
        if (textPrefab == null || canvasRect == null)
            return;

        Vector3 screenPosition = mainCamera.WorldToScreenPoint(worldPosition + worldOffset);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosition, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera, out Vector2 localPoint);

        FloatingCombatText text = Instantiate(textPrefab, canvasRect);

        RectTransform rect = text.GetComponent<RectTransform>();
        rect.anchoredPosition = localPoint;

        text.Initialize(message, color);
    }

    public void SpawnShieldDamage(int amount, Vector3 worldPosition)
    {
        if (amount <= 0) return;

        SpawnText($"Shield -{amount}", worldPosition, new Color(0.35f, 0.75f, 1f));
    }

    public void SpawnHpDamage(int amount, Vector3 worldPosition)
    {
        if (amount <= 0) return;

        SpawnText($"HP -{amount}", worldPosition, new Color(1f, 0.25f, 0.25f));
    }

    public void SpawnHeal(int amount, Vector3 worldPosition)
    {
        if (amount <= 0) return;

        SpawnText($"HP +{amount}", worldPosition, new Color(0.3f, 1f, 0.45f));
    }
}
