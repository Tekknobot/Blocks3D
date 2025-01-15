using UnityEngine;

public class GridVisualizer : MonoBehaviour
{
    public int gridWidth = 10;
    public int gridHeight = 20;
    public float cellSize = 1.0f;

    public GameObject cellPrefab; // Prefab for a single cell
    private GameObject[,] visualGrid; // Store references to all cell GameObjects

    void Start()
    {
        InitializeGrid();
        VisualizeGrid();
    }

    void InitializeGrid()
    {
        visualGrid = new GameObject[gridWidth, gridHeight];
    }

    void VisualizeGrid()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                CreateCell(x, y);
            }
        }
    }

    void CreateCell(int x, int y)
    {
        // Instantiate and position the cell
        GameObject cell = Instantiate(cellPrefab, transform);
        cell.transform.position = new Vector3(x * cellSize, y * cellSize, 0);

        // Name the cell for debugging
        cell.name = $"Cell ({x}, {y})";

        // Scale the cell (optional)
        cell.transform.localScale = Vector3.one * (cellSize * 0.9f);

        // Store the reference
        visualGrid[x, y] = cell;

        // Deactivate the cell by default
        cell.SetActive(true);
    }

    public void UpdateCellState(int x, int y, bool isActive, Color color = default)
    {
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) return;

        GameObject cell = visualGrid[x, y];
        if (cell != null)
        {
            cell.SetActive(isActive);
            if (isActive && color != default)
            {
                Renderer renderer = cell.GetComponent<Renderer>();
                if (renderer != null && renderer.material.HasProperty("_Color"))
                {
                    renderer.material.SetColor("_Color", color);
                }
            }
        }
    }

}
