using UnityEngine;

/// <summary>
/// 원거리 공격 투사체
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SphereCollider))]
public class Projectile : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float speed = 10f;
    [SerializeField] private float maxLifetime = 5f;
    [SerializeField] private float collisionRadius = 0.3f;

    [Header("Target Filtering")]
    [SerializeField] private LayerMask targetLayer;

    private Vector3 direction;
    private int damage;
    private float lifetime;
    private Rigidbody rb;
    private bool hasHit = false;

    private void Awake()
    {
        // Rigidbody 설정
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = true;

        // Collider 설정
        SphereCollider collider = GetComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = collisionRadius;
    }

    public void Initialize(Vector3 direction, int damage, LayerMask targetLayer)
    {
        this.direction = direction.normalized;
        this.damage = damage;
        this.targetLayer = targetLayer;
        this.lifetime = 0f;
        this.hasHit = false;

        // 발사 방향으로 회전
        if (this.direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(this.direction);
        }
    }

    private void Update()
    {
        // 이미 명중했으면 업데이트 중지
        if (hasHit)
            return;

        lifetime += Time.deltaTime;
        if (lifetime >= maxLifetime)
        {
            Destroy(gameObject);
            return;
        }

        // 직진 이동
        transform.position += direction * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        // 이미 명중했으면 무시
        if (hasHit)
            return;

        // 레이어 체크
        int otherLayer = 1 << other.gameObject.layer;
        if ((targetLayer.value & otherLayer) == 0)
            return;

        // 충돌한 오브젝트가 유닛인지 확인
        if (!other.TryGetComponent<UnitBase>(out var unit))
            return;

        // 명중 처리
        HitTarget(unit);
    }

    private void HitTarget(UnitBase unit)
    {
        hasHit = true;
        unit.TakeDamage(damage);
        Destroy(gameObject);
    }
}
