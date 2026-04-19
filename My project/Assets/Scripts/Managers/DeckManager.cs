using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    [Header("Deck Setup")]
    [Tooltip("Card prefabs")]
    public List<Card> startingDeckPrefabs = new List<Card>();

    [Header("Draw Settings")]
    public int cardsPerTurn = 5;

    [Header("References")]
    public Transform handZone;
    public HandLayout handLayout;

    private List<Card> drawPile = new List<Card>();
    private List<Card> discardPile = new List<Card>();
    private List<Card> hand = new List<Card>();

    private void Start()
    {
        InitializeFromRuntimeDeck();
        DrawCards(cardsPerTurn);
    }

    public void InitializeDeck()
    {
        drawPile.Clear();
        discardPile.Clear();
        hand.Clear();

        foreach (Card prefab in startingDeckPrefabs)
        {
            Card newCard = Instantiate(prefab, handZone.parent);
            newCard.gameObject.SetActive(false);
            drawPile.Add(newCard);
        }

        Shuffle(drawPile);
    }

    public void DrawCards(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            if (drawPile.Count == 0)
            {
                ReshuffleDiscardIntoDeck();

                if (drawPile.Count == 0)
                {
                    Debug.Log("Have nor cards in deck");
                    return;
                }
            }

            Card card = drawPile[0];
            drawPile.RemoveAt(0);

            AddToHand(card);
        }
    }

    private void AddToHand(Card card)
    {
        card.gameObject.SetActive(true);
        card.transform.SetParent(handZone, false);

        hand.Add(card);
        handLayout.UpdateLayout(hand);
    }

    public void EndTurn()
    {
        Debug.Log("End turn");

        foreach (Card card in hand)
        {
            MoveToDiscard(card);
        }

        hand.Clear();
    }

    public void MoveToDiscard(Card card)
    {
        card.gameObject.SetActive(false);
        card.transform.SetParent(transform);

        discardPile.Add(card);
    }

    public void OnCardPlayed(Card card)
    {
        if (hand.Contains(card))
            hand.Remove(card);

        MoveToDiscard(card);
        handLayout.UpdateLayout(hand);
    }

    private void ReshuffleDiscardIntoDeck()
    {
        Debug.Log("Shuffle discard to deck");

        drawPile.AddRange(discardPile);
        discardPile.Clear();

        Shuffle(drawPile);
    }

    private void Shuffle(List<Card> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randomIndex = Random.Range(i, list.Count);
            (list[i], list[randomIndex]) = (list[randomIndex], list[i]);
        }
    }
    
    public void InitializeFromRuntimeDeck()
    {
        drawPile.Clear();
        discardPile.Clear();
        hand.Clear();

        foreach (Card prefab in DeckRuntimeManager.Instance.CurrentDeck)
        {
            Card newCard = Instantiate(prefab, transform);
            newCard.gameObject.SetActive(false);
            drawPile.Add(newCard);
        }

        Shuffle(drawPile);
    }
}
