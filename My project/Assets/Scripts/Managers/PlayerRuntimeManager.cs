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
        if (currentHp < 0)
            currentHp = 0;

        Debug.Log($"Player HP: {currentHp}/{maxHp}");
    }

    public void Heal(int amount)
    {
        currentHp += amount;
        if (currentHp > maxHp)
            currentHp = maxHp;

        Debug.Log($"Player HP: {currentHp}/{maxHp}");
    }

    public void ResetStats()
    {
        currentHp = maxHp;
        Debug.Log("Player stats reset");
    }
}
