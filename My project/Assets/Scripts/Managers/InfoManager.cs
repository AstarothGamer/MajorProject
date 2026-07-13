using TMPro;
using UnityEngine;

public class InfoManager : MonoBehaviour
{
    [Header("Texts")]
    [SerializeField] public TMP_Text currentLevel;
    [SerializeField] public TMP_Text currentAvailableLevel;
    [SerializeField] public TMP_Text drawPileCount;
    [SerializeField] public TMP_Text discardPileCount;
    [SerializeField] public TMP_Text playerHealth;
    [SerializeField] public TMP_Text playerEnergy;
    
    [Header("References")]
    [SerializeField] private DeckManager deckManager;
    [SerializeField] private PlayerCombat player;

    void Start()
    {
        if(currentLevel != null)
        {
            string level = RunMapManager.Instance.CurrentLevel.ToString();
            currentLevel.text = level;
        }

        if (currentAvailableLevel != null)
        {
            string level = RunMapManager.Instance.CurrentAvailableLevel.ToString();
            currentAvailableLevel.text = level;
        }
    }

    void Update()
    {
        if (drawPileCount != null)
        {
            drawPileCount.text = deckManager.DrawPileCount.ToString();
        }
        
        if (discardPileCount != null)
        {
            discardPileCount.text = deckManager.DiscardPileCount.ToString();
        }

        if (playerHealth != null)
        {
            playerHealth.text = PlayerRuntimeManager.Instance.currentHp.ToString() + '/' + PlayerRuntimeManager.Instance.MaxHp.ToString();
        }

        if (playerEnergy != null)
        {
            playerEnergy.text = player.CurrentEnergy.ToString();
        }
    }
}
