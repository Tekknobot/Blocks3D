using UnityEngine;

public class CameraController : MonoBehaviour
{
    public int gridWidth = 10;   // Width of the Tetris grid
    public int gridHeight = 20; // Height of the Tetris grid
    public float cellSize = 1.0f; // Size of each grid cell

    void Start()
    {
        SetCameraPosition();
    }

    void SetCameraPosition()
    {
        // Calculate the center of the grid
        float centerX = (gridWidth * cellSize) / 2f - (cellSize / 2f);
        float centerY = (gridHeight * cellSize) / 2f - (cellSize / 2f);

        // Position the camera to look at the grid center
        transform.position = new Vector3(centerX, centerY, -10f); // Negative Z to face the grid

        // Set the camera to orthographic mode
        Camera mainCamera = GetComponent<Camera>();
        mainCamera.orthographic = true;

        // Adjust the orthographic size to fit the grid
        float gridAspect = (float)gridWidth / gridHeight;
        float screenAspect = (float)Screen.width / Screen.height;

        if (screenAspect >= gridAspect)
        {
            // Wider screen, fit height
            mainCamera.orthographicSize = (gridHeight * cellSize) / 2f;
        }
        else
        {
            // Taller screen, fit width
            mainCamera.orthographicSize = (gridWidth * cellSize) / (2f * screenAspect);
        }
    }
}
