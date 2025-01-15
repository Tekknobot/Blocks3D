using UnityEngine;

public class TetriminoSpawner : MonoBehaviour
{
    public GameObject[] tetriminoPrefabs; // Array of Tetrimino prefabs
    public Vector3 spawnPosition = new Vector3(5, 18, 0); // Adjust to match the grid's top center
    public GameObject nextTetrimino;

    public Transform previewPosition; // UI position for preview

    public void SpawnTetrimino()
    {
        if (nextTetrimino == null)
        {
            nextTetrimino = Instantiate(tetriminoPrefabs[Random.Range(0, tetriminoPrefabs.Length)]);
        }

        // Set the next piece as the current piece
        GameObject currentTetrimino = nextTetrimino;
        currentTetrimino.transform.position = spawnPosition; // Spawn position
        currentTetrimino.GetComponent<TetriminoController>().enabled = true;

        // Make the piece invisible initially
        SetTetriminoVisibility(currentTetrimino, false);

        // Generate a new next piece for the preview
        nextTetrimino = Instantiate(tetriminoPrefabs[Random.Range(0, tetriminoPrefabs.Length)]);
        nextTetrimino.transform.position = previewPosition.position; // Position in preview
        nextTetrimino.GetComponent<TetriminoController>().enabled = false;

        // Start a coroutine to make the current Tetrimino visible after a delay
        StartCoroutine(ShowTetriminoWhenOnGrid(currentTetrimino));
    }

    void Start()
    {
        // Spawn the first Tetrimino
        SpawnTetrimino();
    }

    private void SetTetriminoVisibility(GameObject tetrimino, bool isVisible)
    {
        foreach (Renderer renderer in tetrimino.GetComponentsInChildren<Renderer>())
        {
            renderer.enabled = isVisible;
        }
    }

    private System.Collections.IEnumerator ShowTetriminoWhenOnGrid(GameObject tetrimino)
    {
        // Wait until the Tetrimino is fully within the grid
        yield return new WaitForSeconds(0.1f); // Adjust delay as needed

        // Make the Tetrimino visible
        SetTetriminoVisibility(tetrimino, true);
    }
}
