using UnityEngine;

public class TetriminoController : MonoBehaviour
{
    public float moveDelay = 0.5f; // Time between automatic downward movement
    private float lastMoveTime;

    private TetrisGrid grid; // Reference to the logical TetrisGrid

    [Tooltip("Drag the block to be used as the center pivot")]
    public Transform centerBlock; // Reference to the chosen center block
    private bool isLocked;

    void Start()
    {
        CenterPivot();

        // Reference the grid object in the scene
        grid = FindObjectOfType<TetrisGrid>();
    }

    void Update()
    {
        if (isLocked) return; // Do nothing if the piece is locked
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
            Rotate();
        }

        // Handle mouse input
        HandleMouseInput();
    }

    void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0)) // Left click for rotation
        {
            Rotate();
        }
        else if (Input.GetMouseButton(1)) // Right mouse button hold for fast drop
        {
            Move(Vector3.down);
        }

        if (Input.GetMouseButton(2)) // Middle mouse button to hard drop
        {
            while (Move(Vector3.down)) { }
        }

        // Drag for lateral movement
        if (Input.GetMouseButton(0)) // Left mouse button drag
        {
            Vector3 mouseDelta = GetMouseDelta();

            // Normalize mouse delta to a more reasonable scale
            float screenWidth = Screen.width; // Get screen width in pixels
            float screenHeight = Screen.height; // Get screen height in pixels

            // Scale the sensitivity relative to the screen dimensions
            float dragSensitivity = 0.02f; // Adjust sensitivity (lower = slower movement)

            float normalizedDeltaX = mouseDelta.x / screenWidth; // Normalize X movement
            float normalizedDeltaY = mouseDelta.y / screenHeight; // Normalize Y movement

            // Move based on normalized and scaled drag
            if (normalizedDeltaX > dragSensitivity)
            {
                Move(Vector3.right);
            }
            else if (normalizedDeltaX < -dragSensitivity)
            {
                Move(Vector3.left);
            }

            if (normalizedDeltaY < -dragSensitivity) // Drag down for fast drop
            {
                Move(Vector3.down);
            }
        }

    }

    // Track the previous mouse position for calculating drag
    private Vector3 previousMousePosition;

    Vector3 GetMouseDelta()
    {
        Vector3 currentMousePosition = Input.mousePosition;
        Vector3 delta = currentMousePosition - previousMousePosition;

        // Update previous mouse position for the next frame
        previousMousePosition = currentMousePosition;

        return delta;
    }


    // Automatically moves the piece down after a delay
    void AutoMoveDown()
    {
        // Do nothing if the piece is already locked
        if (isLocked) return;

        if (Time.time - lastMoveTime > moveDelay)
        {
            if (!Move(Vector3.down))
            {
                Debug.Log("Piece hit the bottom. Locking...");
                LockPiece();
            }
            lastMoveTime = Time.time;
        }
    }


    // Moves the Tetrimino in the specified direction
    bool Move(Vector3 direction)
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


    // Rotates the Tetrimino
    void Rotate()
    {
        // Check if the Tetrimino is the "O" shape
        if (gameObject.name.Contains("O")) // Assumes the Tetrimino is named "Tetrimino_O"
        {
            Debug.Log("Tetrimino_O does not rotate.");
            return; // Skip rotation for the "O" shape
        }

        // Rotate the parent object
        transform.Rotate(0, 0, 90);

        // Check for collisions
        if (!IsValidPosition())
        {
            // Undo rotation if invalid
            transform.Rotate(0, 0, -90);
        }
        else
        {
            // Align the child blocks to the grid
            SnapToGrid();
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
