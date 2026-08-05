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

    private int generatedMaxHp;

    [Header("Texts")] 
    [SerializeField] TMP_Text enemyTakeDamageText;
    [SerializeField] TMP_Text enemyHP;
    [SerializeField] TMP_Text enemyShield;
    [SerializeField] TMP_Text nextTurnText;
    [SerializeField] private Transform floatingTextPoint;
    
    [Header("VFX and Audio")]
    [SerializeField] private GameObject particleEffect;
    [SerializeField] private HealthBarUI healthBar;
    [SerializeField] private DamageShake damageShake;

    // [Header("Optional Multi Target Setup")]
    // public List<EnemyUnit> additionalTargets = new List<EnemyUnit>();
    
    [Header("Animation")]
    [SerializeField] private Animator animator;

    private bool attacked = false;

    void Awake()
    {
        generatedMaxHp = Random.Range(minHp, maxHp + 1);
        currentHp = generatedMaxHp;
        
        enemyHP.text = $"HP {currentHp}";
        enemyShield.text = $"Shield {shieldForCurrentTurn}";
        
        if (healthBar != null)
        {
            healthBar.SetValueInstant(currentHp, generatedMaxHp);
        }
        
        if (damageShake == null)
            damageShake = GetComponentInChildren<DamageShake>();
    }

    void Start()
    {
        NextTurn();
    }

    public int TakeDamage(int damage)
    {
        int damageLeft = damage;
        int absorbed = 0;
        
        Vector3 textPosition = floatingTextPoint != null ? floatingTextPoint.position : transform.position;

        if (shieldForCurrentTurn > 0)
        {
            absorbed = Mathf.Min(shieldForCurrentTurn, damageLeft);

            shieldForCurrentTurn -= absorbed;
            damageLeft -= absorbed;

            if (FloatingCombatTextManager.Instance != null)
            {
                FloatingCombatTextManager.Instance.SpawnShieldDamage(absorbed, textPosition + new Vector3(0f, -0.25f, 0f));
            }
        }

        int before = currentHp;

        if (damageLeft > 0)
        {
            currentHp -= damageLeft;

            if (currentHp < 0)
                currentHp = 0;
        }

        int realDamage = before - currentHp;

        if (realDamage > 0)
        {
            if (FloatingCombatTextManager.Instance != null)
            {
                FloatingCombatTextManager.Instance.SpawnHpDamage(realDamage, textPosition);
            }
            
            if (damageShake != null)
                damageShake.Shake();

            if (particleEffect != null)
            {
                Instantiate(particleEffect, transform.position, Quaternion.identity);
            }
        }

        enemyHP.text = $"HP {currentHp}";
        enemyShield.text = $"Shield {shieldForCurrentTurn}";

        if (enemyTakeDamageText != null)
        {
            enemyTakeDamageText.text = $"[{enemyName}] got {realDamage} damage.";
        }
        
        if (healthBar != null)
        {
            healthBar.SetValue(currentHp, generatedMaxHp);
        }

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
            Instantiate(particleEffect, player.transform.position, Quaternion.identity);
        }
        else
        {
            Debug.Log($"Enemy [{enemyName}] gains {currentShield} shield");
            TakeShield(currentShield);
        }

        enemyHP.text = $"HP {currentHp}";
        enemyShield.text = $"Shield {shieldForCurrentTurn}";
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

