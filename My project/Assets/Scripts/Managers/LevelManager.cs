using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    private const string LevelKey = "CurrentLevel";

    public int CurrentLevel { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadProgress();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void LoadProgress()
    {
        CurrentLevel = PlayerPrefs.GetInt(LevelKey, 0);
    }

    public bool IsLevelUnlocked(int levelIndex)
    {
        return levelIndex == CurrentLevel;
    }

    public void CompleteCurrentLevel()
    {
        CurrentLevel++;
        PlayerPrefs.SetInt(LevelKey, CurrentLevel);
        PlayerPrefs.Save();
    }

    public void ResetProgress()
    {
        CurrentLevel = 0;
        PlayerPrefs.SetInt(LevelKey, CurrentLevel);
        PlayerPrefs.Save();
    }
}
