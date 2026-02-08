using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum AttackType
{
    None,
    Melee,
    Ranged
}

/// <summary>
/// 모든 유닛의 기본 클래스
/// </summary>
public abstract class UnitBase : MonoBehaviour
{
    [Header("Basic Info")]
    [SerializeField] protected string unitName = "Unit";

    [Header("Stats")]
    [SerializeField] protected int maxHealth = 100;
    [SerializeField] protected int attackDamage = 10;
    [SerializeField] protected float attackSpeed = 1f;
    [SerializeField] protected float attackRange = 1f;

    [Header("Attack")]
    [SerializeField] protected AttackType attackType = AttackType.Melee;
    [SerializeField] protected GameObject projectilePrefab;

    [Header("Health Bar")]
    [SerializeField] protected bool showHealthBar = true;
    [SerializeField] protected Vector3 healthBarOffset = new Vector3(0, 1.5f, 0);
    [SerializeField] protected Vector2 healthBarSize = new Vector2(1f, 0.1f);

    [Header("Hit Flash")]
    [SerializeField] private Color hitFlashColor = Color.red;
    [SerializeField] private float hitFlashDuration = 0.15f;

    [Header("Runtime State")]
    [SerializeField] protected int currentHealth;
    [SerializeField] protected int lane;

    protected float attackTimer;
    protected UnitBase currentTarget;

    // Health Bar UI
    private Canvas healthBarCanvas;
    private Image healthBarFill;
    private Text nameLabel;

    // Hit Flash
    private Renderer unitRenderer;
    private Color originalColor;
    private Coroutine hitFlashCoroutine;

    // Events
    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    // Properties
    public int Lane => lane;
    public bool IsAlive => currentHealth > 0;
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public string UnitName => unitName;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;

        unitRenderer = GetComponentInChildren<Renderer>();
        if (unitRenderer != null)
        {
            originalColor = unitRenderer.material.color;
        }
    }

    protected virtual void Start()
    {
        if (showHealthBar)
        {
            CreateHealthBar();
        }
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);
    }

    protected virtual void Update()
    {
        if (!IsAlive) return;

        if (attackType != AttackType.None)
        {
            attackTimer += Time.deltaTime;

            if (attackTimer >= 1f / attackSpeed)
            {
                TryAttack();
                attackTimer = 0f;
            }
        }
    }

    public virtual void SetLane(int lane)
    {
        this.lane = lane;
    }

    #region Combat

    protected virtual void TryAttack()
    {
        if (currentTarget == null || !currentTarget.IsAlive)
        {
            currentTarget = FindTarget();
        }

        if (currentTarget != null && IsInRange(currentTarget))
        {
            PerformAttack(currentTarget);
        }
    }

    protected abstract UnitBase FindTarget();

    protected virtual bool IsInRange(UnitBase target)
    {
        if (target == null) return false;

        float distance = Vector3.Distance(transform.position, target.transform.position);
        return distance <= attackRange;
    }

    protected virtual void PerformAttack(UnitBase target)
    {
        if (target == null) return;

        if (attackType == AttackType.Ranged && projectilePrefab != null)
        {
            SpawnProjectile(target);
        }
        else if (attackType == AttackType.Melee)
        {
            DealDamage(target, attackDamage);
        }
    }

    protected virtual void SpawnProjectile(UnitBase target)
    {
        if (projectilePrefab == null) return;

        GameObject projObj = Instantiate(
            projectilePrefab,
            transform.position + Vector3.up * 0.5f,
            Quaternion.identity
        );

        var projectile = projObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(target, attackDamage);
        }
    }

    protected virtual void DealDamage(UnitBase target, int damage)
    {
        target.TakeDamage(damage);
    }

    #endregion

    #region Damage

    public virtual void TakeDamage(int damage)
    {
        if (!IsAlive || damage <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);
        UpdateHealthBar();
        FlashOnHit();

        Debug.Log($"{UnitName} took {damage} damage. HP: {currentHealth}/{MaxHealth}");

        if (!IsAlive)
        {
            Die();
        }
    }

    public virtual void Heal(int amount)
    {
        if (!IsAlive || amount <= 0) return;

        currentHealth = Mathf.Min(MaxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);
        UpdateHealthBar();
    }

    protected virtual void Die()
    {
        Debug.Log($"{UnitName} died!");
        OnDeath?.Invoke();
        OnUnitDeath();
    }

    protected abstract void OnUnitDeath();

    #endregion

    #region Hit Flash

    private void FlashOnHit()
    {
        if (unitRenderer == null) return;

        if (hitFlashCoroutine != null)
        {
            StopCoroutine(hitFlashCoroutine);
        }
        hitFlashCoroutine = StartCoroutine(HitFlashRoutine());
    }

    private IEnumerator HitFlashRoutine()
    {
        unitRenderer.material.color = hitFlashColor;
        yield return new WaitForSeconds(hitFlashDuration);
        unitRenderer.material.color = originalColor;
        hitFlashCoroutine = null;
    }

    #endregion

    #region Health Bar

    private void CreateHealthBar()
    {
        // Canvas 생성
        var canvasObj = new GameObject("HealthBar");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = healthBarOffset;

        healthBarCanvas = canvasObj.AddComponent<Canvas>();
        healthBarCanvas.renderMode = RenderMode.WorldSpace;
        healthBarCanvas.overrideSorting = true;
        healthBarCanvas.sortingOrder = 100;

        var rectTransform = canvasObj.GetComponent<RectTransform>();
        rectTransform.sizeDelta = healthBarSize;

        // Background
        var bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform);
        var bgImage = bgObj.AddComponent<Image>();
        bgImage.color = Color.black;
        var bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.sizeDelta = Vector2.zero;
        bgRect.anchoredPosition3D = Vector3.zero;

        // Fill
        var fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(canvasObj.transform);
        healthBarFill = fillObj.AddComponent<Image>();
        healthBarFill.color = Color.green;
        var fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.sizeDelta = Vector2.zero;
        fillRect.anchoredPosition3D = Vector3.zero;
        fillRect.pivot = new Vector2(0, 0.5f);

        // Name Label
        var nameObj = new GameObject("NameLabel");
        nameObj.transform.SetParent(canvasObj.transform);
        nameLabel = nameObj.AddComponent<Text>();
        nameLabel.text = unitName;
        nameLabel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        nameLabel.fontSize = 80;
        nameLabel.alignment = TextAnchor.LowerCenter;
        nameLabel.horizontalOverflow = HorizontalWrapMode.Overflow;
        nameLabel.verticalOverflow = VerticalWrapMode.Overflow;
        nameLabel.color = Color.white;
        var nameRect = nameObj.GetComponent<RectTransform>();
        nameRect.anchorMin = new Vector2(0.5f, 1);
        nameRect.anchorMax = new Vector2(0.5f, 1);
        nameRect.pivot = new Vector2(0.5f, 0);
        nameRect.anchoredPosition3D = Vector3.zero;
        nameRect.sizeDelta = new Vector2(200, 30);
        nameRect.localScale = Vector3.one * 0.003f;

        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill == null) return;

        float ratio = (float)currentHealth / maxHealth;
        var rect = healthBarFill.rectTransform;
        rect.anchorMax = new Vector2(ratio, 1);

        // 체력에 따라 색상 변경
        if (ratio > 0.5f)
            healthBarFill.color = Color.green;
        else if (ratio > 0.25f)
            healthBarFill.color = Color.yellow;
        else
            healthBarFill.color = Color.red;
    }

    protected virtual void LateUpdate()
    {
        // 체력바가 카메라를 바라보도록
        if (healthBarCanvas != null && Camera.main != null)
        {
            healthBarCanvas.transform.forward = Camera.main.transform.forward;
        }
    }

    #endregion
}
