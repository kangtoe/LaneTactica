using UnityEngine;
using UnityEngine.InputSystem;

public class GridManager : MonoBehaviour
{
    [Header("Grid Settings")]
    [SerializeField] private int rows = 5;
    [SerializeField] private int cols = 9;
    [SerializeField] private float cellSize = 1f;
    [SerializeField] private float cellHeight = 0.1f;
    [SerializeField] private float cellSpacing = 0.1f;

    [Header("Materials")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material occupiedMaterial;

    private GridCell[,] grid;
    private GridCell currentHoveredCell;
    private Camera mainCamera;

    public int Rows => rows;
    public int Cols => cols;
    public float CellSize => cellSize;

    private void Start()
    {
        mainCamera = Camera.main;
        CreateMaterials();
        GenerateGrid();
    }

    private void Update()
    {
        HandleMouseInput();
    }

    private void HandleMouseInput()
    {
        // 마우스 위치로 레이캐스트
        Vector2 mousePos = Mouse.current.position.ReadValue();
        Ray ray = mainCamera.ScreenPointToRay(mousePos);

        GridCell hitCell = null;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            hitCell = hit.collider.GetComponent<GridCell>();
        }

        // 하이라이트 업데이트
        if (hitCell != currentHoveredCell)
        {
            // 이전 셀 하이라이트 해제
            if (currentHoveredCell != null)
            {
                currentHoveredCell.SetHighlight(false);
            }

            // 새 셀 하이라이트
            currentHoveredCell = hitCell;
            if (currentHoveredCell != null)
            {
                currentHoveredCell.SetHighlight(true);
            }
        }

        // 타워 배치는 GameUI에서 처리
    }

    private void CreateMaterials()
    {
        // 머티리얼이 할당되지 않은 경우 코드로 생성
        if (normalMaterial == null)
        {
            normalMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            normalMaterial.color = new Color(0.5f, 0.5f, 0.5f); // 회색
        }

        if (highlightMaterial == null)
        {
            highlightMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            highlightMaterial.color = new Color(1f, 1f, 0.3f); // 노란색
        }

        if (occupiedMaterial == null)
        {
            occupiedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            occupiedMaterial.color = new Color(0.8f, 0.3f, 0.3f); // 빨간색
        }
    }

    private void GenerateGrid()
    {
        grid = new GridCell[rows, cols];

        // 그리드 중앙 정렬을 위한 오프셋 계산
        float totalWidth = cols * (cellSize + cellSpacing) - cellSpacing;
        float totalHeight = rows * (cellSize + cellSpacing) - cellSpacing;
        Vector3 startPos = new Vector3(-totalWidth / 2 + cellSize / 2, 0, -totalHeight / 2 + cellSize / 2);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                Vector3 position = startPos + new Vector3(
                    col * (cellSize + cellSpacing),
                    0,
                    row * (cellSize + cellSpacing)
                );

                GridCell cell = CreateCell(position, row, col);
                grid[row, col] = cell;
            }
        }

        Debug.Log($"Grid generated: {rows}x{cols}");
    }

    private GridCell CreateCell(Vector3 position, int row, int col)
    {
        GameObject cellObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cellObj.name = $"Cell_{row}_{col}";
        cellObj.transform.parent = transform;
        cellObj.transform.position = position;
        cellObj.transform.localScale = new Vector3(cellSize, cellHeight, cellSize);

        GridCell cell = cellObj.AddComponent<GridCell>();
        cell.Initialize(row, col, normalMaterial, highlightMaterial, occupiedMaterial);

        return cell;
    }

    public GridCell GetCell(int row, int col)
    {
        if (row < 0 || row >= rows || col < 0 || col >= cols)
            return null;

        return grid[row, col];
    }

    public bool CanPlace(int row, int col)
    {
        GridCell cell = GetCell(row, col);
        return cell != null && cell.IsEmpty;
    }

    public bool OccupyCell(int row, int col, GameObject unit)
    {
        GridCell cell = GetCell(row, col);
        if (cell == null) return false;

        return cell.Occupy(unit);
    }

    public void FreeCell(int row, int col)
    {
        GridCell cell = GetCell(row, col);
        cell?.Free();
    }

    public Vector3 GetCellWorldPosition(int row, int col)
    {
        GridCell cell = GetCell(row, col);
        if (cell == null) return Vector3.zero;

        return cell.transform.position;
    }

    /// <summary>
    /// 월드 좌표를 그리드 좌표로 변환
    /// </summary>
    public bool WorldToGrid(Vector3 worldPos, out int row, out int col)
    {
        float totalWidth = cols * (cellSize + cellSpacing) - cellSpacing;
        float totalHeight = rows * (cellSize + cellSpacing) - cellSpacing;

        float localX = worldPos.x + totalWidth / 2;
        float localZ = worldPos.z + totalHeight / 2;

        col = Mathf.FloorToInt(localX / (cellSize + cellSpacing));
        row = Mathf.FloorToInt(localZ / (cellSize + cellSpacing));

        return row >= 0 && row < rows && col >= 0 && col < cols;
    }
}
