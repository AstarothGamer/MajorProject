using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance;

    public int CurrentLevel { get; private set; } = 0;

    private void Awake()
    {
        ResetProgress();
        
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        CurrentLevel = 0;
    }

    public bool IsLevelUnlocked(int levelIndex)
    {
        return levelIndex == CurrentLevel;
    }

    public void CompleteCurrentLevel()
    {
        CurrentLevel++;
        Debug.Log($"Level completed. CurrentLevel = {CurrentLevel}");
    }

    public void ResetProgress()
    {
        CurrentLevel = 0;
        Debug.Log("Progress reset");
    }
}
