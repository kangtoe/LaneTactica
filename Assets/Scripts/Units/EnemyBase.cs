using UnityEngine;

/// <summary>
/// 적 유닛 기본 클래스
/// </summary>
public class EnemyBase : UnitBase
{
    [Header("Enemy Settings")]
    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private int goldReward = 10;

    private bool isMoving = true;

    public float MoveSpeed => moveSpeed;
    public int GoldReward => goldReward;

    protected override void Update()
    {
        base.Update();

        if (!IsAlive) return;

        if (isMoving)
        {
            Move();
        }
    }

    protected virtual void Move()
    {
        // 왼쪽으로 이동 (X 감소)
        transform.position += Vector3.left * moveSpeed * Time.deltaTime;

        // 기지 도달 체크
        if (transform.position.x < -6f)
        {
            OnReachBase();
        }
    }

    protected virtual void OnReachBase()
    {
        Debug.Log($"{UnitName} reached the base!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.Defeat();
        }

        Destroy(gameObject);
    }

    public void StopMoving()
    {
        isMoving = false;
    }

    public void StartMoving()
    {
        isMoving = true;
        currentTarget = null;
    }

    protected override ITargetable FindTarget()
    {
        // 같은 레인의 가장 가까운 타워 찾기
        var towers = FindObjectsByType<TowerBase>(FindObjectsSortMode.None);

        TowerBase closest = null;
        float closestDist = float.MaxValue;

        foreach (var tower in towers)
        {
            if (!tower.IsAlive) continue;
            if (tower.Lane != this.Lane) continue;

            // 적보다 왼쪽에 있는 타워만
            if (tower.transform.position.x >= transform.position.x) continue;

            float dist = Vector3.Distance(transform.position, tower.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = tower;
            }
        }

        // 타워가 범위 안에 있으면 멈추기
        if (closest != null && closestDist <= attackRange)
        {
            StopMoving();
        }

        return closest;
    }

    protected override void OnUnitDeath()
    {
        // 골드 보상
        if (GameManager.Instance != null)
        {
            GameManager.Instance.Resources.AddGold(goldReward);
        }

        Destroy(gameObject);
    }
}
