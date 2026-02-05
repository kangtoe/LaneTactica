using System;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    [Header("Energy Settings")]
    [SerializeField] private int startingEnergy = 50;
    [SerializeField] private int energyPerTick = 10;
    [SerializeField] private float energyTickInterval = 1f;

    [Header("Gold Settings")]
    [SerializeField] private int startingGold = 0;

    [Header("Current Resources (Read Only)")]
    [SerializeField] private int currentEnergy;
    [SerializeField] private int currentGold;

    private float energyTimer;

    // Events
    public event Action<int> OnEnergyChanged;
    public event Action<int> OnGoldChanged;

    public int Energy => currentEnergy;
    public int Gold => currentGold;

    private void Awake()
    {
        currentEnergy = startingEnergy;
        currentGold = startingGold;
    }

    private void Start()
    {
        // 초기 이벤트 발생
        OnEnergyChanged?.Invoke(currentEnergy);
        OnGoldChanged?.Invoke(currentGold);

        Debug.Log($"ResourceManager initialized - Energy: {currentEnergy}, Gold: {currentGold}");
    }

    private void Update()
    {
        // 시간 경과에 따른 에너지 증가
        energyTimer += Time.deltaTime;
        if (energyTimer >= energyTickInterval)
        {
            energyTimer -= energyTickInterval;
            AddEnergy(energyPerTick);
        }
    }

    public void AddEnergy(int amount)
    {
        if (amount <= 0) return;

        currentEnergy += amount;
        OnEnergyChanged?.Invoke(currentEnergy);
        Debug.Log($"Energy +{amount} → {currentEnergy}");
    }

    public bool SpendEnergy(int amount)
    {
        if (amount <= 0) return false;
        if (currentEnergy < amount) return false;

        currentEnergy -= amount;
        OnEnergyChanged?.Invoke(currentEnergy);
        Debug.Log($"Energy -{amount} → {currentEnergy}");
        return true;
    }

    public bool HasEnoughEnergy(int amount)
    {
        return currentEnergy >= amount;
    }

    public void AddGold(int amount)
    {
        if (amount <= 0) return;

        currentGold += amount;
        OnGoldChanged?.Invoke(currentGold);
        Debug.Log($"Gold +{amount} → {currentGold}");
    }

    public bool SpendGold(int amount)
    {
        if (amount <= 0) return false;
        if (currentGold < amount) return false;

        currentGold -= amount;
        OnGoldChanged?.Invoke(currentGold);
        Debug.Log($"Gold -{amount} → {currentGold}");
        return true;
    }

    public bool HasEnoughGold(int amount)
    {
        return currentGold >= amount;
    }

    // 테스트/디버그용 리셋
    public void ResetResources()
    {
        currentEnergy = startingEnergy;
        currentGold = startingGold;
        OnEnergyChanged?.Invoke(currentEnergy);
        OnGoldChanged?.Invoke(currentGold);
    }
}
