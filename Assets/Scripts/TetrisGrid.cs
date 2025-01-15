using UnityEngine;

public class TetrisGrid : MonoBehaviour
{
    public int gridWidth = 10;
    public int gridHeight = 20;
    public Transform[,] grid;

    private GridVisualizer visualizer;

    void Start()
    {
        grid = new Transform[gridWidth, gridHeight];
        visualizer = FindObjectOfType<GridVisualizer>();
    }

    public bool IsInsideGrid(Vector3 position)
    {
        return position.x >= 0 && position.x < gridWidth && position.y >= 0;
    }

    public Vector2Int RoundToGrid(Vector3 position)
    {
        return Vector2Int.RoundToInt(new Vector2(position.x, position.y));
    }

    public bool IsValidPosition(Transform tetrimino)
    {
        foreach (Transform block in tetrimino)
        {
            Vector2Int gridPosition = RoundToGrid(block.position);

            if (gridPosition.x < 0 || gridPosition.x >= gridWidth || gridPosition.y < 0)
            {
                return false; // Out of bounds
            }

            if (grid[gridPosition.x, gridPosition.y] != null)
            {
                return false; // Cell already occupied
            }
        }
        return true;
    }

    public void AddToGrid(Transform tetrimino)
    {
        foreach (Transform block in tetrimino)
        {
            Vector2Int gridPosition = RoundToGrid(block.position);

            if (gridPosition.y < gridHeight)
            {
                // Only add Tetrimino blocks to the grid array
                if (!block.CompareTag("GridCell"))
                {
                    grid[gridPosition.x, gridPosition.y] = block;
                    Debug.Log($"Block locked at grid position: {gridPosition}");
                }
            }
        }
    }


    public void ClearRow(int row)
    {
        Debug.Log($"Clearing row {row}"); // Debugging for row clearing

        // Clear all blocks in the given row
        for (int x = 0; x < gridWidth; x++)
        {
            if (grid[x, row] != null)
            {
                GameObject block = grid[x, row].gameObject;

                // Check if the block is a Tetrimino block (not a GridCell)
                if (block.CompareTag("GridCell"))
                {
                    Debug.Log($"Skipping visual GridCell at ({x}, {row})");
                    continue; // Skip visual grid cells
                }

                Debug.Log($"Destroying Tetrimino block at ({x}, {row})");
                Destroy(block); // Destroy only Tetrimino blocks
                grid[x, row] = null; // Clear the block from the logical grid
            }
        }

        // Shift rows above down
        for (int y = row; y < gridHeight - 1; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                grid[x, y] = grid[x, y + 1];

                if (grid[x, y] != null)
                {
                    grid[x, y].position += Vector3.down;
                }

                // Update the visual grid
                visualizer.UpdateMechanicsCellState(
                    x,
                    y,
                    grid[x, y] != null,
                    grid[x, y]?.GetComponent<Renderer>()?.material.color ?? Color.clear
                );
            }
        }

        // Clear the topmost row (now shifted down)
        for (int x = 0; x < gridWidth; x++)
        {
            grid[x, gridHeight - 1] = null;
            visualizer.UpdateMechanicsCellState(x, gridHeight - 1, false);
        }
    }
    public void CheckForCompleteRows()
    {
        for (int y = 0; y < gridHeight; y++)
        {
            if (IsRowFull(y))
            {
                ClearRow(y);

                // After clearing a row, check the same row again as it may now also be full
                y--;
            }
        }
    }

    public bool IsRowFull(int row)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            if (grid[x, row] == null)
            {
                return false; // Found an empty cell
            }
        }
        return true; // All cells are occupied
    }

    public void AddBlockToGrid(Transform block, Vector2Int position)
    {
        // Add the block to the grid array
        grid[position.x, position.y] = block;

        // Snap the block's position to ensure alignment
        block.position = new Vector3(position.x, position.y, block.position.z);
    }
}
