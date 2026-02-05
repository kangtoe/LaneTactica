using UnityEngine;

/// <summary>
/// 타겟팅 가능한 유닛 인터페이스
/// </summary>
public interface ITargetable
{
    Transform Transform { get; }
    int Lane { get; }
    bool IsAlive { get; }
}
