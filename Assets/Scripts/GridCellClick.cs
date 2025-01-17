using UnityEngine;

public class GridCellClick : MonoBehaviour
{
    public int effectType; // Effect type this cell triggers (0 = None, 1 = A, 2 = B, etc.)
    public Material cameraEffectMaterial; // Reference to the shader material

    void OnMouseDown()
    {
        if (cameraEffectMaterial != null)
        {
            // Set the effect type on the material
            cameraEffectMaterial.SetFloat("_EffectType", effectType);
        }
    }
}
