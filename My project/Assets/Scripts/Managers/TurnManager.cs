using System.Collections;
using UnityEngine;

public class TurnManager : MonoBehaviour
{
    public DeckManager deckManager;
    public PlayerCombat playerCombat;

    private bool isPlayerTurn = true;

    private void Start()
    {
        StartPlayerTurn();
    }

    public void EndPlayerTurn()
    {
        if (!isPlayerTurn) return;

        StartCoroutine(EnemyTurnRoutine());
    }

    private void StartPlayerTurn()
    {
        Debug.Log("=== player turn ===");

        isPlayerTurn = true;

        playerCombat.ResetEnergy();
        deckManager.DrawCards(deckManager.cardsPerTurn);
    }

    private IEnumerator EnemyTurnRoutine()
    {
        Debug.Log("=== enemy turn ===");

        isPlayerTurn = false;

        deckManager.EndTurn();

        yield return new WaitForSeconds(0.5f);

        EnemyUnit[] enemies = FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None);

        foreach (EnemyUnit enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.TakeTurn(playerCombat);
                yield return new WaitForSeconds(0.5f);
            }
        }

        playerCombat.ResetShieldAtEndTurn();

        yield return new WaitForSeconds(0.5f);

        StartPlayerTurn();
    }
    
    public bool IsPlayerTurn()
    {
        return isPlayerTurn;
    }
}