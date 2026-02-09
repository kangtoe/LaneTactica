using UnityEngine;

/// <summary>
/// 주기적으로 에너지를 생산하는 컴포넌트
/// UnitBase가 있는 오브젝트에 부착하면 생존 시에만 동작
/// </summary>
public class EnergyGenerator : MonoBehaviour
{
    [Header("Energy Generation")]
    [SerializeField] private int energyPerGeneration = 25;
    [SerializeField] private float generationInterval = 5f;

    private float generationTimer;
    private UnitBase unit;

    private void Awake()
    {
        unit = GetComponent<UnitBase>();
    }

    private void Update()
    {
        if (unit != null && !unit.IsAlive) return;

        generationTimer += Time.deltaTime;
        if (generationTimer >= generationInterval)
        {
            generationTimer -= generationInterval;
            GenerateEnergy();
        }
    }

    private void GenerateEnergy()
    {
        if (GameManager.Instance == null) return;

        var resourceManager = GameManager.Instance.Resources;
        if (resourceManager == null) return;

        resourceManager.AddEnergy(energyPerGeneration);
    }
}
