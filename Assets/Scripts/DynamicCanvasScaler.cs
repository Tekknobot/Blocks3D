using UnityEngine;
using UnityEngine.UI;

public class DynamicCanvasScaler : MonoBehaviour
{
    public Canvas canvas;
    public Vector2 referenceResolution = new Vector2(1920, 1080); // Set your desired reference resolution
    public float minAspectRatio = 1.3f; // Example: 4:3 aspect ratio
    public float maxAspectRatio = 2.4f; // Example: 21:9 aspect ratio

    void Start()
    {
        // Ensure the Canvas Scaler is properly configured
        CanvasScaler canvasScaler = canvas.GetComponent<CanvasScaler>();
        if (canvasScaler == null)
        {
            Debug.LogError("Canvas Scaler not found. Please add a Canvas Scaler component to your Canvas.");
            return;
        }

        AdjustCanvas(canvasScaler);
    }

    void AdjustCanvas(CanvasScaler canvasScaler)
    {
        float screenAspectRatio = (float)Screen.width / Screen.height;

        if (screenAspectRatio < minAspectRatio)
        {
            Debug.Log("Adjusting UI for narrow screens (e.g., 4:3)");
            canvasScaler.matchWidthOrHeight = 1; // Prioritize height
        }
        else if (screenAspectRatio > maxAspectRatio)
        {
            Debug.Log("Adjusting UI for wide screens (e.g., 21:9)");
            canvasScaler.matchWidthOrHeight = 0; // Prioritize width
        }
        else
        {
            Debug.Log("Adjusting UI for standard screens");
            canvasScaler.matchWidthOrHeight = 0.5f; // Balance between width and height
        }

        canvasScaler.referenceResolution = referenceResolution;
        Debug.Log($"Canvas adjusted for aspect ratio: {screenAspectRatio}");
    }

    void Update()
    {
        // Optional: Dynamically adjust during runtime if screen resolution changes
        if (Screen.width != canvas.GetComponent<RectTransform>().rect.width ||
            Screen.height != canvas.GetComponent<RectTransform>().rect.height)
        {
            AdjustCanvas(canvas.GetComponent<CanvasScaler>());
        }
    }
}
