using System.Collections;
using TMPro;
using UnityEngine;

public enum WheelUseMode
{
    SpinOnEveryCard,
    SpinOncePerTurn
}

public class TurnManager : MonoBehaviour
{
    public DeckManager deckManager;
    public PlayerCombat playerCombat;

    [Header("Wheel")]
    [SerializeField] private WheelOfFortune wheel;
    [SerializeField] private WheelUseMode wheelUseMode = WheelUseMode.SpinOnEveryCard;
    [SerializeField] private TMP_Text wheelModeText;

    public bool IsActionInProgress { get; private set; }

    public int CurrentTurnWheelIndex { get; private set; } = -1;
    public bool HasCurrentTurnWheelIndex { get; private set; }

    public bool UseTurnWheelForCards
    {
        get
        {
            return wheelUseMode == WheelUseMode.SpinOncePerTurn && HasCurrentTurnWheelIndex;
        }
    }

    private bool isPlayerTurn = true;

    private void Awake()
    {
        if (wheel == null)
            wheel = FindFirstObjectByType<WheelOfFortune>();
    }

    private void Start()
    {
        UpdateWheelModeText();
        StartPlayerTurn();
    }

    public void StartAction()
    {
        IsActionInProgress = true;
    }

    public void EndAction()
    {
        IsActionInProgress = false;
    }

    public void ToggleWheelUseMode()
    {
        if (IsActionInProgress)
            return;

        if (wheelUseMode == WheelUseMode.SpinOnEveryCard)
        {
            wheelUseMode = WheelUseMode.SpinOncePerTurn;
        }
        else
        {
            wheelUseMode = WheelUseMode.SpinOnEveryCard;
        }

        CurrentTurnWheelIndex = -1;
        HasCurrentTurnWheelIndex = false;

        UpdateWheelModeText();

        Debug.Log($"Wheel mode changed to: {wheelUseMode}");

        if (isPlayerTurn && wheelUseMode == WheelUseMode.SpinOncePerTurn)
        {
            StartCoroutine(RollWheelForCurrentTurn());
        }
    }

    private void UpdateWheelModeText()
    {
        if (wheelModeText == null)
            return;

        switch (wheelUseMode)
        {
            case WheelUseMode.SpinOnEveryCard:
                wheelModeText.text = "Wheel: Every card";
                break;

            case WheelUseMode.SpinOncePerTurn:
                wheelModeText.text = "Wheel: Once per turn";
                break;
        }
    }

    private IEnumerator RollWheelForCurrentTurn()
    {
        if (wheel == null)
        {
            Debug.LogWarning("WheelOfFortune was not found.");
            yield break;
        }

        IsActionInProgress = true;

        int result = -1;

        yield return StartCoroutine(wheel.SpinAndWait(index => { result = index; }));

        CurrentTurnWheelIndex = result;
        HasCurrentTurnWheelIndex = true;

        Debug.Log($"Turn wheel result: {CurrentTurnWheelIndex}");

        IsActionInProgress = false;
    }

    public void EndPlayerTurn()
    {
        if (!isPlayerTurn) return;

        if (IsActionInProgress) return;

        StartCoroutine(EnemyTurnRoutine());
    }

    private void StartPlayerTurn()
    {
        StartCoroutine(StartPlayerTurnRoutine());
    }

    private IEnumerator StartPlayerTurnRoutine()
    {
        Debug.Log("=== player turn ===");

        isPlayerTurn = true;

        CurrentTurnWheelIndex = -1;
        HasCurrentTurnWheelIndex = false;

        IsActionInProgress = true;

        playerCombat.ResetEnergy();
        deckManager.DrawCards(deckManager.cardsPerTurn);

        if (wheelUseMode == WheelUseMode.SpinOncePerTurn)
        {
            if (wheel != null)
            {
                int result = -1;

                yield return StartCoroutine(wheel.SpinAndWait(index => { result = index; }));

                CurrentTurnWheelIndex = result;
                HasCurrentTurnWheelIndex = true;

                Debug.Log($"Wheel result for this turn: {CurrentTurnWheelIndex}");
            }
            else
            {
                Debug.LogWarning("WheelOfFortune was not found.");
            }
        }

        IsActionInProgress = false;
    }

    private IEnumerator EnemyTurnRoutine()
    {
        Debug.Log("=== enemy turn ===");

        isPlayerTurn = false;

        CurrentTurnWheelIndex = -1;
        HasCurrentTurnWheelIndex = false;

        deckManager.EndTurn();

        yield return new WaitForSeconds(0.5f);

        EnemyUnit[] enemies = FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None);

        foreach (EnemyUnit enemy in enemies)
        {
            if (enemy != null)
            {
                yield return StartCoroutine(enemy.TakeTurnRoutine(playerCombat));
            }
        }

        foreach (EnemyUnit enemy in enemies)
        {
            if (enemy != null)
            {
                enemy.NextTurn();
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