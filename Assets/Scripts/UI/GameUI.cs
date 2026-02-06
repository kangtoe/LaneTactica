using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// 메인 게임 UI 컨트롤러
/// HUD, 게임 상태 패널, 타워 선택 UI를 통합 관리
/// </summary>
public class GameUI : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject hudPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject victoryPanel;
    [SerializeField] private GameObject defeatPanel;

    [Header("HUD Elements")]
    [SerializeField] private Text energyText;
    [SerializeField] private Text goldText;
    [SerializeField] private Text waveText;
    [SerializeField] private Text gameStateText;

    [Header("Tower Selection")]
    [SerializeField] private TowerPlacementSystem towerPlacementSystem;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private TowerCard cardPrefab;
    [SerializeField] private Text selectedTowerText;

    [Header("References")]
    [SerializeField] private ResourceManager resourceManager;
    [SerializeField] private GridManager gridManager;

    // Tower Selection
    private TowerBase[] towerPrefabs;
    private TowerCard[] towerCards;
    private int selectedIndex = -1;
    private Camera mainCamera;

    public TowerBase SelectedTower =>
        towerPrefabs != null && selectedIndex >= 0 && selectedIndex < towerPrefabs.Length
            ? towerPrefabs[selectedIndex]
            : null;

    private void Start()
    {
        mainCamera = Camera.main;
        Initialize();
    }

    private void Initialize()
    {
        // 참조 자동 찾기
        if (towerPlacementSystem == null)
            towerPlacementSystem = FindAnyObjectByType<TowerPlacementSystem>();
        if (resourceManager == null)
            resourceManager = FindAnyObjectByType<ResourceManager>();
        if (gridManager == null)
            gridManager = FindAnyObjectByType<GridManager>();

        // 타워 프리팹 가져오기
        if (towerPlacementSystem != null)
            towerPrefabs = towerPlacementSystem.TowerPrefabs;

        // 이벤트 구독
        if (resourceManager != null)
        {
            resourceManager.OnEnergyChanged += UpdateEnergyDisplay;
            resourceManager.OnGoldChanged += UpdateGoldDisplay;
            resourceManager.OnEnergyChanged += UpdateCardAffordability;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }

        // UI 초기화
        InitializeUI();
        CreateTowerCards();
        UpdateSelection();
    }

    private void OnDestroy()
    {
        if (resourceManager != null)
        {
            resourceManager.OnEnergyChanged -= UpdateEnergyDisplay;
            resourceManager.OnGoldChanged -= UpdateGoldDisplay;
            resourceManager.OnEnergyChanged -= UpdateCardAffordability;
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }
    }

    private void Update()
    {
        if (GameManager.Instance == null) return;
        if (GameManager.Instance.CurrentState != GameState.Playing) return;

        HandleKeyboardSelection();
        HandleTowerPlacement();
    }

    #region UI Initialization

    private void InitializeUI()
    {
        SetPanelActive(pausePanel, false);
        SetPanelActive(victoryPanel, false);
        SetPanelActive(defeatPanel, false);
        SetPanelActive(hudPanel, true);

        if (resourceManager != null)
        {
            UpdateEnergyDisplay(resourceManager.Energy);
            UpdateGoldDisplay(resourceManager.Gold);
        }

        UpdateWaveDisplay(1, 10);
        UpdateGameStateText("Playing");
    }

    private void CreateTowerCards()
    {
        if (towerPrefabs == null || towerPrefabs.Length == 0) return;
        if (cardContainer == null || cardPrefab == null)
        {
            Debug.LogWarning("GameUI: cardContainer or cardPrefab not assigned.");
            return;
        }

        towerCards = new TowerCard[towerPrefabs.Length];

        for (int i = 0; i < towerPrefabs.Length; i++)
        {
            TowerCard card = Instantiate(cardPrefab, cardContainer);
            card.Initialize(i, towerPrefabs[i], this);
            towerCards[i] = card;
        }
    }

    #endregion

    #region Game State

    private void OnGameStateChanged(GameState newState)
    {
        SetPanelActive(pausePanel, false);
        SetPanelActive(victoryPanel, false);
        SetPanelActive(defeatPanel, false);

        switch (newState)
        {
            case GameState.Playing:
                SetPanelActive(hudPanel, true);
                UpdateGameStateText("Playing");
                break;
            case GameState.Paused:
                SetPanelActive(pausePanel, true);
                UpdateGameStateText("Paused");
                break;
            case GameState.Victory:
                SetPanelActive(victoryPanel, true);
                UpdateGameStateText("Victory!");
                break;
            case GameState.Defeat:
                SetPanelActive(defeatPanel, true);
                UpdateGameStateText("Defeat");
                break;
            case GameState.Preparing:
                UpdateGameStateText("Preparing...");
                break;
        }
    }

    #endregion

    #region Display Updates

    private void UpdateEnergyDisplay(int energy)
    {
        if (energyText != null)
            energyText.text = $"Energy: {energy}";
    }

    private void UpdateGoldDisplay(int gold)
    {
        if (goldText != null)
            goldText.text = $"Gold: {gold}";
    }

    public void UpdateWaveDisplay(int currentWave, int totalWaves)
    {
        if (waveText != null)
            waveText.text = $"Wave: {currentWave}/{totalWaves}";
    }

    private void UpdateGameStateText(string state)
    {
        if (gameStateText != null)
            gameStateText.text = state;
    }

    private void UpdateSelectedTowerText()
    {
        if (selectedTowerText == null) return;

        if (SelectedTower == null)
        {
            selectedTowerText.text = "No Tower";
            return;
        }

        selectedTowerText.text = $"[{selectedIndex + 1}] {SelectedTower.UnitName}\nCost: {SelectedTower.EnergyCost}";
    }

    private void UpdateCardAffordability(int energy)
    {
        if (towerCards == null) return;

        foreach (var card in towerCards)
        {
            if (card != null)
                card.UpdateAffordability(energy);
        }
    }

    #endregion

    #region Tower Selection

    private void HandleKeyboardSelection()
    {
        if (towerPrefabs == null || towerPrefabs.Length == 0) return;

        for (int i = 0; i < Mathf.Min(towerPrefabs.Length, 9); i++)
        {
            if (Keyboard.current[Key.Digit1 + i].wasPressedThisFrame)
            {
                SelectTower(i);
            }
        }
    }

    public void SelectTower(int index)
    {
        if (towerPrefabs == null) return;
        if (index < 0 || index >= towerPrefabs.Length) return;

        selectedIndex = index;
        UpdateSelection();
    }

    private void UpdateSelection()
    {
        if (towerCards != null)
        {
            for (int i = 0; i < towerCards.Length; i++)
            {
                if (towerCards[i] != null)
                    towerCards[i].SetSelected(i == selectedIndex);
            }
        }

        UpdateSelectedTowerText();
    }

    #endregion

    #region Tower Placement

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
        GameObject towerObj = Instantiate(SelectedTower.gameObject);

        Vector3 pos = cell.transform.position;
        pos.y = 0.5f;
        towerObj.transform.position = pos;

        var tower = towerObj.GetComponent<TowerBase>();
        if (tower != null)
        {
            tower.SetGridPosition(cell.Row, cell.Col);
        }

        cell.Occupy(towerObj);
        Debug.Log($"Placed tower at [{cell.Row}, {cell.Col}]");

        // 배치 후 선택 해제
        ClearSelection();
    }

    private void ClearSelection()
    {
        selectedIndex = -1;
        UpdateSelection();
    }

    #endregion

    #region Button Callbacks

    public void OnResumeButtonClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ResumeGame();
    }

    public void OnRestartButtonClicked()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RestartGame();
    }

    public void OnQuitButtonClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    #endregion

    private void SetPanelActive(GameObject panel, bool active)
    {
        if (panel != null)
            panel.SetActive(active);
    }
}
