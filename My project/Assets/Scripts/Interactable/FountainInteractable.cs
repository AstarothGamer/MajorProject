using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FountainInteractable : MonoBehaviour
{
    [SerializeField] private int healPersentage;
    [SerializeField] private PlayerCombat playerCombat;
    [SerializeField] private GameObject panel;
    private bool healed = false;

    private void OnMouseDown()
    {
        if (healed) return;

        healed = true;
        
        playerCombat.Heal(healPersentage * PlayerRuntimeManager.Instance.MaxHp / 100);
        StartCoroutine(ActivatePanel());
    }

    private IEnumerator ActivatePanel()
    {
        yield return new WaitForSeconds(2f);
        panel.SetActive(true);
    }

    public void LoadScene()
    {
        SceneManager.LoadScene(1);
    }
}
