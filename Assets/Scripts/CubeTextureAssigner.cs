using UnityEngine;

public class CubeTextureAssigner : MonoBehaviour
{
    [Tooltip("Assign textures for each face of the cube in the order: X+, X-, Y+, Y-, Z+, Z-")]
    public Texture2D[] faceTextures; // Textures for the cube faces

    void Start()
    {
        // Ensure we have exactly 6 textures
        if (faceTextures == null || faceTextures.Length != 6)
        {
            Debug.LogError("Please assign exactly 6 textures for the cube faces in the inspector.");
            return;
        }

        // Get the MeshRenderer component
        MeshRenderer renderer = GetComponent<MeshRenderer>();
        if (renderer == null)
        {
            Debug.LogError("No MeshRenderer found on this GameObject. Ensure it has a MeshRenderer component.");
            return;
        }

        // Create materials for each face and assign their textures
        Material[] materials = new Material[6];
        for (int i = 0; i < 6; i++)
        {
            materials[i] = new Material(Shader.Find("Custom/TetriminoShader")); // Use the custom shader
            materials[i].mainTexture = faceTextures[i]; // Assign the texture

            // Assign face-specific tints
            switch (i)
            {
                case 0: materials[i].SetColor("_BaseColor", Color.white); materials[i].SetColor("_TintXPositive", Color.red); break;
                case 1: materials[i].SetColor("_BaseColor", Color.white); materials[i].SetColor("_TintXNegative", Color.green); break;
                case 2: materials[i].SetColor("_BaseColor", Color.white); materials[i].SetColor("_TintYPositive", Color.blue); break;
                case 3: materials[i].SetColor("_BaseColor", Color.white); materials[i].SetColor("_TintYNegative", Color.yellow); break;
                case 4: materials[i].SetColor("_BaseColor", Color.white); materials[i].SetColor("_TintZPositive", Color.cyan); break;
                case 5: materials[i].SetColor("_BaseColor", Color.white); materials[i].SetColor("_TintZNegative", Color.magenta); break;
            }
        }

        // Assign the materials to the renderer
        renderer.materials = materials;
    }
}
