using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Player Stats")]
    public int maxHp = 100;
    public int currentHp = 100;

    [Header("Energy")]
    public int maxEnergy = 3;
    [SerializeField] private int currentEnergy = 3;
    public int CurrentEnergy => currentEnergy;

    [Header("Shield")]
    public int shieldForCurrentTurn = 0;

    public void SpendEnergy(int value)
    {
        currentEnergy -= value;
        if (currentEnergy < 0)
            currentEnergy = 0;

        Debug.Log($"Игрок потратил {value} энергии. Текущая энергия: {currentEnergy}/{maxEnergy}");
    }

    public void GainEnergy(int value)
    {
        currentEnergy += value;
        if (currentEnergy > maxEnergy)
            currentEnergy = maxEnergy;

        Debug.Log($"Игрок получил {value} энергии. Текущая энергия: {currentEnergy}/{maxEnergy}");
    }

    public void AddShieldForOneTurn(int value)
    {
        shieldForCurrentTurn += value;
        Debug.Log($"Игрок получил {value} щита на 1 ход. Щит: {shieldForCurrentTurn}");
    }

    public void Heal(int value)
    {
        currentHp += value;
        if (currentHp > maxHp)
            currentHp = maxHp;

        Debug.Log($"Игрок восстановил {value} HP. HP: {currentHp}/{maxHp}");
    }

    public void TakeDamage(int value)
    {
        int damageLeft = value;

        if (shieldForCurrentTurn > 0)
        {
            int absorbed = Mathf.Min(shieldForCurrentTurn, damageLeft);
            shieldForCurrentTurn -= absorbed;
            damageLeft -= absorbed;
        }

        if (damageLeft > 0)
        {
            currentHp -= damageLeft;
            if (currentHp < 0)
                currentHp = 0;
        }

        Debug.Log($"Игрок получил {value} урона. HP: {currentHp}/{maxHp}, Shield: {shieldForCurrentTurn}");
    }

    public void ResetShieldAtEndTurn()
    {
        shieldForCurrentTurn = 0;
        Debug.Log("Щит игрока сброшен в конце хода.");
    }
}
