/// <summary>
/// 피격 가능한 유닛 인터페이스
/// </summary>
public interface IDamageable
{
    int CurrentHealth { get; }
    int MaxHealth { get; }
    bool IsAlive { get; }

    void TakeDamage(int damage);
    void Heal(int amount);
}
