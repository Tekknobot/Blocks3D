using UnityEngine;

[ExecuteInEditMode]
public class CameraEffectSelector : MonoBehaviour
{
    public Material effectMaterial;

    public enum EffectType
    {
        None,
        Effect_A,
        Effect_B,
        Effect_C,
        Effect_D
    }

    public EffectType selectedEffect = EffectType.None;

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        if (effectMaterial != null)
        {
            // Pass the selected effect to the shader
            effectMaterial.SetInt("_EffectType", (int)selectedEffect);

            // Apply the shader
            Graphics.Blit(source, destination, effectMaterial);
        }
        else
        {
            // Default behavior (no effect)
            Graphics.Blit(source, destination);
        }
    }
}
