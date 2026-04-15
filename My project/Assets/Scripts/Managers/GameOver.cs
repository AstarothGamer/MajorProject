using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public GameObject gameOverPanel;

    public void Show()
    {
        gameOverPanel.SetActive(true);
    }

    public void Restart()
    {
        SceneManager.LoadScene(1);
    }

    public void Quit()
    {
        SceneManager.LoadScene(0);
    }
}
