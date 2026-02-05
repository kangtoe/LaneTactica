using UnityEngine;

/// <summary>
/// 타워 유닛 기본 클래스
/// </summary>
public class TowerBase : UnitBase
{
    [Header("Tower Settings")]
    [SerializeField] private int energyCost = 50;
    [SerializeField] private float cooldown = 5f;

    [Header("Grid Position")]
    [SerializeField] protected int gridRow;
    [SerializeField] protected int gridCol;

    public int EnergyCost => energyCost;
    public float Cooldown => cooldown;
    public int GridRow => gridRow;
    public int GridCol => gridCol;

    public void SetGridPosition(int row, int col)
    {
        this.gridRow = row;
        this.gridCol = col;
        this.lane = row; // 레인 = 행
    }

    protected override ITargetable FindTarget()
    {
        // 같은 레인의 가장 가까운 적 찾기
        var enemies = FindObjectsByType<EnemyBase>(FindObjectsSortMode.None);

        EnemyBase closest = null;
        float closestDist = float.MaxValue;

        foreach (var enemy in enemies)
        {
            if (!enemy.IsAlive) continue;
            if (enemy.Lane != this.Lane) continue;

            float dist = Vector3.Distance(transform.position, enemy.transform.position);
            if (dist < closestDist)
            {
                closestDist = dist;
                closest = enemy;
            }
        }

        return closest;
    }

    protected override void OnUnitDeath()
    {
        // 그리드 셀 해제
        if (GameManager.Instance != null && GameManager.Instance.Grid != null)
        {
            GameManager.Instance.Grid.FreeCell(gridRow, gridCol);
        }

        Destroy(gameObject);
    }
}
