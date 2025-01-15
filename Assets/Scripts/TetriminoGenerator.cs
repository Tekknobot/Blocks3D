using UnityEngine;

public class TetriminoGenerator : MonoBehaviour
{
    public Material tetriminoMaterial; // Assign a base material for Tetrimino blocks
    public float cellSize = 1.0f;      // Size of each block

    void Start()
    {
        GenerateTetriminos();
    }

    // Define names and colors for each Tetrimino shape
    private string[] tetriminoNames = { "I", "O", "T", "S", "Z", "J", "L" };
    private Color[] baseColors = new Color[]
    {
        Color.cyan,    // I Shape
        Color.yellow,  // O Shape
        Color.magenta, // T Shape
        Color.green,   // S Shape
        Color.red,     // Z Shape
        Color.blue,    // J Shape
        new Color(1f, 0.65f, 0f) // L Shape (orange)
    };

    private Color[] glowColors = new Color[]
    {
        new Color(0.5f, 1f, 1f), // Slightly brighter cyan
        new Color(1f, 1f, 0.5f), // Slightly brighter yellow
        new Color(1f, 0.5f, 1f), // Slightly brighter magenta
        new Color(0.5f, 1f, 0.5f), // Slightly brighter green
        new Color(1f, 0.5f, 0.5f), // Slightly brighter red
        new Color(0.5f, 0.5f, 1f), // Slightly brighter blue
        new Color(1f, 0.8f, 0.5f)  // Slightly brighter orange
    };

    // Tetrimino shapes represented as 4x4 grids
    private int[,,] shapes = new int[7, 4, 4]
    {
        // I Shape
        {
            { 0, 0, 0, 0 },
            { 1, 1, 1, 1 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        },
        // O Shape
        {
            { 0, 1, 1, 0 },
            { 0, 1, 1, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        },
        // T Shape
        {
            { 0, 1, 0, 0 },
            { 1, 1, 1, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        },
        // S Shape
        {
            { 0, 1, 1, 0 },
            { 1, 1, 0, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        },
        // Z Shape
        {
            { 1, 1, 0, 0 },
            { 0, 1, 1, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        },
        // J Shape
        {
            { 1, 0, 0, 0 },
            { 1, 1, 1, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        },
        // L Shape
        {
            { 0, 0, 1, 0 },
            { 1, 1, 1, 0 },
            { 0, 0, 0, 0 },
            { 0, 0, 0, 0 }
        }
    };

    public void GenerateTetriminos()
    {
        for (int i = 0; i < shapes.GetLength(0); i++)
        {
            CreateTetriminoPrefab(i);
        }
    }

    void CreateTetriminoPrefab(int shapeIndex)
    {
        // Create a new Tetrimino GameObject and name it dynamically
        GameObject tetrimino = new GameObject($"Tetrimino_{tetriminoNames[shapeIndex]}");
        tetrimino.transform.position = Vector3.zero;

        // Generate the blocks for the Tetrimino
        for (int y = 0; y < 4; y++)
        {
            for (int x = 0; x < 4; x++)
            {
                if (shapes[shapeIndex, y, x] == 1)
                {
                    CreateBlock(tetrimino.transform, x, y, baseColors[shapeIndex], glowColors[shapeIndex]);
                }
            }
        }

        // Save the Tetrimino GameObject as a prefab
        SavePrefab(tetrimino);
    }

    void CreateBlock(Transform parent, int x, int y, Color baseColor, Color glowColor)
    {
        // Create a cube for each block of the Tetrimino
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube); // Creates a valid mesh
        block.transform.SetParent(parent);
        block.transform.localPosition = new Vector3(x * cellSize, y * cellSize, 0);
        block.transform.localScale = Vector3.one * (cellSize * 0.9f); // Slightly smaller for spacing

        if (tetriminoMaterial != null)
        {
            // Create a new material instance for the block
            Material blockMaterial = new Material(tetriminoMaterial);

            // Set the base and glow colors in the shader
            if (blockMaterial.HasProperty("_BaseColor"))
                blockMaterial.SetColor("_BaseColor", baseColor);

            if (blockMaterial.HasProperty("_GlowColor"))
                blockMaterial.SetColor("_GlowColor", glowColor);

            // Apply the material to the block
            block.GetComponent<MeshRenderer>().material = blockMaterial;
        }
    }


    void SavePrefab(GameObject tetrimino)
    {
        #if UNITY_EDITOR
        // Save the Tetrimino GameObject as a prefab in the Prefabs folder
        string prefabPath = $"Assets/Prefabs/{tetrimino.name}.prefab";
        UnityEditor.PrefabUtility.SaveAsPrefabAsset(tetrimino, prefabPath);
        Destroy(tetrimino);
        #endif
    }
}
