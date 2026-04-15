using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private int levelIndex;

    [Header("References")]
    [SerializeField] private Button button;

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        if (LevelManager.Instance == null)
        {
            Debug.LogWarning("LevelProgressManager not found!");
            return;
        }

        bool unlocked = LevelManager.Instance.IsLevelUnlocked(levelIndex);

        if (button != null)
            button.interactable = unlocked;
    }

    public void LoadLevel()
    {
        if (LevelManager.Instance == null)
            return;

        if (!LevelManager.Instance.IsLevelUnlocked(levelIndex))
            return;

        SceneManager.LoadScene(levelIndex + 2);
    }
}
