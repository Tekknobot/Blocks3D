using System.Collections;
using System.Collections.Generic;
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

        currentDifficultyLevel = 1; // Reset difficulty level
        TetriminoController.baseDropDelay = baseDropDelay; // Reset drop delay

        int highScore = LoadHighScore();
        highScoreText.text = "" + highScore;        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q)) // Press Space to rotate
        {
            StartCoroutine(RotateGridQuads()); // Rotate with a 3D twist over 1 second
        }
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

        Debug.Log($"Clearing row {row}");

        // Create a list to hold the blocks that will be cleared
        List<GameObject> blocksToClear = new List<GameObject>();

        Vector3 explosionCenter = new Vector3(gridWidth / 2f, row, 0f);
        float explosionForce = 13f;
        float explosionRadius = 8f;
        float upwardModifier = 5f;

        for (int x = 0; x < gridWidth; x++)
        {
            if (grid[x, row] != null)
            {
                GameObject block = grid[x, row].gameObject;

                // Skip non-Tetrimino blocks (e.g., visual GridCells)
                if (block.CompareTag("GridCell"))
                {
                    continue;
                }

                // Add the block to the list for flashing
                blocksToClear.Add(block);

                // Add Rigidbody dynamically if missing
                Rigidbody blockRigidbody = block.GetComponent<Rigidbody>();
                if (blockRigidbody == null)
                {
                    blockRigidbody = block.AddComponent<Rigidbody>();
                    blockRigidbody.isKinematic = false; // Enable physics interactions
                    blockRigidbody.useGravity = true;  // Enable gravity
                }

                // Apply explosion force
                blockRigidbody.AddExplosionForce(
                    explosionForce,
                    explosionCenter,
                    explosionRadius,
                    upwardModifier,
                    ForceMode.Impulse
                );

                // Schedule block destruction
                Destroy(block, 3f);
                grid[x, row] = null; // Remove from the logical grid
            }
        }

        // Flash the blocks before explosion effect
        //StartCoroutine(FlashBlocksWhite(blocksToClear, 1f));

        // Start coroutine to shift rows down
        StartCoroutine(ShiftRowsDownWithDelay(row, FindObjectsOfType<TetriminoController>()));
    }

    private IEnumerator ShiftRowsDownWithDelay(int clearedRow, TetriminoController[] activeTetriminos)
    {
        // Wait for the explosion effect to complete
        yield return new WaitForSeconds(1f);

        // Shift mechanical grid rows down
        for (int y = clearedRow; y < gridHeight - 1; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                grid[x, y] = grid[x, y + 1];

                if (grid[x, y] != null)
                {
                    grid[x, y].position += Vector3.down;
                }
            }
        }

        // Clear the topmost row (now shifted down)
        for (int x = 0; x < gridWidth; x++)
        {
            grid[x, gridHeight - 1] = null;
        }

        // Update game state
        rowsCleared++;
        int points = CalculateScore(rowsCleared, currentDifficultyLevel - 1);
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
        rowsClearedText.text = rowsCleared.ToString();
        scoreText.text = score.ToString();

        // Increase difficulty based on total rows cleared
        if (rowsCleared / rowsPerDifficultyIncrease >= currentDifficultyLevel)
        {
            IncreaseDifficulty();
            currentDifficultyLevel++;
        }
    }

    void IncreaseDifficulty()
    {
        Debug.Log("Difficulty increased!");

        // Reduce the base drop delay in TetriminoController
        TetriminoController.baseDropDelay = Mathf.Max(
            TetriminoController.baseDropDelay - dropDelayDecrease,
            0.1f // Minimum delay
        );

        // Rotate individual quads in the visual grid
        StartCoroutine(RotateGridQuads());
    }

    private IEnumerator RotateGridQuads()
    {
        float rotationDuration = 0.5f; // Duration of each rotation animation
        GameObject[,] displayGrid = visualizer.GetDisplayGrid(); // Get the display grid from GridVisualizer

        // Calculate the maximum diagonal index
        int maxDiagonalIndex = gridWidth + gridHeight - 2;

        for (int diagonalIndex = 0; diagonalIndex <= maxDiagonalIndex; diagonalIndex++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                int y = diagonalIndex - x; // Calculate y based on diagonal index

                // Ensure the calculated (x, y) is within bounds
                if (y >= 0 && y < gridHeight)
                {
                    GameObject quad = displayGrid[x, y];
                    if (quad != null)
                    {
                        StartCoroutine(RotateQuad(quad.transform, rotationDuration));
                    }
                }
            }

            // Add a delay between each diagonal wave
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator RotateQuad(Transform quad, float duration)
    {
        Quaternion initialRotation = quad.rotation;

        // Target rotation: Flip to the opposite side with a 3D twist
        Quaternion targetRotation = initialRotation * Quaternion.Euler(0, 0, 180);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            // Smoothly interpolate between the initial and target rotations
            quad.rotation = Quaternion.Lerp(initialRotation, targetRotation, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ensure the final rotation is exact
        quad.rotation = targetRotation;
    }


    public void CheckForCompleteRows()
    {
        int rowsClearedThisCheck = 0;

        // Start from the bottom row and move up
        for (int y = gridHeight - 1; y >= 0; y--)
        {
            if (IsRowFull(y))
            {
                ClearRow(y);
                rowsClearedThisCheck++;

                // Recheck the same row since rows above have shifted down
                y++;
            }
        }

        // Add score based on rows cleared
        if (rowsClearedThisCheck > 0)
        {
            int points = CalculateScore(rowsClearedThisCheck, currentDifficultyLevel - 1);
            score += points;
            UpdateUI();
        }

        // Play sound effects based on rows cleared
        if (rowsClearedThisCheck == 4)
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.tetrimino_4_RowSound);
        }
        else if (rowsClearedThisCheck > 0)
        {
            SoundManager.Instance.PlaySound(SoundManager.Instance.rowClearSound);
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
        SoundManager.Instance.PlaySound(SoundManager.Instance.gameOverSound);

        Debug.Log("Game Over!");
        CheckAndSaveHighScore();

        Vector3 explosionCenter = new Vector3(gridWidth / 2f, gridHeight / 2f, 0f);
        float explosionForce = 13f; // Adjust the explosion force
        float explosionRadius = 8f; // Adjust the radius to cover the grid
        float upwardModifier = 5f; // Optional upward force

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
        // Reset difficulty-related variables
        currentDifficultyLevel = 1;
        TetriminoController.baseDropDelay = baseDropDelay;

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

    private int CalculateScore(int rowsCleared, int level)
    {
        switch (rowsCleared)
        {
            case 1:
                return 40 * (level + 1);
            case 2:
                return 100 * (level + 1);
            case 3:
                return 300 * (level + 1);
            case 4:
                return 1200 * (level + 1);
            default:
                return 0; // No points for clearing 0 rows
        }
    }

    private IEnumerator FlashBlocksWhite(List<GameObject> blocks, float duration)
    {
        float elapsedTime = 0f; // Track elapsed time
        float flashInterval = 0.1f; // Interval between flashes (adjust for faster/slower flashing)

        // Store the original materials of the blocks
        Dictionary<GameObject, Material> originalMaterials = new Dictionary<GameObject, Material>();
        foreach (GameObject block in blocks)
        {
            Renderer renderer = block.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = renderer.material;
                if (mat.HasProperty("_FlashAmount"))
                {
                    originalMaterials[block] = mat;
                }
            }
        }

        // Alternate flash state (on/off) for the duration
        bool flashOn = true; // Start with flash "on"
        while (elapsedTime < duration)
        {
            float flashAmount = flashOn ? 1.0f : 0.0f; // Toggle between 1 and 0

            // Update _FlashAmount property on each block's material
            foreach (var pair in originalMaterials)
            {
                if (pair.Value.HasProperty("_FlashAmount"))
                {
                    pair.Value.SetFloat("_FlashAmount", flashAmount);
                }
            }

            // Toggle the flash state
            flashOn = !flashOn;

            // Wait for the next flash interval
            elapsedTime += flashInterval;
            yield return new WaitForSeconds(flashInterval);
        }

        // Reset _FlashAmount to 0 at the end for all blocks
        foreach (var pair in originalMaterials)
        {
            if (pair.Value.HasProperty("_FlashAmount"))
            {
                pair.Value.SetFloat("_FlashAmount", 0f);
            }
        }
    }


}
