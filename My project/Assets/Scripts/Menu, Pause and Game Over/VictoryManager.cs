using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class VictoryManager : MonoBehaviour
{
    public static VictoryManager Instance;

    [SerializeField] private GameObject victoryPanel;

    private void Awake()
    {
        Instance = this;

        if (victoryPanel != null)
            victoryPanel.SetActive(false);
    }

    public void CheckVictory()
    {
        EnemyUnit[] enemies = FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None);

        if (enemies.Length == 0)
        {
            ShowVictory();
        }
    }

    private void ShowVictory()
    {
        Debug.Log("VICTORY!");
        
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.CompleteCurrentLevel();
        }

        if (victoryPanel != null)
            victoryPanel.SetActive(true);
    }
    
    public void CheckVictoryDelayed()
    {
        StartCoroutine(CheckNextFrame());
    }

    private IEnumerator CheckNextFrame()
    {
        yield return null;

        EnemyUnit[] enemies = FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None);

        if (enemies.Length == 0)
        {
            ShowVictory();
        }
    }

    public void GoToMap()
    {
        SceneManager.LoadScene(1);
    }
}
