using System.Collections.Generic;
using UnityEngine;

public class DeckRuntimeManager : MonoBehaviour
{
    public static DeckRuntimeManager Instance;

    [Header("Starter Deck")] [SerializeField]
    private List<Card> starterDeck = new List<Card>();

    public List<Card> currentDeck = new List<Card>();

    public IReadOnlyList<Card> CurrentDeck => currentDeck;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            currentDeck = new List<Card>(starterDeck);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void AddCard(Card cardPrefab)
    {
        currentDeck.Add(cardPrefab);
        Debug.Log($"Added card: {cardPrefab.name}");
    }
    
    public void ResetDeckToStarter()
    {
        currentDeck = new List<Card>(starterDeck);
    }
}
