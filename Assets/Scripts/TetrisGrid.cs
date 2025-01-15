using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static System.Net.Mime.MediaTypeNames;

public class TetrisGrid : MonoBehaviour
{
    public int gridWidth = 10;
    public int gridHeight = 20;
    public Transform[,] grid;
    private GridVisualizer visualizer;
    public int rowsCleared = 0;
    public int score = 0;
    public TMP_Text rowsClearedText;
    public TMP_Text scoreText;
    public float baseDropDelay = 0.5f; // Initial drop delay
    public float dropDelayDecrease = 0.05f; // Amount to decrease the delay per level
    public int rowsPerDifficultyIncrease = 10; // Rows cleared per difficulty increase
    private int currentDifficultyLevel = 1;
    public TMP_Text highScoreText;
    public bool isClearingRows = false;

    void Start()
    {
        grid = new Transform[gridWidth, gridHeight];
        visualizer = FindObjectOfType<GridVisualizer>();

        int highScore = LoadHighScore();
        highScoreText.text = "HIGHSCORE: " + highScore;        
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
        isClearingRows = true;

        // Disable all TetriminoControllers
        var activeTetriminos = FindObjectsOfType<TetriminoController>();
        foreach (var tetrimino in activeTetriminos)
        {
            tetrimino.enabled = false;
        }

        Debug.Log($"Clearing row {row}");

        // Center of the explosion effect
        Vector3 explosionCenter = new Vector3(gridWidth / 2f, row, 0f);
        float explosionForce = 8f; // Adjust explosion force
        float explosionRadius = 3f; // Adjust explosion radius
        float upwardModifier = 2f; // Adjust upward push

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

                // Add Rigidbody component programmatically
                Rigidbody blockRigidbody = block.GetComponent<Rigidbody>();
                if (blockRigidbody == null)
                {
                    blockRigidbody = block.AddComponent<Rigidbody>();
                }

                // Configure the Rigidbody
                blockRigidbody.isKinematic = false; // Allow physics interactions
                blockRigidbody.useGravity = true; // Enable gravity

                // Add explosive force
                blockRigidbody.AddExplosionForce(
                    explosionForce,
                    explosionCenter,
                    explosionRadius,
                    upwardModifier,
                    ForceMode.Impulse
                );

                Debug.Log($"Applying explosion force to block at ({x}, {row})");

                // Schedule destruction after the explosion effect
                Destroy(block, 3f); // Adjust time as needed for effect completion
                grid[x, row] = null; // Clear the block from the logical grid
            }
        }

        // Start coroutine to delay row shifting
        StartCoroutine(ShiftRowsDownWithDelay(row, activeTetriminos));
    }

    private IEnumerator ShiftRowsDownWithDelay(int clearedRow, TetriminoController[] activeTetriminos)
    {
        // Wait for the explosion effect to complete
        yield return new WaitForSeconds(1f); // Adjust delay as needed

        for (int y = clearedRow; y < gridHeight - 1; y++)
        {
            // Skip empty rows
            if (IsRowEmpty(y)) continue;

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

        rowsCleared++;
        // Update score
        int points = (int)(100 * Mathf.Pow(2, rowsCleared - 1)); // Exponential scoring
        score += points;

        UpdateUI();

        // Re-enable TetriminoControllers
        foreach (var tetrimino in activeTetriminos)
        {
            tetrimino.enabled = true;
        }

        isClearingRows = false;
    }

    void UpdateUI()
    {
        rowsClearedText.text = "Cleared: " + rowsCleared;
        scoreText.text = "Score: " + score;

        // Check if it's time to increase difficulty
        if (rowsCleared / rowsPerDifficultyIncrease >= currentDifficultyLevel)
        {
            IncreaseDifficulty();
            currentDifficultyLevel++;
        }
    }

    void IncreaseDifficulty()
    {
        // Notify the player (optional)
        Debug.Log("Difficulty increased!");

        // Reduce the base drop delay in TetriminoController
        TetriminoController.baseDropDelay = Mathf.Max(
            TetriminoController.baseDropDelay - dropDelayDecrease,
            0.1f // Minimum delay
        );
    }    

    public void CheckForCompleteRows()
    {
        int rowsClearedThisCheck = 0; // Track how many rows are cleared in this check

        // Start from the bottom row and move up
        for (int y = gridHeight - 1; y >= 0; y--)
        {
            if (IsRowFull(y))
            {
                ClearRow(y);
                rowsClearedThisCheck++;

                // After clearing a row, recheck the same row index
                y++; // Increment y back because rows above have shifted down
            }
        }
        // Play specific sound effect if 4 rows are cleared at once
        if (rowsClearedThisCheck == 4)
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.tetrimino_4_RowSound);
        }
        else if (rowsClearedThisCheck > 0)
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.rowClearSound); // Regular row clear sound
        }        
    }


    public bool IsRowEmpty(int row)
    {
        for (int x = 0; x < gridWidth; x++)
        {
            if (grid[x, row] != null)
            {
                return false; // Found a block, the row is not empty
            }
        }
        return true; // All cells are empty
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

    public void GameOver()
    {
        Debug.Log("Game Over!");
        CheckAndSaveHighScore();

        Vector3 explosionCenter = new Vector3(gridWidth / 2f, gridHeight / 2f, 0f);
        float explosionForce = 8f; // Adjust the explosion force
        float explosionRadius = 3f; // Adjust the radius to cover the grid
        float upwardModifier = 2f; // Optional upward force

        // Loop through the entire grid and apply the explosion effect to all blocks
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (grid[x, y] != null)
                {
                    GameObject block = grid[x, y].gameObject;

                    // Add Rigidbody component programmatically
                    Rigidbody blockRigidbody = block.GetComponent<Rigidbody>();
                    if (blockRigidbody == null)
                    {
                        blockRigidbody = block.AddComponent<Rigidbody>();
                    }

                    // Configure the Rigidbody
                    blockRigidbody.isKinematic = false; // Allow physics interactions
                    blockRigidbody.useGravity = true; // Enable gravity

                    // Apply the explosive force
                    blockRigidbody.AddExplosionForce(
                        explosionForce,
                        explosionCenter,
                        explosionRadius,
                        upwardModifier,
                        ForceMode.Impulse
                    );

                    // Schedule destruction for the block
                    Destroy(block, 3f); // Adjust the time to suit the effect
                    grid[x, y] = null; // Clear the block from the logical grid
                }
            }
        }

        // Optionally disable input or pause the game
        DisableGameControls();

        // Restart the game after a delay
            Invoke(nameof(RestartGame), 3f); // Adjust delay to match the explosion duration
    }

    void DisableGameControls()
    {
        // Example: Disable all Tetrimino controllers
        TetriminoController[] controllers = FindObjectsOfType<TetriminoController>();
        foreach (var controller in controllers)
        {
            controller.enabled = false;
        }
    }

    void RestartGame()
    {
        // Reload the current scene to reset the game
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void SaveScore()
    {
        PlayerPrefs.SetInt("HighScore", score);
        PlayerPrefs.Save();
        Debug.Log("Score saved: " + score);
    }

    void CheckAndSaveHighScore()
    {
        int highScore = LoadHighScore();

        if (score > highScore)
        {
            Debug.Log("New high score achieved!");
            SaveScore();
        }
    }

    public int LoadHighScore()
    {
        return PlayerPrefs.GetInt("HighScore", 0); // Default to 0 if no high score is saved
    }

}
