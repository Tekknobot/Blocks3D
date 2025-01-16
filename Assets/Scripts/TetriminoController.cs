using UnityEngine;
using UnityEngine.EventSystems;

public class TetriminoController : MonoBehaviour
{
    public float moveDelay = 0.5f; // Time between automatic downward movement
    private float lastMoveTime;

    private TetrisGrid grid; // Reference to the logical TetrisGrid

    [Tooltip("Drag the block to be used as the center pivot")]
    public Transform centerBlock; // Reference to the chosen center block
    private bool isLocked;
    private Vector3 previousMousePosition;
    public static float baseDropDelay = 0.5f; // Default drop delay
    public bool isClearingRows = false;

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
            // Hard drop
            while (Move(Vector3.down)) { }
        }
        else if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            //Rotate();
        }

        // Handle mouse input
        HandleMouseInput();
    }

    void HandleMouseInput()
    {
        // Check if the pointer is over a UI element
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            // Do nothing if the click is over a UI element
            return;
        }
                
        if (Input.GetMouseButtonDown(0)) // Left mouse click
        {
            // Get mouse position in screen space
            Vector3 mousePosition = Input.mousePosition;

            // Check if the click is in the top half for fast drop
            if (IsInTopHalf(mousePosition))
            {
                while (Move(Vector3.down)) { } // Fast drop
            }
            else
            {
                // Determine which screen region was clicked
                if (IsInLeftRegion(mousePosition))
                {
                    Move(Vector3.left);
                }
                else if (IsInRightRegion(mousePosition))
                {
                    Move(Vector3.right);
                }
                else if (IsInMiddleRegion(mousePosition))
                {
                    //Rotate();
                }
            }
        }
    }

    bool IsInLeftRegion(Vector3 mousePosition)
    {
        return mousePosition.x < Screen.width / 3f; // Left third of the screen
    }

    bool IsInRightRegion(Vector3 mousePosition)
    {
        return mousePosition.x > Screen.width * 2f / 3f; // Right third of the screen
    }

    bool IsInMiddleRegion(Vector3 mousePosition)
    {
        return mousePosition.x >= Screen.width / 3f && mousePosition.x <= Screen.width * 2f / 3f; // Middle third
    }

    bool IsInTopHalf(Vector3 mousePosition)
    {
        return mousePosition.y > Screen.height / 2f; // Top half of the screen
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

    public void HardDrop()
    {
        while (Move(Vector3.down)) { }
    }

    // Moves the Tetrimino in the specified direction
    public bool Move(Vector3 direction)
    {
        // Move the parent object
        transform.position += direction;

        // Check for collisions
        if (!IsValidPosition())
        {
            // Undo move if invalid
            transform.position -= direction;
            return false;
        }

        // Align the child blocks to the grid
        SnapToGrid();
        return true;
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
            // Calculate the floored position for each block
            Vector3 localSnappedPosition = new Vector3(
                Mathf.Floor(block.localPosition.x),
                Mathf.Floor(block.localPosition.y),
                Mathf.Floor(block.localPosition.z)
            );

            // Apply the floored position to the block's local position
            block.localPosition = localSnappedPosition;
        }
    }


    // Locks the piece into the grid
    void LockPiece()
    {
        // Prevent locking if rows are being cleared
        if (grid.isClearingRows) return;        
        
        foreach (Transform block in transform)
        {
            if (block.position.y >= 19)
            {
                Debug.Log("Game Over!");
                FindObjectOfType<TetrisGrid>().GameOver();
                return;
            }
        }

        // Existing locking logic
        FindObjectOfType<TetrisGrid>().AddToGrid(transform);
        FindObjectOfType<TetrisGrid>().CheckForCompleteRows();

        // Prevent multiple locks
        if (isLocked) return;

        isLocked = true; // Mark the piece as locked

        // Snap all blocks to the grid
        SnapToGrid();

        // Add the piece to the grid
        grid.AddToGrid(transform);

        // Check for complete rows
        grid.CheckForCompleteRows();

        // Disable this script to prevent further movement or rotation
        this.enabled = false;

        // Spawn a new piece
        FindObjectOfType<TetriminoSpawner>().SpawnTetrimino();     
    }


    // Checks if the current position of the Tetrimino is valid
    bool IsValidPosition()
    {
        // Check with the grid logic
        return grid.IsValidPosition(transform);
    }

    void CenterPivot()
    {
        if (centerBlock == null)
        {
            Debug.LogError("Center block not assigned! Please drag a block in the Inspector.");
            return;
        }

        // Get the offset to the center block
        Vector3 centerBlockLocalPosition = centerBlock.localPosition;

        // Move the parent to align with the center block
        transform.position += centerBlock.position - transform.position;

        // Adjust child block positions to maintain relative alignment
        foreach (Transform block in transform)
        {
            block.localPosition -= centerBlockLocalPosition;
        }

        // Ensure blocks remain aligned to the grid
        SnapToGrid();
    }


}