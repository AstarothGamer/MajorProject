using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class FloatingCombatText : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    [SerializeField] private float lifetime = 1.2f;
    [SerializeField] private float moveDownDistance = 80f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (text == null)
            text = GetComponentInChildren<TMP_Text>();
    }

    public void Initialize(string message, Color color)
    {
        text.text = message;
        text.color = color;

        StartCoroutine(AnimationRoutine());
    }

    private IEnumerator AnimationRoutine()
    {
        Vector2 startPosition = rectTransform.anchoredPosition;
        Vector2 endPosition = startPosition + Vector2.down * moveDownDistance;

        float timer = 0f;

        while (timer < lifetime)
        {
            timer += Time.deltaTime;

            float t = timer / lifetime;

            rectTransform.anchoredPosition = Vector2.Lerp(
                startPosition,
                endPosition,
                t
            );

            canvasGroup.alpha = Mathf.Lerp(
                1f,
                0f,
                t
            );

            yield return null;
        }

        Destroy(gameObject);
    }
}
