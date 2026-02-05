using UnityEngine;

public enum AttackType
{
    None,       // 공격 불가 (방벽 등)
    Melee,      // 근접 공격
    Ranged      // 원거리 공격 (투사체)
}

/// <summary>
/// 유닛 공통 데이터 (ScriptableObject)
/// </summary>
[CreateAssetMenu(fileName = "NewUnitData", menuName = "LaneTactica/Unit Data")]
public class UnitData : ScriptableObject
{
    [Header("Basic Info")]
    public string unitName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Stats")]
    public int maxHealth = 100;
    public int attackDamage = 10;
    public float attackSpeed = 1f;      // 초당 공격 횟수
    public float attackRange = 1f;      // 공격 거리 (그리드 단위)

    [Header("Attack")]
    public AttackType attackType = AttackType.Melee;
    public GameObject projectilePrefab;  // 원거리 공격 시 사용

    [Header("Visual")]
    public GameObject prefab;
    public Color unitColor = Color.white;
}
