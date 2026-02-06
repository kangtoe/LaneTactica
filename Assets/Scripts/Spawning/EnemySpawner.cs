using UnityEngine;

/// <summary>
/// 테스트용 적 스포너
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("Enemy Prefab")]
    [SerializeField] private GameObject enemyPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private float spawnX = 6f;
    [SerializeField] private float randomOffset = 0.2f;

    [Header("References")]
    [SerializeField] private GridManager gridManager;

    private float spawnTimer;

    private void Start()
    {
        if (gridManager == null)
        {
            gridManager = FindAnyObjectByType<GridManager>();
        }

        spawnTimer = spawnInterval;
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentState != GameState.Playing) return;
        if (enemyPrefab == null) return;

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            SpawnEnemy();
        }
    }

    private void SpawnEnemy()
    {
        // 랜덤 레인 선택
        int lane = Random.Range(0, gridManager.Rows);

        // 스폰 위치 계산
        Vector3 spawnPos = GetSpawnPosition(lane);

        // 적 생성
        GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        // 레인 설정
        var enemy = enemyObj.GetComponent<EnemyBase>();
        if (enemy != null)
        {
            enemy.SetLane(lane);
        }

        Debug.Log($"Spawned enemy at lane {lane}");
    }

    private Vector3 GetSpawnPosition(int lane)
    {
        float cellSize = gridManager.CellSize;
        float spacing = 0.1f;

        float totalHeight = gridManager.Rows * (cellSize + spacing) - spacing;
        float z = -totalHeight / 2 + cellSize / 2 + lane * (cellSize + spacing);

        // 랜덤 오프셋 추가
        float offsetZ = Random.Range(-randomOffset, randomOffset);

        return new Vector3(spawnX, 0.5f, z + offsetZ);
    }

    [ContextMenu("Spawn Enemy Now")]
    public void SpawnEnemyNow()
    {
        if (enemyPrefab != null && gridManager != null)
        {
            SpawnEnemy();
        }
    }
}
