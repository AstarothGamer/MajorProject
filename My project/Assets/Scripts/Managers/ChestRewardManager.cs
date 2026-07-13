using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ChestRewardManager : MonoBehaviour
{
    public static ChestRewardManager Instance;

    [Header("Pool")]
    [SerializeField] private List<Card> rewardPool;

    [Header("UI")]
    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private Button okButton;
    [SerializeField] private Transform rewardHandLayout;
    [SerializeField] private HandLayout layout;
    [SerializeField] private DescriptionPanel descriptionPanel;

    [Header("Exit")]
    [SerializeField] private GameObject exitPanel;

    private List<Card> spawnedCards = new List<Card>();
    private Card selectedCard;

    private void Awake()
    {
        Instance = this;

        rewardPanel.SetActive(false);
        exitPanel.SetActive(false);
        okButton.interactable = false;
    }

    public void Open()
    {
        if (rewardPanel != null)
            rewardPanel.SetActive(true);

        ClearOldCards();

        List<Card> picks = GetRandomCards(3);

        foreach (Card prefab in picks)
        {
            Card card = Instantiate(prefab, rewardHandLayout);
            
            card.transform.SetParent(rewardHandLayout, false);
            card.SetBaseScale(6f);

            card.isRewardCard = true;
            card.SetOriginalPrefab(prefab);
            card.OnRewardSelected += OnCardSelected;
            card.SetSelected(false);

            spawnedCards.Add(card);
            layout.UpdateLayout(spawnedCards);
        }

        selectedCard = null;

        if (okButton != null)
            okButton.interactable = false;
    }

    private void OnCardSelected(Card card)
    {
        selectedCard = card;

        Debug.Log($"Selected: {card.name}");

        foreach (var c in spawnedCards)
        {
            c.SetSelected(c == card);
        }
        
        if (descriptionPanel != null)
        {
            descriptionPanel.Show(
                card.cardName,
                card.description,
                "",
                Vector2.zero
            );
        }

        if (okButton != null)
            okButton.interactable = true;
    }

    public void Confirm()
    {
        if (selectedCard == null) return;

        DeckRuntimeManager.Instance.AddCard(selectedCard.GetOriginalPrefab());
        
        LevelManager.Instance.CompleteCurrentLevel();

        rewardPanel.SetActive(false);
        
        exitPanel.SetActive(true);
    }
    
    private void ClearOldCards()
    {
        foreach (var card in spawnedCards)
        {
            if (card != null)
                Destroy(card.gameObject);
        }

        spawnedCards.Clear();
    }

    public void ReturnToMap()
    {
        SceneManager.LoadScene(2);
    }

    private List<Card> GetRandomCards(int count)
    {
        List<Card> copy = new List<Card>(rewardPool);
        List<Card> result = new List<Card>();

        for (int i = 0; i < count && copy.Count > 0; i++)
        {
            int index = Random.Range(0, copy.Count);
            result.Add(copy[index]);
            copy.RemoveAt(index);
        }

        return result;
    }
}
