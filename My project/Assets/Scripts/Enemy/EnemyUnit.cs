using System.Collections;
using TMPro;
using UnityEngine;

public class EnemyUnit : MonoBehaviour
{
    [Header("Enemy Stats")]
    public string enemyName;
    public int minHp;
    public int maxHp;
    public int currentHp;
    public int minDamage;
    public int maxDamage;
    public int currentDamage;
    public int minShield;
    public int maxShield;
    public int currentShield;
    public int shieldForCurrentTurn;

    [Header("Texts")] 
    [SerializeField] TMP_Text enemyTakeDamageText;
    [SerializeField] TMP_Text enemyText;
    [SerializeField] TMP_Text nextTurnText;

    // [Header("Optional Multi Target Setup")]
    // public List<EnemyUnit> additionalTargets = new List<EnemyUnit>();
    
    [Header("Animation")]
    [SerializeField] private Animator animator;

    private bool attacked = false;

    void Awake()
    {
        currentHp = Random.Range(minHp, maxHp + 1);
        enemyText.text = $"HP {currentHp} \n Shield {shieldForCurrentTurn}";
    }

    void Start()
    {
        NextTurn();
    }

    public int TakeDamage(int damage)
    {
        int damageLeft = damage;

        if (shieldForCurrentTurn > 0)
        {
            int absorbed = Mathf.Min(shieldForCurrentTurn, damageLeft);
            shieldForCurrentTurn -= absorbed;
            damageLeft -= absorbed;
        }
        
        int before = currentHp;
        
        if (damageLeft > 0)
        {
            currentHp -= damageLeft;
            if(currentHp < 0)
                currentHp = 0;
        }

        int realDamage = before - currentHp;

        enemyText.text = $"HP {currentHp} \n Shield {shieldForCurrentTurn}";
        enemyTakeDamageText.text = $"[{enemyName}] got {realDamage} damage.";

        if (currentHp <= 0)
            Die();

        return realDamage;
    }
    
    public IEnumerator TakeTurnRoutine(PlayerCombat player)
    {
        shieldForCurrentTurn = 0;

        if (attacked)
        {
            Debug.Log($"Enemy [{enemyName}] attacks for {currentDamage}");

            if (animator != null)
                animator.SetTrigger("Attack");

            yield return new WaitForSeconds(2f);

            player.TakeDamage(currentDamage);
        }
        else
        {
            Debug.Log($"Enemy [{enemyName}] gains {currentShield} shield");
            TakeShield(currentShield);
        }

        enemyText.text = $"HP {currentHp} \n Shield {shieldForCurrentTurn}";
    }

    public void TakeShield(int amount)
    {
        shieldForCurrentTurn += amount;
    }

    public void NextTurn()
    {
        if (!attacked)
        {
            currentDamage = Random.Range(minDamage, maxDamage + 1);
            attacked = true;
            nextTurnText.text = $"Next action: Attack {currentDamage}";
        }
        else
        {
            currentShield = Random.Range(minShield, maxShield + 1);
            attacked = false;
            nextTurnText.text = "Next action: Shield";
        }
    }

    private void Die()
    {
        Debug.Log($"Enemy [{enemyName}] died.");

        if (VictoryManager.Instance != null)
        {
            VictoryManager.Instance.CheckVictoryDelayed();
        }

        Destroy(gameObject);
    }
}

