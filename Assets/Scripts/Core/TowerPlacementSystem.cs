using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 타워 배치 시스템 (레거시 - GameUI 사용 권장)
/// GameUI가 없을 경우 폴백으로 동작
/// </summary>
[DefaultExecutionOrder(100)]
public class TowerPlacementSystem : MonoBehaviour
{
    [Header("Tower Prefabs")]
    [SerializeField] private TowerBase[] towerPrefabs;
    [SerializeField] private int selectedIndex = 0;

    [Header("UI")]
    [SerializeField] private Text selectedTowerText;

    [Header("References")]
    [SerializeField] private GridManager gridManager;
    [SerializeField] private ResourceManager resourceManager;

    private Camera mainCamera;
    private bool isDisabled;

    // 외부에서 타워 프리팹 목록 접근용
    public TowerBase[] TowerPrefabs => towerPrefabs;

    public TowerBase SelectedTower =>
        towerPrefabs != null && selectedIndex >= 0 && selectedIndex < towerPrefabs.Length
            ? towerPrefabs[selectedIndex]
            : null;

    private void Start()
    {
        // GameUI가 있으면 이 컴포넌트 비활성화
        var gameUI = FindAnyObjectByType<GameUI>();
        if (gameUI != null)
        {
            Debug.Log("GameUI found. TowerPlacementSystem disabled.");
            isDisabled = true;
            return;
        }

        mainCamera = Camera.main;

        if (gridManager == null)
            gridManager = FindAnyObjectByType<GridManager>();
        if (resourceManager == null)
            resourceManager = FindAnyObjectByType<ResourceManager>();

        UpdateUI();
    }

    private void Update()
    {
        if (isDisabled) return;
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        HandleTowerSelection();
        HandleTowerPlacement();
    }

    private void HandleTowerSelection()
    {
        if (towerPrefabs == null || towerPrefabs.Length == 0) return;

        for (int i = 0; i < Mathf.Min(towerPrefabs.Length, 9); i++)
        {
            if (Keyboard.current[Key.Digit1 + i].wasPressedThisFrame)
            {
                selectedIndex = i;
                UpdateUI();
            }
        }
    }

    private void UpdateUI()
    {
        if (selectedTowerText == null) return;
        if (SelectedTower == null)
        {
            selectedTowerText.text = "No Tower Selected";
            return;
        }

        selectedTowerText.text = $"[{selectedIndex + 1}] {SelectedTower.UnitName}\nCost: {SelectedTower.EnergyCost}";
    }

    private void HandleTowerPlacement()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;
        if (SelectedTower == null) return;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        GridCell cell = hit.collider.GetComponent<GridCell>();
        if (cell == null) return;

        TryPlaceTower(cell);
    }

    private void TryPlaceTower(GridCell cell)
    {
        if (!cell.IsEmpty)
        {
            Debug.Log("Cell is occupied!");
            return;
        }

        if (!resourceManager.HasEnoughEnergy(SelectedTower.EnergyCost))
        {
            Debug.Log("Not enough energy!");
            return;
        }

        resourceManager.SpendEnergy(SelectedTower.EnergyCost);
        PlaceTower(cell);
    }

    private void PlaceTower(GridCell cell)
    {
        // 프리팹 인스턴스 생성
        GameObject towerObj = Instantiate(SelectedTower.gameObject);

        // 위치 설정
        Vector3 pos = cell.transform.position;
        pos.y = 0.5f;
        towerObj.transform.position = pos;

        // 그리드 위치 설정
        var tower = towerObj.GetComponent<TowerBase>();
        if (tower != null)
        {
            tower.SetGridPosition(cell.Row, cell.Col);
        }

        // 셀 점유
        cell.Occupy(towerObj);

        Debug.Log($"Placed tower at [{cell.Row}, {cell.Col}]");

        // 배치 후 선택 해제
        selectedIndex = -1;
        UpdateUI();
    }
}
