using UnityEngine;

public class PlayerRuntimeManager : MonoBehaviour
{
    public static PlayerRuntimeManager Instance;

    [Header("Base Stats")]
    [SerializeField] private int maxHp = 100;

    public int currentHp { get; private set; }
    public int MaxHp => maxHp;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        currentHp = maxHp;
    }

    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        if (currentHp <= 0)
        {
            currentHp = 0;
            GameOver.Instance.Show();
        }
        
        Debug.Log($"Player HP: {currentHp}/{maxHp}");
    }

    public void Heal(int amount)
    {
        currentHp += amount;
        
        Debug.Log($"Player HP: {currentHp}/{maxHp}");
    }

    public void ResetStats()
    {
        currentHp = maxHp;
        Debug.Log("Player stats reset");
    }

    public int HealedAmount(int value)
    {
        int amount = currentHp + value;
        int totalHP = currentHp + value;
        if (totalHP > maxHp)
        {
            amount = - totalHP + maxHp +  value;
        }
        else
        {
            amount = value;
        }
        return amount;
    }
}
