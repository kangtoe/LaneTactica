using System;
using UnityEngine;

/// <summary>
/// 모든 유닛의 기본 클래스
/// </summary>
public abstract class UnitBase : MonoBehaviour, ITargetable, IDamageable
{
    [Header("Unit Data")]
    [SerializeField] protected UnitData unitData;

    [Header("Runtime State")]
    [SerializeField] protected int currentHealth;
    [SerializeField] protected int lane;

    protected float attackTimer;
    protected ITargetable currentTarget;

    // Events
    public event Action<int, int> OnHealthChanged;  // current, max
    public event Action OnDeath;

    // ITargetable
    public Transform Transform => transform;
    public int Lane => lane;
    public bool IsAlive => currentHealth > 0;

    // IDamageable
    public int CurrentHealth => currentHealth;
    public int MaxHealth => unitData != null ? unitData.maxHealth : 0;

    // Properties
    public UnitData Data => unitData;
    public string UnitName => unitData != null ? unitData.unitName : "Unknown";

    protected virtual void Awake()
    {
        if (unitData != null)
        {
            currentHealth = unitData.maxHealth;
        }
    }

    protected virtual void Start()
    {
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);
    }

    protected virtual void Update()
    {
        if (!IsAlive) return;

        // 공격 타이머
        if (unitData != null && unitData.attackType != AttackType.None)
        {
            attackTimer += Time.deltaTime;

            if (attackTimer >= 1f / unitData.attackSpeed)
            {
                TryAttack();
                attackTimer = 0f;
            }
        }
    }

    public virtual void Initialize(UnitData data, int lane)
    {
        this.unitData = data;
        this.lane = lane;
        this.currentHealth = data.maxHealth;

        // 색상 적용
        var renderer = GetComponent<Renderer>();
        if (renderer != null && data.unitColor != Color.white)
        {
            renderer.material.color = data.unitColor;
        }

        OnHealthChanged?.Invoke(currentHealth, MaxHealth);
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

    protected abstract ITargetable FindTarget();

    protected virtual bool IsInRange(ITargetable target)
    {
        if (target == null || unitData == null) return false;

        float distance = Vector3.Distance(transform.position, target.Transform.position);
        return distance <= unitData.attackRange;
    }

    protected virtual void PerformAttack(ITargetable target)
    {
        if (target == null || unitData == null) return;

        if (unitData.attackType == AttackType.Ranged && unitData.projectilePrefab != null)
        {
            SpawnProjectile(target);
        }
        else if (unitData.attackType == AttackType.Melee)
        {
            DealDamage(target, unitData.attackDamage);
        }
    }

    protected virtual void SpawnProjectile(ITargetable target)
    {
        if (unitData.projectilePrefab == null) return;

        GameObject projObj = Instantiate(
            unitData.projectilePrefab,
            transform.position + Vector3.up * 0.5f,
            Quaternion.identity
        );

        var projectile = projObj.GetComponent<Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(target, unitData.attackDamage);
        }
    }

    protected virtual void DealDamage(ITargetable target, int damage)
    {
        if (target is IDamageable damageable)
        {
            damageable.TakeDamage(damage);
        }
    }

    #endregion

    #region IDamageable

    public virtual void TakeDamage(int damage)
    {
        if (!IsAlive || damage <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - damage);
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);

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
    }

    protected virtual void Die()
    {
        Debug.Log($"{UnitName} died!");
        OnDeath?.Invoke();
        OnUnitDeath();
    }

    protected abstract void OnUnitDeath();

    #endregion
}
