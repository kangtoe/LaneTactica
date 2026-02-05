using System;
using UnityEngine;

public enum AttackType
{
    None,
    Melee,
    Ranged
}

/// <summary>
/// 모든 유닛의 기본 클래스
/// </summary>
public abstract class UnitBase : MonoBehaviour, ITargetable, IDamageable
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

    [Header("Runtime State")]
    [SerializeField] protected int currentHealth;
    [SerializeField] protected int lane;

    protected float attackTimer;
    protected ITargetable currentTarget;

    // Events
    public event Action<int, int> OnHealthChanged;
    public event Action OnDeath;

    // ITargetable
    public Transform Transform => transform;
    public int Lane => lane;
    public bool IsAlive => currentHealth > 0;

    // IDamageable
    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

    // Properties
    public string UnitName => unitName;

    protected virtual void Awake()
    {
        currentHealth = maxHealth;
    }

    protected virtual void Start()
    {
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

    protected abstract ITargetable FindTarget();

    protected virtual bool IsInRange(ITargetable target)
    {
        if (target == null) return false;

        float distance = Vector3.Distance(transform.position, target.Transform.position);
        return distance <= attackRange;
    }

    protected virtual void PerformAttack(ITargetable target)
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

    protected virtual void SpawnProjectile(ITargetable target)
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
