using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    public int gridWidth = 10;   // Width of the Tetris grid
    public int gridHeight = 20; // Height of the Tetris grid
    public float cellSize = 1.0f; // Size of each grid cell

    [Header("Rotation Effect Settings")]
    public float rotationSpeed = 45f; // Speed of 360-degree rotation (degrees per second)

    [Header("Isometric View Settings")]
    public float isometricRotationX = 30f; // Rotation angle around the X-axis
    public float isometricRotationY = 45f; // Rotation angle around the Y-axis
    public float isometricFieldOfView = 60f; // Field of view for perspective mode
    public float padding = 1.0f; // Padding around the grid in world units

    private bool isOrthographic = true; // Track the current view mode
    private Coroutine transitionCoroutine;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        SetOrthographicView();
    }

    public void TriggerRowClearEffect()
    {
        if (transitionCoroutine != null)
            StopCoroutine(transitionCoroutine);

        transitionCoroutine = StartCoroutine(RotateAroundGrid());
    }

    IEnumerator RotateAroundGrid()
    {
        // Cache the grid center
        Vector3 gridCenter = new Vector3(
            (gridWidth * cellSize) / 2f - (cellSize / 2f),
            (gridHeight * cellSize) / 2f - (cellSize / 2f),
            0f
        );

        // Cache the starting position and rotation
        Vector3 initialPosition = transform.position;
        Quaternion initialRotation = transform.rotation;

        // Perform a full 360-degree rotation
        float elapsedRotation = 0f;
        while (elapsedRotation < 360f)
        {
            float rotationStep = rotationSpeed * Time.deltaTime;
            elapsedRotation += rotationStep;

            // Rotate around the grid center
            transform.RotateAround(gridCenter, Vector3.up, rotationStep);

            yield return null;
        }

        // Reset position and rotation to the initial view
        transform.position = initialPosition;
        transform.rotation = initialRotation;

        // Ensure the camera stays aligned with the orthographic view
        SetOrthographicView();
    }

    void SetOrthographicView()
    {
        // Calculate the center of the grid
        float centerX = (gridWidth * cellSize) / 2f - (cellSize / 2f);
        float centerY = (gridHeight * cellSize) / 2f - (cellSize / 2f);

        transform.position = new Vector3(centerX, centerY, -10f); // Negative Z to face the grid
        transform.rotation = Quaternion.identity; // Reset rotation to default

        mainCamera.orthographic = true;

        float gridAspect = (float)gridWidth / gridHeight;
        float screenAspect = (float)Screen.width / Screen.height;

        if (screenAspect >= gridAspect)
        {
            mainCamera.orthographicSize = ((gridHeight * cellSize) / 2f) + padding;
        }
        else
        {
            mainCamera.orthographicSize = ((gridWidth * cellSize) / (2f * screenAspect)) + padding;
        }

        isOrthographic = true;
    }
}
