using System.Collections;
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

    [Header("Wheel Effects")]
    public List<int> damageValues = new List<int>();
    public List<int> shieldValues = new List<int>();
    public List<int> playerLoseHpValues = new List<int>();

    [Tooltip("0.0 = not healing, 1.0 = heal 100% on dealing damage")]
    [Range(0f, 3f)]
    public float healFromDamageMultiplier = 0f;
}

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class Card : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
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
    [SerializeField] private float dragFollowSmoothTime = 0.06f;
    
    [Header("Reward Mode")]
    public bool isRewardCard = false;
    public System.Action<Card> OnRewardSelected;
    [SerializeField] private GameObject selectionFrame;
    private Card originalPrefab;
    
    [Header("Hover Settings")]
    [SerializeField] private float hoverScale = 1.15f;
    [SerializeField] private float hoverLift = 70f;
    [SerializeField] private float hoverAnimationSpeed = 12f;
    [SerializeField] private GameObject hoverHighlight;
    
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;

    private Transform originalParent;
    private int originalSiblingIndex;
    private Vector3 originalPosition;
    private Vector3 originalScale;

    private bool isDragging;
    private bool isResolvingPlay;
    
    private Vector3 dragTargetPosition;
    private Vector3 dragVelocity;
    private Vector3 dragPointerOffset;
    
    private Vector2 layoutPosition;
    private Quaternion layoutRotation;
    private Vector3 baseScale;

    private bool layoutInitialized;
    private bool isHovered;
    private int hoverSiblingIndex;

    private CardHandZone currentHandZone;
    private CardPlayZone playZone;
    private PlayerCombat playerCombat;
    private DeckManager deckManager;
    private TurnManager turnManager;
    private WheelOfFortune wheel;
    
    private OutlineOnPoint outline;
    private LayerMask layerMask;
    
    private int lastWheelIndex = 0;
    private bool wheelFinished = false;

    #region Unity Functions

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        baseScale = rectTransform.localScale;

        if (hoverHighlight != null)
            hoverHighlight.SetActive(false);

        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();

        playZone = FindFirstObjectByType<CardPlayZone>();
        playerCombat = FindFirstObjectByType<PlayerCombat>();
        currentHandZone = GetComponentInParent<CardHandZone>();
        deckManager = FindFirstObjectByType<DeckManager>();
        turnManager = FindFirstObjectByType<TurnManager>();
        wheel = FindFirstObjectByType<WheelOfFortune>();
    }

    void Start()
    {
        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();
        
        if (wheel != null)
        {
            wheel.OnSpinFinished.AddListener(OnWheelFinished);
        }
    }
    
    private void Update()
    {
        if (isDragging)
        {
            rectTransform.position =
                Vector3.SmoothDamp(
                    rectTransform.position,
                    dragTargetPosition,
                    ref dragVelocity,
                    dragFollowSmoothTime
                );

            return;
        }
        
        if (isResolvingPlay)
            return;

        if (!layoutInitialized)
            return;

        float animationStep = 1f - Mathf.Exp(-hoverAnimationSpeed * Time.deltaTime);

        Vector2 targetPosition = layoutPosition;

        Quaternion targetRotation = layoutRotation;

        Vector3 targetScale = baseScale;

        if (isHovered)
        {
            targetPosition += Vector2.up * hoverLift;

            targetScale = baseScale * hoverScale;

            targetRotation = Quaternion.identity;
        }

        rectTransform.anchoredPosition =
            Vector2.Lerp(
                rectTransform.anchoredPosition,
                targetPosition,
                animationStep
            );

        rectTransform.localScale =
            Vector3.Lerp(
                rectTransform.localScale,
                targetScale,
                animationStep
            );

        rectTransform.localRotation =
            Quaternion.Slerp(
                rectTransform.localRotation,
                targetRotation,
                animationStep
            );
    }

    #endregion
    
    
    private void HoldCardAtPlayPosition()
    {
        isDragging = false;
        isHovered = false;
        isResolvingPlay = true;

        dragVelocity = Vector3.zero;

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 1f;

        rectTransform.localScale = baseScale;

        transform.SetAsLastSibling();

        if (hoverHighlight != null)
            hoverHighlight.SetActive(false);
    }
    
    public void SetOriginalPrefab(Card prefab)
    {
        originalPrefab = prefab;
    }

    public Card GetOriginalPrefab()
    {
        return originalPrefab != null ? originalPrefab : this;
    }
    
    public void SetSelected(bool value)
    {
        if (selectionFrame != null)
            selectionFrame.SetActive(value);
    }
    
    
    private void OnWheelFinished(int index)
    {
        lastWheelIndex = index;
        wheelFinished = true;
    }
    
    private void OnDestroy()
    {
        if (wheel != null)
        {
            wheel.OnSpinFinished.RemoveListener(OnWheelFinished);
        }
    }
    
    

    #region Drag

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isRewardCard) return;
        if (!CanStartDrag())
            return;
        if (turnManager != null && turnManager.IsActionInProgress)
            return;

        isDragging = true;

        originalParent = transform.parent;
        originalSiblingIndex = isHovered ? hoverSiblingIndex : transform.GetSiblingIndex();
        
        isHovered = false;
        
        originalPosition = rectTransform.position;
        originalScale = rectTransform.localScale;
        
        if (hoverHighlight != null)
            hoverHighlight.SetActive(false);

        transform.SetParent(rootCanvas.transform, true);
        transform.SetAsLastSibling();

        rectTransform.localScale = baseScale * dragScale;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.85f;
        
        dragPointerOffset = rectTransform.position - (Vector3)eventData.position;
        dragTargetPosition = rectTransform.position;
        dragVelocity = Vector3.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isRewardCard) return;
        if (!isDragging)
            return;
        
        OutlineOnPoint outline = GetOutlineUnderMouse(eventData);
        UpdateOutline(outline);

        dragTargetPosition = (Vector3)eventData.position + dragPointerOffset;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isRewardCard) return;
        if (!isDragging)
            return;
        
        ClearOutline();
        
        if (turnManager != null && turnManager.IsActionInProgress)
        {
            ReturnToHand();
            return;
        }
    
        isDragging = false;
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        rectTransform.localScale = Vector3.one;
    
        EnemyUnit enemy = GetEnemyUnderMouse(eventData);
    
        GameObject dropObject = eventData.pointerCurrentRaycast.gameObject;
        CardPlayZone zone = null;
    
        if (dropObject != null)
            zone = dropObject.GetComponentInParent<CardPlayZone>();
    
        if (TryPlayCardHybrid(enemy, zone))
        {
            HoldCardAtPlayPosition();
            return;
        }
    
        ReturnToHand();
    }

    #endregion
    
    #region Pointer
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isRewardCard) return;

        OnRewardSelected?.Invoke(this);
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isDragging && !isHovered)
        {
            isHovered = true;

            hoverSiblingIndex =
                transform.GetSiblingIndex();

            transform.SetAsLastSibling();

            if (hoverHighlight != null)
                hoverHighlight.SetActive(true);
        }

        if (DescriptionPanel.Instance != null)
        {
            DescriptionPanel.Instance.Show(
                cardName,
                description,
                GetFullDescription(),
                eventData.position +
                new Vector2(200f, -50f)
            );
        }
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isDragging && isHovered)
        {
            isHovered = false;

            if (hoverHighlight != null)
                hoverHighlight.SetActive(false);

            if (transform.parent != null)
            {
                int siblingIndex = Mathf.Clamp(
                    hoverSiblingIndex,
                    0,
                    transform.parent.childCount - 1
                );

                transform.SetSiblingIndex(
                    siblingIndex
                );
            }
        }

        if (DescriptionPanel.Instance != null)
            DescriptionPanel.Instance.Hide();
    }
    #endregion
    
    public void SetLayoutTransform(
        Vector2 position,
        float angle)
    {
        layoutPosition = position;
        layoutRotation = Quaternion.Euler(
            0f,
            0f,
            angle
        );

        if (!layoutInitialized)
        {
            rectTransform.anchoredPosition =
                layoutPosition;

            rectTransform.localRotation =
                layoutRotation;

            layoutInitialized = true;
        }
    }
    
    private OutlineOnPoint GetOutlineUnderMouse(PointerEventData eventData)
    {
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);

        if (Physics.Raycast(ray, out RaycastHit hit, 100f))
        {
            return hit.collider.GetComponentInParent<OutlineOnPoint>();
        }

        return null;
    }

    private void UpdateOutline(OutlineOnPoint newOutline)
    {
        OutlineOnPoint validOutline = null;

        if (newOutline != null)
        {
            switch (cardType)
            {
                case CardType.Attack:
                    if (newOutline.GetComponent<EnemyUnit>() != null)
                    {
                        validOutline = newOutline;
                    }
                    break;
                case CardType.Passive:
                case CardType.Skill:
                {
                    if (newOutline.GetComponent<PlayerCombat>() != null)
                    {
                        validOutline = newOutline;
                    }
                }   
                    break;
            }
        }
        
        if(outline == validOutline)
            return;
        
        if(outline != null)
            outline.Outline(false);
        
        outline = newOutline;

        if (outline != null)
            outline.Outline(true);
    }

    private void ClearOutline()
    {
        if (outline != null)
        {
            outline.Outline(false);
            outline = null;
        }
    }
    
    private string GetFullDescription()
    {
        string text = "";

        if (effects.damageValues != null && effects.damageValues.Count > 0)
            text += $"Wheel:   1| 2| 3| 4\nDamage: {string.Join("| ", effects.damageValues)}\n";

        if (effects.shieldValues != null && effects.shieldValues.Count > 0)
            text += $"Wheel:   1| 2| 3| 4\nShield: {string.Join(", ", effects.shieldValues)}\n";

        if (effects.playerLoseHpValues != null && effects.playerLoseHpValues.Count > 0)
            text += $"Player loses HP: {string.Join(", ", effects.playerLoseHpValues)}\n";

        if (effects.healFromDamageMultiplier > 0)
            text += $"Heals for {(effects.healFromDamageMultiplier * 100f):0}% of dealt damage\n";

        if (effects.energyCost > 0)
            text += $"Cost: {effects.energyCost} energy\n";

        if (effects.energyGainAfterPlay > 0)
            text += $"Gain after play: {effects.energyGainAfterPlay} energy\n";

        return text;
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

    private bool TryPlayAttack(EnemyUnit enemy, CardPlayZone zone)
    {
        switch (attackTargetMode)
        {
            case AttackTargetMode.SingleEnemy:
                if (enemy == null)
                    return false;

                StartCoroutine(PlayCardWithWheel(new List<EnemyUnit> { enemy }));
                return true;

            case AttackTargetMode.SeveralEnemiesFromSelected:
                if (enemy == null)
                    return false;

                List<EnemyUnit> severalTargets = new List<EnemyUnit>();
                severalTargets.Add(enemy);
                
                StartCoroutine(PlayCardWithWheel(severalTargets));
                return true;

            case AttackTargetMode.AllEnemies:
                if (zone == null)
                    return false;

                EnemyUnit[] allEnemies = FindObjectsByType<EnemyUnit>(FindObjectsSortMode.None);
                StartCoroutine(PlayCardWithWheel(new List<EnemyUnit>(allEnemies)));
                return true;

            default:
                return false;
        }
    }

    private bool TryPlaySkillOrPassive(CardPlayZone zone)
    {
        if (zone == null)
            return false;

        StartCoroutine(PlayCardWithWheel(null));
        return true;
    }
    
    private IEnumerator PlayCardWithWheel(List<EnemyUnit> enemies)
    {
        if (turnManager != null)
            turnManager.StartAction();
        
        if (wheel == null)
        {
            Debug.LogWarning($"Card [{cardName}] cannot be played: WheelOfFortune not found.");
            yield break;
        }

        playerCombat.SpendEnergy(effects.energyCost);

        wheelFinished = false;

        wheel.Spin();

        while (!wheelFinished)
            yield return null;

        ApplyEffectsByIndex(lastWheelIndex, enemies);
    }
    
    private void ApplyEffectsByIndex(int wheelIndex, List<EnemyUnit> enemies)
    {
        int damage = GetValueFromWheelIndex(effects.damageValues, wheelIndex);
        int shield = GetValueFromWheelIndex(effects.shieldValues, wheelIndex);
        int loseHp = GetValueFromWheelIndex(effects.playerLoseHpValues, wheelIndex);

        int totalDamageDealt = 0;

        if (shield > 0)
            playerCombat.AddShieldForOneTurn(shield);

        if (loseHp > 0)
            playerCombat.TakeDamage(loseHp);

        if (damage > 0 && enemies != null)
        {
            foreach (EnemyUnit enemy in enemies)
            {
                if (enemy == null)
                    continue;

                int dealt = enemy.TakeDamage(damage);
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
    
    private int GetValueFromWheelIndex(List<int> values, int wheelIndex)
    {
        if (values == null || values.Count == 0)
            return 0;

        if (wheelIndex < 0)
            wheelIndex = 0;

        if (wheelIndex >= values.Count)
            wheelIndex = values.Count - 1;

        return values[wheelIndex];
    }

    private bool HasEnoughEnergy()
    {
        return playerCombat.CurrentEnergy >= effects.energyCost;
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
        isDragging = false;
        isHovered = false;
        isResolvingPlay = false;

        transform.SetParent(originalParent, true);

        transform.SetSiblingIndex(originalSiblingIndex);

        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        dragVelocity = Vector3.zero;

        if (hoverHighlight != null)
            hoverHighlight.SetActive(false);

        Debug.Log(
            $"Card [{cardName}] was back to hand."
        );
    }
}