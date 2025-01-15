using UnityEngine;

public class TetriminoSpawner : MonoBehaviour
{
    public GameObject[] tetriminoPrefabs; // Array of Tetrimino prefabs
    public Vector3 spawnPosition = new Vector3(5, 18, 0); // Adjust to match the grid's top center
    public GameObject nextTetrimino;

    public Transform previewPosition; // UI position for preview

    private DynamicUIController uiController; // Reference to the UI controller

    void Start()
    {
        // Get reference to the UI controller
        uiController = FindObjectOfType<DynamicUIController>();

        // Spawn the first Tetrimino
        SpawnTetrimino();
    }

    public void SpawnTetrimino()
    {
        // If there's no next piece, generate one
        if (nextTetrimino == null)
        {
            nextTetrimino = Instantiate(tetriminoPrefabs[Random.Range(0, tetriminoPrefabs.Length)]);
        }

        // Set the next piece as the current piece
        GameObject currentTetrimino = nextTetrimino;
        currentTetrimino.transform.position = spawnPosition; // Spawn position
        currentTetrimino.GetComponent<TetriminoController>().enabled = true;

        // Make the piece visible
        SetTetriminoVisibility(currentTetrimino, true);

        // Notify the UI controller about the active Tetrimino
        uiController?.SetActiveTetrimino(currentTetrimino.GetComponent<TetriminoController>());

        // Generate a new next piece for the preview
        nextTetrimino = Instantiate(tetriminoPrefabs[Random.Range(0, tetriminoPrefabs.Length)]);
        nextTetrimino.transform.position = previewPosition.position; // Position in preview
        nextTetrimino.GetComponent<TetriminoController>().enabled = false;

        // Make the preview piece invisible on the grid
        SetTetriminoVisibility(nextTetrimino, true);
    }

    private void SetTetriminoVisibility(GameObject tetrimino, bool isVisible)
    {
        foreach (Renderer renderer in tetrimino.GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = isVisible;
        }
    }
}
