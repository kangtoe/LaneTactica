using UnityEngine;

public enum CellState
{
    Empty,
    Occupied
}

public class GridCell : MonoBehaviour
{
    [Header("Position")]
    [SerializeField] private int row;
    [SerializeField] private int col;

    [Header("State")]
    [SerializeField] private CellState state = CellState.Empty;

    [Header("Materials")]
    [SerializeField] private Material normalMaterial;
    [SerializeField] private Material highlightMaterial;
    [SerializeField] private Material occupiedMaterial;

    private Renderer cellRenderer;
    private GameObject occupyingUnit;

    public int Row => row;
    public int Col => col;
    public CellState State => state;
    public bool IsEmpty => state == CellState.Empty;
    public GameObject OccupyingUnit => occupyingUnit;

    private void Awake()
    {
        cellRenderer = GetComponent<Renderer>();
    }

    public void Initialize(int row, int col, Material normal, Material highlight, Material occupied)
    {
        this.row = row;
        this.col = col;
        this.normalMaterial = normal;
        this.highlightMaterial = highlight;
        this.occupiedMaterial = occupied;

        UpdateVisual();
    }

    public bool Occupy(GameObject unit)
    {
        if (state != CellState.Empty)
            return false;

        state = CellState.Occupied;
        occupyingUnit = unit;
        UpdateVisual();
        return true;
    }

    public void Free()
    {
        state = CellState.Empty;
        occupyingUnit = null;
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        if (cellRenderer == null) return;

        cellRenderer.material = state == CellState.Occupied ? occupiedMaterial : normalMaterial;
    }

    /// <summary>
    /// 하이라이트 표시 (GridManager에서 호출)
    /// </summary>
    public void SetHighlight(bool highlighted)
    {
        if (cellRenderer == null) return;

        if (highlighted && state == CellState.Empty && highlightMaterial != null)
        {
            cellRenderer.material = highlightMaterial;
        }
        else
        {
            UpdateVisual();
        }
    }

}
