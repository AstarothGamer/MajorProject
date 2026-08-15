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
    public Transform discardZone;
    public Transform passivePileZone;

    private List<Card> drawPile = new List<Card>();
    private List<Card> discardPile = new List<Card>();
    private List<Card> passivePile = new List<Card>();
    private List<Card> hand = new List<Card>();
    
    public int PassivePileCount => passivePile.Count;

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
        
        card.PrepareForHand();

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
        card.transform.SetParent(discardZone, false);

        discardPile.Add(card);
    }
    
    private void MoveToPassivePile(Card card)
    {
        if (card == null)
            return;

        card.ResetCardState();

        card.gameObject.SetActive(false);

        if (passivePileZone != null)
            card.transform.SetParent(passivePileZone, false);

        passivePile.Add(card);

        Debug.Log($"Card [{card.cardName}] moved to PassivePile.");
    }

    public void OnCardPlayed(Card card)
    {
        if (hand.Contains(card))
            hand.Remove(card);

        if (card.cardType == CardType.Passive)
        {
            MoveToPassivePile(card);
        }
        else
        {
            MoveToDiscard(card);
        }

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
            Card newCard = Instantiate(prefab, handZone);
            newCard.gameObject.SetActive(false);
            drawPile.Add(newCard);
        }

        Shuffle(drawPile);
    }

    public int DrawPileCount
    {
        get
        {
            return drawPile.Count;
        }
    }
    
    public int DiscardPileCount
    {
        get
        {
            return discardPile.Count;
        }
    }
}
