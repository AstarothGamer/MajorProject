using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StoryManager : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] private TMP_Text storyText;

    [Header("Story Parts")]
    [TextArea(3, 10)]
    [SerializeField] private List<string> storyParts = new List<string>();

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.03f;

    [Header("Next Scene")]
    [SerializeField] private string nextSceneName;

    private int currentIndex = 0;
    private Coroutine typingCoroutine;
    private bool isTyping = false;

    private void Start()
    {
        ShowCurrentPart();
    }

    public void OnSkipPressed()
    {
        if (isTyping)
        {
            StopCoroutine(typingCoroutine);
            storyText.text = storyParts[currentIndex];
            isTyping = false;
        }
        else
        {
            NextPart();
        }
    }

    private void ShowCurrentPart()
    {
        if (currentIndex >= storyParts.Count)
        {
            LoadNextScene();
            return;
        }

        typingCoroutine = StartCoroutine(TypeText(storyParts[currentIndex]));
    }

    private IEnumerator TypeText(string text)
    {
        storyText.text = "";
        isTyping = true;

        foreach (char c in text)
        {
            storyText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void NextPart()
    {
        currentIndex++;
        ShowCurrentPart();
    }

    private void LoadNextScene()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}