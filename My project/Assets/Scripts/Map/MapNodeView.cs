using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class MapNodeView : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Button button;
    [SerializeField] private Image background;
    [SerializeField] private Image icon;
    [SerializeField] private GameObject currentMarker;

    [Header("State Colors")]
    [SerializeField] private Color lockedColor =
        new Color(0.35f, 0.35f, 0.35f, 0.7f);

    [SerializeField] private Color availableColor =
        Color.white;

    [SerializeField] private Color visitedColor =
        new Color(0.55f, 0.55f, 0.55f, 1f);

    [SerializeField] private Color currentColor =
        new Color(1f, 0.8f, 0.2f, 1f);

    public int NodeId { get; private set; }

    public RectTransform RectTransform { get; private set; }

    private Action<int> onClicked;

    private void Awake()
    {
        RectTransform = GetComponent<RectTransform>();

        if (button == null)
            button = GetComponent<Button>();

        if (background == null)
            background = GetComponent<Image>();
    }

    public void Initialize(
        MapNodeData nodeData,
        Sprite nodeIcon,
        Action<int> clickCallback)
    {
        NodeId = nodeData.id;
        onClicked = clickCallback;

        if (icon != null)
            icon.sprite = nodeIcon;

        button.onClick.RemoveListener(HandleClick);
        button.onClick.AddListener(HandleClick);
    }

    public void SetState(
        bool isAvailable,
        bool isVisited,
        bool isCurrent)
    {
        if (currentMarker != null)
            currentMarker.SetActive(isCurrent);

        if (isCurrent)
        {
            button.interactable = false;
            background.color = currentColor;
            SetIconDark(1f);
            return;
        }

        if (isAvailable)
        {
            button.interactable = true;
            background.color = availableColor;
            SetIconDark(1f);
            return;
        }

        if (isVisited)
        {
            button.interactable = false;
            background.color = visitedColor;
            SetIconDark(0.85f);
            return;
        }

        button.interactable = false;
        background.color = lockedColor;
        SetIconDark(0.532f);
    }

    private void SetIconDark(float number)
    {
        if (icon == null)
            return;

        Color color = icon.color;
        color.r = number;
        color.g = number;
        color.b = number;
        icon.color = color;
    }

    private void HandleClick()
    {
        onClicked?.Invoke(NodeId);
    }

    private void OnDestroy()
    {
        if (button != null)
            button.onClick.RemoveListener(HandleClick);
    }
}
