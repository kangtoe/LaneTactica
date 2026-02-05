using UnityEngine;

/// <summary>
/// 타워 전용 데이터
/// </summary>
[CreateAssetMenu(fileName = "NewTowerData", menuName = "LaneTactica/Tower Data")]
public class TowerData : UnitData
{
    [Header("Tower Specific")]
    public int energyCost = 50;         // 배치 비용
    public float cooldown = 5f;         // 재배치 쿨다운

    [Header("Special")]
    public int energyProduction = 0;    // 에너지 생산량 (에너지 생성기용)
    public float productionInterval = 1f; // 생산 간격
}
