using UnityEngine;
using UnityEngine.SceneManagement;

public class BackToMainMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void MainMenu()
    {
        PlayerRuntimeManager.Instance.ResetStats();
        DeckRuntimeManager.Instance.ResetDeckToStarter();
        LevelManager.Instance.ResetProgress();
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }
}