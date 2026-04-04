using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnemyUnit : MonoBehaviour
{
    [Header("Enemy Stats")]
    public string enemyName;
    public int maxHp = 30;
    public int currentHp = 30;
    public int damage = 5;
    
    [SerializeField] TMP_Text enemyText;

    [Header("Optional Multi Target Setup")]
    public List<EnemyUnit> additionalTargets = new List<EnemyUnit>();

    public int TakeDamage(int damage)
    {
        int before = currentHp;
        currentHp -= damage;
        if (currentHp < 0)
            currentHp = 0;

        int realDamage = before - currentHp;

        enemyText.text = $"[{enemyName}] got {realDamage} damage. HP: {currentHp}/{maxHp}";

        if (currentHp <= 0)
            Die();

        return realDamage;
    }
    
    public void TakeTurn(PlayerCombat player)
    {
        Debug.Log($"Enemy [{enemyName}] deal to player {damage} damage.");

        player.TakeDamage(damage);
    }

    private void Die()
    {
        Debug.Log($"Enemy [{enemyName}] died.");
        Destroy(gameObject);
    }
}

