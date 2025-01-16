using UnityEngine;

public class GridVisualizer : MonoBehaviour
{
    public int gridWidth = 10;
    public int gridHeight = 20;
    public float cellSize = 1.0f;

    public GameObject cellPrefab; // Prefab for a single cell
    private GameObject[,] mechanicsGrid; // Visual grid for mechanics (hidden)
    private GameObject[,] displayGrid;   // Visual-only grid (displayed)

    void Start()
    {
        InitializeGrids();
        VisualizeGrids();
    }

    void InitializeGrids()
    {
        // Initialize both grids
        mechanicsGrid = new GameObject[gridWidth, gridHeight];
        displayGrid = new GameObject[gridWidth, gridHeight];
    }

    void VisualizeGrids()
    {
        // Create cells for both grids
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                mechanicsGrid[x, y] = CreateCell(x, y, "MechanicsGrid", false); // Hidden
                displayGrid[x, y] = CreateCell(x, y, "DisplayGrid", true); // Visible
            }
        }
    }

    GameObject CreateCell(int x, int y, string parentName, bool isVisible)
    {
        // Create a parent object for organizational purposes
        Transform parent = transform.Find(parentName);
        if (parent == null)
        {
            GameObject parentObject = new GameObject(parentName);
            parentObject.transform.SetParent(transform);
            parent = parentObject.transform;
        }

        // Instantiate and position the cell
        GameObject cell = Instantiate(cellPrefab, parent);
        cell.transform.position = new Vector3(x * cellSize, y * cellSize, 0);

        // Name the cell for debugging
        cell.name = $"{parentName} Cell ({x}, {y})";

        // Scale the cell (optional)
        cell.transform.localScale = Vector3.one * (cellSize * 0.9f);

        // Set visibility
        cell.SetActive(isVisible);

        return cell;
    }

    public void UpdateMechanicsCellState(int x, int y, bool isActive, Color color = default)
    {
        // Use Color.clear as the default for inactive cells
        if (!isActive && color == default)
        {
            color = Color.clear; // Or replace with a color representing "empty" (e.g., Color.black)
        }

        UpdateCellState(mechanicsGrid, x, y, isActive, color);
    }


    public void UpdateDisplayCellState(int x, int y, bool isActive, Color color = default)
    {
        UpdateCellState(displayGrid, x, y, isActive, color);
    }

    private void UpdateCellState(GameObject[,] grid, int x, int y, bool isActive, Color color)
    {
        if (x < 0 || x >= gridWidth || y < 0 || y >= gridHeight) return;

        GameObject cell = grid[x, y];
        if (cell != null)
        {
            cell.SetActive(isActive);

            Renderer renderer = cell.GetComponent<Renderer>();
            if (renderer != null && renderer.material.HasProperty("_Color"))
            {
                // Reset color for inactive cells
                if (!isActive)
                {
                    renderer.material.SetColor("_Color", Color.clear); // Default for empty cells
                }
                else
                {
                    renderer.material.SetColor("_Color", color != default ? color : Color.white);
                }
            }
        }
    }
}
