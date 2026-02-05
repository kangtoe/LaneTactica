using UnityEngine;

/// <summary>
/// 원거리 공격 투사체
/// </summary>
public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float maxLifetime = 5f;

    private ITargetable target;
    private int damage;
    private float lifetime;

    public void Initialize(ITargetable target, int damage)
    {
        this.target = target;
        this.damage = damage;
        this.lifetime = 0f;
    }

    private void Update()
    {
        lifetime += Time.deltaTime;
        if (lifetime >= maxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        if (target == null || !target.IsAlive)
        {
            Destroy(gameObject);
            return;
        }

        // 타겟을 향해 이동
        Vector3 direction = (target.Transform.position - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;

        // 타겟 방향으로 회전
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        // 타겟에 도달했는지 체크
        float distance = Vector3.Distance(transform.position, target.Transform.position);
        if (distance < 0.3f)
        {
            HitTarget();
        }
    }

    private void HitTarget()
    {
        if (target is IDamageable damageable)
        {
            damageable.TakeDamage(damage);
        }

        Destroy(gameObject);
    }
}
