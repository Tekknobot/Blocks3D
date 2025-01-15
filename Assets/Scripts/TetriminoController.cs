using UnityEngine;

public class TetriminoController : MonoBehaviour
{
    public float moveDelay = 0.5f; // Time between automatic downward movement
    private float lastMoveTime;

    private TetrisGrid grid; // Reference to the logical TetrisGrid

    [Tooltip("Drag the block to be used as the center pivot")]
    public Transform centerBlock; // Reference to the chosen center block
    private bool isLocked;
    public static float baseDropDelay = 0.5f; // Default drop delay
    public bool isClearingRows = false;

    public static TetriminoController Instance;

    void Awake()
    {
        Instance = this; // Register this instance
    }

    void Start()
    {
        CenterPivot();

        // Reference the grid object in the scene
        grid = FindObjectOfType<TetrisGrid>();
    }

    void Update()
    {
        if (isLocked || grid.isClearingRows) return; // Do nothing if locked or clearing rows
        HandleInput();
        AutoMoveDown();
    }

    void HandleInput()
    {
        // Handle keyboard input
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Move(Vector3.left);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Move(Vector3.right);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            Move(Vector3.down);
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            HardDrop();
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            RotateRight();
        }
    }

    // Automatically moves the piece down after a delay
    void AutoMoveDown()
    {
        float currentDropDelay = baseDropDelay;

        if (Time.time - lastMoveTime > currentDropDelay)
        {
            if (!Move(Vector3.down))
            {
                LockPiece();
            }
            lastMoveTime = Time.time;
        }
    }

    public void HardDrop()
    {
        while (Move(Vector3.down)) { }
    }

    // Moves the Tetrimino in the specified direction
    public bool Move(Vector3 direction)
    {
        transform.position += direction;

        // Check for collisions
        if (!IsValidPosition())
        {
            transform.position -= direction; // Undo move if invalid
            return false;
        }

        SnapToGrid(); // Align the child blocks to the grid
        return true;
    }

    // Rotates the Tetrimino to the right
    public void RotateRight()
    {
        Rotate(90); // Rotate 90 degrees clockwise
    }

    // Rotates the Tetrimino to the left
    public void RotateLeft()
    {
        Rotate(-90); // Rotate 90 degrees counterclockwise
    }

    // General rotation logic
    private void Rotate(float angle)
    {
        if (gameObject.name.Contains("O")) // Prevent rotation for "O" Tetrimino
        {
            Debug.Log("Tetrimino_O does not rotate.");
            return;
        }

        transform.Rotate(0, 0, angle); // Rotate the parent object

        // Check for collisions
        if (!IsValidPosition())
        {
            transform.Rotate(0, 0, -angle); // Undo rotation if invalid
        }
        else
        {
            SnapToGrid(); // Align the child blocks to the grid
            SoundManager.Instance.PlaySound(SoundManager.Instance.rotateSound);
        }
    }

    void SnapToGrid()
    {
        foreach (Transform block in transform)
        {
            // Align each block to the grid
            Vector3 localSnappedPosition = new Vector3(
                Mathf.Floor(block.localPosition.x),
                Mathf.Floor(block.localPosition.y),
                Mathf.Floor(block.localPosition.z)
            );

            block.localPosition = localSnappedPosition;
        }
    }

    void LockPiece()
    {
        if (grid.isClearingRows) return; // Prevent locking if rows are being cleared

        foreach (Transform block in transform)
        {
            if (block.position.y >= 19)
            {
                Debug.Log("Game Over!");
                grid.GameOver();
                SoundManager.Instance.PlaySound(SoundManager.Instance.gameOverSound);
                return;
            }
        }

        // Add the piece to the grid and check for complete rows
        grid.AddToGrid(transform);
        grid.CheckForCompleteRows();

        if (isLocked) return;

        isLocked = true;
        SnapToGrid();

        this.enabled = false;

        FindObjectOfType<TetriminoSpawner>().SpawnTetrimino();
        SoundManager.Instance.PlaySound(SoundManager.Instance.dropSound);
    }

    bool IsValidPosition()
    {
        return grid.IsValidPosition(transform); // Check with the grid logic
    }

    void CenterPivot()
    {
        if (centerBlock == null)
        {
            Debug.LogError("Center block not assigned! Please drag a block in the Inspector.");
            return;
        }

        Vector3 centerBlockLocalPosition = centerBlock.localPosition;

        transform.position += centerBlock.position - transform.position;

        foreach (Transform block in transform)
        {
            block.localPosition -= centerBlockLocalPosition;
        }

        SnapToGrid();
    }
}
