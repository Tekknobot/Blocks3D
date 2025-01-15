using UnityEngine;

public class TetriminoSpawner : MonoBehaviour
{
    public GameObject[] tetriminoPrefabs; // Array of Tetrimino prefabs
    public Vector3 spawnPosition = new Vector3(5, 18, 0); // Adjust to match the grid's top center

    public void SpawnTetrimino()
    {
        // Randomly select a Tetrimino prefab
        int randomIndex = Random.Range(0, tetriminoPrefabs.Length);

        // Instantiate the Tetrimino at the spawn position
        Instantiate(tetriminoPrefabs[randomIndex], spawnPosition, Quaternion.identity);
    }

    void Start()
    {
        // Spawn the first Tetrimino
        SpawnTetrimino();
    }
}
