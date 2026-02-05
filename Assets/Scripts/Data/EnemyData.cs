using UnityEngine;

/// <summary>
/// 적 전용 데이터
/// </summary>
[CreateAssetMenu(fileName = "NewEnemyData", menuName = "LaneTactica/Enemy Data")]
public class EnemyData : UnitData
{
    [Header("Enemy Specific")]
    public float moveSpeed = 1f;        // 이동 속도

    [Header("Spawn Settings")]
    public int tier = 1;                // 등장 최소 웨이브
    public int spawnWeight = 10;        // 스폰 가중치 (높을수록 자주 등장)
    public int pointCost = 1;           // 웨이브 예산 소모량

    [Header("Rewards")]
    public int goldReward = 10;         // 처치 시 골드 보상
}
