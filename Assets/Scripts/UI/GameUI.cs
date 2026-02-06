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

    // Drag Placement
    private bool isDragging;
    private TowerCard draggingCard;
    private GameObject ghostPreview;
    private GridCell currentHoverCell;
    private Material ghostValidMaterial;
    private Material ghostInvalidMaterial;

    public TowerBase SelectedTower =>
        towerPrefabs != null && selectedIndex >= 0 && selectedIndex < towerPrefabs.Length
            ? towerPrefabs[selectedIndex]
            : null;

    private void Start()
    {
        mainCamera = Camera.main;
        Initialize();
        InitializeGhostMaterials();
    }

    private void InitializeGhostMaterials()
    {
        // 배치 가능 - 반투명 초록
        ghostValidMaterial = new Material(Shader.Find("Standard"));
        ghostValidMaterial.color = new Color(0.2f, 0.8f, 0.2f, 0.5f);
        SetMaterialTransparent(ghostValidMaterial);

        // 배치 불가 - 반투명 빨강
        ghostInvalidMaterial = new Material(Shader.Find("Standard"));
        ghostInvalidMaterial.color = new Color(0.8f, 0.2f, 0.2f, 0.5f);
        SetMaterialTransparent(ghostInvalidMaterial);
    }

    private void SetMaterialTransparent(Material mat)
    {
        mat.SetFloat("_Mode", 3); // Transparent
        mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        mat.SetInt("_ZWrite", 0);
        mat.DisableKeyword("_ALPHATEST_ON");
        mat.EnableKeyword("_ALPHABLEND_ON");
        mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        mat.renderQueue = 3000;
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
        // 드래그 중이 아닐 때만 클릭 배치 허용 (키보드 선택 후 클릭 배치용)
        if (!isDragging)
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

        TryPlaceTower(cell, SelectedTower);
    }

    private bool TryPlaceTower(GridCell cell, TowerBase towerPrefab)
    {
        if (!cell.IsEmpty)
        {
            Debug.Log("Cell is occupied!");
            return false;
        }

        if (!resourceManager.HasEnoughEnergy(towerPrefab.EnergyCost))
        {
            Debug.Log("Not enough energy!");
            return false;
        }

        resourceManager.SpendEnergy(towerPrefab.EnergyCost);
        PlaceTower(cell, towerPrefab);
        return true;
    }

    private void PlaceTower(GridCell cell, TowerBase towerPrefab)
    {
        GameObject towerObj = Instantiate(towerPrefab.gameObject);

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

    #region Drag Placement

    public bool CanAffordTower(TowerBase tower)
    {
        if (resourceManager == null || tower == null) return false;
        return resourceManager.HasEnoughEnergy(tower.EnergyCost);
    }

    public void StartDragPlacement(TowerCard card, UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (card == null || card.Tower == null) return;

        isDragging = true;
        draggingCard = card;

        // 고스트 프리뷰 생성
        CreateGhostPreview(card.Tower);
        UpdateGhostPosition(eventData.position);
    }

    public void UpdateDragPlacement(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (!isDragging || ghostPreview == null) return;

        UpdateGhostPosition(eventData.position);
    }

    public void EndDragPlacement(UnityEngine.EventSystems.PointerEventData eventData)
    {
        if (!isDragging) return;

        // 유효한 셀에 드롭했는지 확인
        if (currentHoverCell != null && currentHoverCell.IsEmpty)
        {
            TryPlaceTower(currentHoverCell, draggingCard.Tower);
        }

        // 정리
        CleanupDrag();
    }

    private void CreateGhostPreview(TowerBase towerPrefab)
    {
        // 기존 고스트 제거
        if (ghostPreview != null)
            Destroy(ghostPreview);

        // 타워 프리팹 복제
        ghostPreview = Instantiate(towerPrefab.gameObject);
        ghostPreview.name = "GhostPreview";

        // 모든 컴포넌트 비활성화 (시각적 요소만 유지)
        DisableGhostComponents(ghostPreview);

        // 반투명 머티리얼 적용
        ApplyGhostMaterial(ghostPreview, ghostValidMaterial);
    }

    private void DisableGhostComponents(GameObject ghost)
    {
        // Collider 비활성화
        foreach (var collider in ghost.GetComponentsInChildren<Collider>())
            collider.enabled = false;

        // TowerBase 및 기타 스크립트 비활성화
        foreach (var behaviour in ghost.GetComponentsInChildren<MonoBehaviour>())
        {
            if (!(behaviour is Renderer))
                behaviour.enabled = false;
        }

        // Rigidbody 제거
        foreach (var rb in ghost.GetComponentsInChildren<Rigidbody>())
            Destroy(rb);
    }

    private void ApplyGhostMaterial(GameObject ghost, Material mat)
    {
        foreach (var renderer in ghost.GetComponentsInChildren<Renderer>())
        {
            Material[] mats = new Material[renderer.materials.Length];
            for (int i = 0; i < mats.Length; i++)
                mats[i] = mat;
            renderer.materials = mats;
        }
    }

    private void UpdateGhostPosition(Vector2 screenPos)
    {
        if (ghostPreview == null || mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(screenPos);

        // 그리드 셀에 레이캐스트
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            GridCell cell = hit.collider.GetComponent<GridCell>();

            if (cell != null)
            {
                // 셀 위치에 고스트 배치
                Vector3 pos = cell.transform.position;
                pos.y = 0.5f;
                ghostPreview.transform.position = pos;

                // 하이라이트 업데이트
                UpdateCellHighlight(cell);

                // 배치 가능 여부에 따라 색상 변경
                bool canPlace = cell.IsEmpty;
                ApplyGhostMaterial(ghostPreview, canPlace ? ghostValidMaterial : ghostInvalidMaterial);

                currentHoverCell = cell;
            }
            else
            {
                // 그리드 밖 - 월드 위치에 표시
                SetGhostToWorldPosition(ray);
                ClearCellHighlight();
                ApplyGhostMaterial(ghostPreview, ghostInvalidMaterial);
                currentHoverCell = null;
            }
        }
        else
        {
            // 아무것도 없음 - 레이 방향으로 일정 거리에 표시
            SetGhostToWorldPosition(ray);
            ClearCellHighlight();
            ApplyGhostMaterial(ghostPreview, ghostInvalidMaterial);
            currentHoverCell = null;
        }
    }

    private void SetGhostToWorldPosition(Ray ray)
    {
        // 그리드 평면(Y=0)과의 교차점 계산
        Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
        if (groundPlane.Raycast(ray, out float distance))
        {
            Vector3 pos = ray.GetPoint(distance);
            pos.y = 0.5f;
            ghostPreview.transform.position = pos;
        }
    }

    private void UpdateCellHighlight(GridCell cell)
    {
        if (currentHoverCell != null && currentHoverCell != cell)
            currentHoverCell.SetDragHighlight(false, false);

        cell.SetDragHighlight(true, cell.IsEmpty);
    }

    private void ClearCellHighlight()
    {
        if (currentHoverCell != null)
            currentHoverCell.SetDragHighlight(false, false);
    }

    private void CleanupDrag()
    {
        isDragging = false;
        draggingCard = null;

        if (ghostPreview != null)
        {
            Destroy(ghostPreview);
            ghostPreview = null;
        }

        ClearCellHighlight();
        currentHoverCell = null;
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
