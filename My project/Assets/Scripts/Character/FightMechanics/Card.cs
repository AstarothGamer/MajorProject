using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum CardType
{
    Attack,
    Skill,
    Passive
}

public enum AttackTargetMode
{
    None,
    SingleEnemy,                
    SeveralEnemiesFromSelected, 
    AllEnemies                  
}

[System.Serializable]
public class CardEffectSettings
{
    [Header("Main parameters")]
    public int energyCost = 1;
    public int energyGainAfterPlay = 0;

    [Header("Effects")]
    public int damage = 0;
    public int shieldToPlayerForOneTurn = 0;
    public int playerLoseHp = 0;

    [Tooltip("0.0 = not healing, 1.0 = heal 100% on dealing damage")]
    [Range(0f, 3f)]
    public float healFromDamageMultiplier = 0f;
}

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class Card : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Card Info")]
    public string cardName;
    [TextArea] public string description;

    [Header("Card Setup")]
    public CardType cardType = CardType.Attack;
    public AttackTargetMode attackTargetMode = AttackTargetMode.SingleEnemy;

    [Header("Effects")]
    public CardEffectSettings effects = new CardEffectSettings();

    [Header("Drag Settings")]
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private float dragScale = 1.1f;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Transform originalParent;
    private int originalSiblingIndex;
    private Vector3 originalPosition;
    private Vector3 originalScale;

    private bool isDragging;

    private CardHandZone currentHandZone;
    private CardPlayZone playZone;
    private PlayerCombat playerCombat;
    private DeckManager deckManager;
    private TurnManager turnManager;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();

        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();

        playZone = FindFirstObjectByType<CardPlayZone>();
        playerCombat = FindFirstObjectByType<PlayerCombat>();
        currentHandZone = GetComponentInParent<CardHandZone>();
        deckManager = FindFirstObjectByType<DeckManager>();
        turnManager = FindFirstObjectByType<TurnManager>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!CanStartDrag())
            return;

        isDragging = true;

        originalParent = transform.parent;
        originalSiblingIndex = transform.GetSiblingIndex();
        originalPosition = rectTransform.position;
        originalScale = rectTransform.localScale;

        transform.SetParent(rootCanvas.transform, true);
        transform.SetAsLastSibling();

        rectTransform.localScale = originalScale * dragScale;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.85f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        rectTransform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;
    
        isDragging = false;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        rectTransform.localScale = originalScale;
    
        EnemyUnit enemy = GetEnemyUnderMouse(eventData);
    
        GameObject dropObject = eventData.pointerCurrentRaycast.gameObject;
        CardPlayZone zone = null;
    
        if (dropObject != null)
            zone = dropObject.GetComponentInParent<CardPlayZone>();
    
        if (TryPlayCardHybrid(enemy, zone))
            return;
    
        ReturnToHand();
    }
    
    private bool TryPlayCardHybrid(EnemyUnit enemy, CardPlayZone zone)
    {
        if (!HasEnoughEnergy())
            return false;

        switch (cardType)
        {
            case CardType.Attack:
                return TryPlayAttack(enemy, zone);

            case CardType.Skill:
            case CardType.Passive:
                return TryPlaySkillOrPassive(zone);

            default:
                return false;
        }
    }
    
    private EnemyUnit GetEnemyUnderMouse(PointerEventData eventData)
    {
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            return hit.collider.GetComponentInParent<EnemyUnit>();
        }

        return null;
    }


    private bool CanStartDrag()
    {
        if (playerCombat == null)
        {
            Debug.LogWarning($"Card [{name}] does not PlayerCombat on scene.");
            return false;
        }
        if (turnManager != null && !turnManager.IsPlayerTurn())
        {
            return false;
        }

        return true;
    }

    private bool TryPlayCard(GameObject dropObject)
    {
        if (dropObject == null)
            return false;

        if (!HasEnoughEnergy())
        {
            Debug.Log($"Not enough energy: {cardName}");
            return false;
        }

        EnemyUnit enemy = dropObject.GetComponentInParent<EnemyUnit>();
        CardPlayZone zone = dropObject.GetComponentInParent<CardPlayZone>();

        switch (cardType)
        {
            case CardType.Attack:
                return TryPlayAttack(enemy, zone);

            case CardType.Skill:
            case CardType.Passive:
                return TryPlaySkillOrPassive(zone);

            default:
                return false;
        }
    }

    private bool TryPlayAttack(EnemyUnit enemy, CardPlayZone zone)
    {
        switch (attackTargetMode)
        {
            case AttackTargetMode.SingleEnemy:
                if (enemy == null)
                    return false;

                PlayOnEnemies(new List<EnemyUnit> { enemy });
                return true;

            case AttackTargetMode.SeveralEnemiesFromSelected:
                if (enemy == null)
                    return false;

                List<EnemyUnit> severalTargets = new List<EnemyUnit>();
                severalTargets.Add(enemy);

                if (enemy.additionalTargets != null)
                {
                    foreach (EnemyUnit extraTarget in enemy.additionalTargets)
                    {
                        if (extraTarget != null && !severalTargets.Contains(extraTarget))
                            severalTargets.Add(extraTarget);
                    }
                }

                PlayOnEnemies(severalTargets);
                return true;

            case AttackTargetMode.AllEnemies:
                if (zone == null)
                    return false;

                EnemyUnit[] allEnemies = FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None);
                PlayOnEnemies(new List<EnemyUnit>(allEnemies));
                return true;

            default:
                return false;
        }
    }

    private bool TryPlaySkillOrPassive(CardPlayZone zone)
    {
        if (zone == null)
            return false;

        PlayWithoutEnemyTarget();
        return true;
    }

    private bool HasEnoughEnergy()
    {
        return playerCombat.CurrentEnergy >= effects.energyCost;
    }

    private void PlayOnEnemies(List<EnemyUnit> enemies)
    {
        int totalDamageDealt = 0;

        playerCombat.SpendEnergy(effects.energyCost);

        if (effects.shieldToPlayerForOneTurn > 0)
            playerCombat.AddShieldForOneTurn(effects.shieldToPlayerForOneTurn);

        if (effects.playerLoseHp > 0)
            playerCombat.TakeDamage(effects.playerLoseHp);

        if (effects.damage > 0)
        {
            foreach (EnemyUnit enemy in enemies)
            {
                if (enemy == null)
                    continue;

                int dealt = enemy.TakeDamage(effects.damage);
                totalDamageDealt += dealt;
            }
        }

        if (effects.healFromDamageMultiplier > 0f && totalDamageDealt > 0)
        {
            int healValue = Mathf.RoundToInt(totalDamageDealt * effects.healFromDamageMultiplier);
            if (healValue > 0)
                playerCombat.Heal(healValue);
        }

        if (effects.energyGainAfterPlay > 0)
            playerCombat.GainEnergy(effects.energyGainAfterPlay);

        FinishPlay();
    }

    private void PlayWithoutEnemyTarget()
    {
        playerCombat.SpendEnergy(effects.energyCost);

        if (effects.shieldToPlayerForOneTurn > 0)
            playerCombat.AddShieldForOneTurn(effects.shieldToPlayerForOneTurn);

        if (effects.playerLoseHp > 0)
            playerCombat.TakeDamage(effects.playerLoseHp);

        if (effects.energyGainAfterPlay > 0)
            playerCombat.GainEnergy(effects.energyGainAfterPlay);

        FinishPlay();
    }
    
    private void FinishPlay()
    {
        Debug.Log($"Card [{cardName}] has been played.");
        
        // Add VFX and SFX
        // discarding cards 
        // and etc
        
        if (deckManager != null)
        {
            deckManager.OnCardPlayed(this);
        }
        else
        {
            Debug.LogWarning("DeckManager not found!");
            Destroy(gameObject);
        }
    }

    private void ReturnToHand()
    {
        transform.SetParent(originalParent, true);
        transform.SetSiblingIndex(originalSiblingIndex);
        rectTransform.position = originalPosition;
        rectTransform.localScale = originalScale;

        Debug.Log($"Card [{cardName}] was back to hand.");
    }
}