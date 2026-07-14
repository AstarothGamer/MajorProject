using System.Collections;
using TMPro;
using UnityEngine;

public class PlayedCardNotification : MonoBehaviour
{
    public static PlayedCardNotification Instance { get; private set; }

    [SerializeField] private TMP_Text notificationText;
    [SerializeField] private CanvasGroup canvasGroup;

    [SerializeField] private float showTime = 1.5f;
    [SerializeField] private float fadeTime = 0.3f;

    private Coroutine currentRoutine;

    private void Awake()
    {
        Instance = this;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        HideInstant();
    }

    public void Show(string message)
    {
        if (notificationText == null || canvasGroup == null)
            return;

        if (currentRoutine != null)
            StopCoroutine(currentRoutine);

        currentRoutine = StartCoroutine(ShowRoutine(message));
    }

    private IEnumerator ShowRoutine(string message)
    {
        notificationText.text = message;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = false;

        yield return new WaitForSeconds(showTime);

        float timer = 0f;

        while (timer < fadeTime)
        {
            timer += Time.deltaTime;

            canvasGroup.alpha = Mathf.Lerp(1f, 0f, timer / fadeTime);

            yield return null;
        }

        HideInstant();
    }

    private void HideInstant()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
