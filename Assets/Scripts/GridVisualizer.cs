using UnityEngine;

public class GridVisualizer : MonoBehaviour
{
    public int gridWidth = 10;
    public int gridHeight = 20;
    public float cellSize = 1.0f;

    public GameObject cellPrefab; // Prefab for a single cell
    public GameObject borderPrefab; // Prefab for the border

    public float borderThickness = 0.1f; // Thickness of the border
    public Color borderColor = Color.white; // Color of the border

    private GameObject[,] mechanicsGrid; // Visual grid for mechanics (hidden)
    private GameObject[,] displayGrid;   // Visual-only grid (displayed)
    public GameObject[,] GetDisplayGrid()
    {
        return displayGrid;
    }
    
    void Start()
    {
        InitializeGrids();
        VisualizeGrids();
        CreateBorder(); // Create the adjustable border
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

    void CreateBorder()
    {
        // Create a parent object for the border
        GameObject borderParent = new GameObject("GridBorder");
        borderParent.transform.SetParent(transform);

        // Calculate grid dimensions
        float borderWidth = gridWidth * cellSize;
        float borderHeight = gridHeight * cellSize;

        // Adjust for grid offset (-0.5) and additional 0.05 buffer
        float buffer = 0.05f;
        float gridOffsetX = -0.5f * cellSize - buffer;
        float gridOffsetY = -0.5f * cellSize - buffer;

        // Adjust positions for even intersection
        float halfBorderThickness = borderThickness / 2.0f;

        // Create border objects (4 sides)
        CreateBorderSide(
            new Vector3(borderWidth / 2 + gridOffsetX, -halfBorderThickness + gridOffsetY, 0), // Bottom border
            new Vector3(borderWidth + 2 * buffer + borderThickness, borderThickness, 1),      // Adjusted width for intersection
            borderParent);

        CreateBorderSide(
            new Vector3(borderWidth / 2 + gridOffsetX, borderHeight + halfBorderThickness + gridOffsetY + 2 * buffer, 0), // Top border
            new Vector3(borderWidth + 2 * buffer + borderThickness, borderThickness, 1),      // Adjusted width for intersection
            borderParent);

        CreateBorderSide(
            new Vector3(-halfBorderThickness + gridOffsetX, borderHeight / 2 + gridOffsetY, 0), // Left border
            new Vector3(borderThickness, borderHeight + 2 * buffer + borderThickness, 1),      // Adjusted height for intersection
            borderParent);

        CreateBorderSide(
            new Vector3(borderWidth + halfBorderThickness + gridOffsetX + 2 * buffer, 
                        borderHeight / 2 + gridOffsetY + 0.05f, 0),                            // Adjusted y position for right border
            new Vector3(borderThickness, borderHeight + 2 * buffer + borderThickness + 0.1f, 1), // Adjusted y scale for right border
            borderParent);
    }

    void CreateBorderSide(Vector3 position, Vector3 scale, GameObject parent)
    {
        if (borderPrefab == null)
        {
            // Use a simple cube if no prefab is provided
            borderPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        }

        GameObject borderSide = Instantiate(borderPrefab, parent.transform);
        borderSide.transform.localPosition = position;
        borderSide.transform.localScale = scale;

        // Set border color
        Renderer renderer = borderSide.GetComponent<Renderer>();
        if (renderer != null && renderer.material.HasProperty("_Color"))
        {
            renderer.material.SetColor("_Color", borderColor);
        }
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
