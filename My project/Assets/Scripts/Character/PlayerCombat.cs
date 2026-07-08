using TMPro;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Player Stats")]
    public int maxHp = 100;

    [Header("Energy")]
    public int maxEnergy = 3;
    [SerializeField] private int currentEnergy = 3;
    public int CurrentEnergy => currentEnergy;

    [Header("Shield")]
    public int shieldForCurrentTurn = 0;

    [Header("Stats")] 
    [SerializeField] private TMP_Text hp;
    [SerializeField] private TMP_Text shield;
    [SerializeField] private TMP_Text energy;
    
    [Header("VFX and Audio")]
    [SerializeField] private GameObject particleEffect;
    
    [SerializeField] GameOver gameOver;

    void Awake()
    {
        gameOver = FindFirstObjectByType<GameOver>();
    }

    void Start()
    {
        ResetEnergy();
        hp.text = $"HP: {PlayerRuntimeManager.Instance.currentHp}";
        shield.text = $"Shield: {shieldForCurrentTurn}";
        energy.text = $"Energy: {currentEnergy}";
    }
    
    public void SpendEnergy(int value)
    {
        currentEnergy -= value;
        if (currentEnergy < 0)
            currentEnergy = 0;

        energy.text = $"Energy: {currentEnergy}";
        Debug.Log($"Player spent {value} energy. current energy: {currentEnergy}/{maxEnergy}");
    }

    public void GainEnergy(int value)
    {
        currentEnergy += value;
        if (currentEnergy > maxEnergy)
            currentEnergy = maxEnergy;

        energy.text = $"Energy: {currentEnergy}";
        Debug.Log($"Player got {value} energy. current energy: {currentEnergy}/{maxEnergy}");
    }

    public void AddShieldForOneTurn(int value)
    {
        shieldForCurrentTurn += value;
        shield.text = $"Shield: {shieldForCurrentTurn}";
        Debug.Log($"Player got {value} shield for this turn. Shield: {shieldForCurrentTurn}");
    }

    public void Heal(int value)
    {
        PlayerRuntimeManager.Instance.Heal(value);
        
        hp.text = $"HP: {PlayerRuntimeManager.Instance.currentHp}";
        Debug.Log($"Player got {value} HP. HP: {PlayerRuntimeManager.Instance.currentHp}/{maxHp}");
    }

    public void TakeDamage(int value)
    {
        int damageLeft = value;

        if (shieldForCurrentTurn > 0)
        {
            int absorbed = Mathf.Min(shieldForCurrentTurn, damageLeft);
            shieldForCurrentTurn -= absorbed;
            damageLeft -= absorbed;
            shield.text = $"Shield: {shieldForCurrentTurn}";
        }

        if (damageLeft > 0)
        {
            PlayerRuntimeManager.Instance.TakeDamage(damageLeft);
            hp.text = $"HP: {PlayerRuntimeManager.Instance.currentHp}";
        }

        Debug.Log($"Player got {value} damage. HP: {PlayerRuntimeManager.Instance.currentHp}/{maxHp}, Shield: {shieldForCurrentTurn}");
    }

    public void ResetShieldAtEndTurn()
    {
        shieldForCurrentTurn = 0;
        shield.text = $"Shield: {shieldForCurrentTurn}";
        Debug.Log("player's shield reset.");
    }
    
    public void ResetEnergy()
    {
        currentEnergy = maxEnergy;
        energy.text = $"Energy: {currentEnergy}";
        Debug.Log($"Energy reset: {currentEnergy}/{maxEnergy}");
    }
}
