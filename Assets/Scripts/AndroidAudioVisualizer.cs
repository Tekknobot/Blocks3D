using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class AndroidAudioVisualizer : MonoBehaviour
{
    public int spectrumSize = 64; // Number of spectrum samples
    public float heightMultiplier = 10f; // Scale for visualizer height
    public float reactionSpeed = 10f; // Speed of reaction (higher = faster)
    public AnimationCurve frequencyCurve; // Custom scaling curve

    private LineRenderer lineRenderer;
    private Vector3[] positions;
    private float[] spectrumData;
    private float[] smoothedSpectrumData; // Holds smoothed values

    void Start()
    {
        #if UNITY_ANDROID
        // Initialize LineRenderer
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = spectrumSize;

        positions = new Vector3[spectrumSize];
        spectrumData = new float[spectrumSize];
        smoothedSpectrumData = new float[spectrumSize]; // Initialize smoothing array
        #else
        Debug.Log("Audio Visualizer is disabled because it's not running on Android.");
        enabled = false; // Disable the script on non-Android platforms
        #endif
    }

    void Update()
    {
        #if UNITY_ANDROID
        UpdateSpectrumData();
        SmoothSpectrumData();
        UpdateVisualizer();
        #endif
    }

    private void UpdateSpectrumData()
    {
        // Get spectrum data from the AudioListener (captures all audio in the scene)
        AudioListener.GetSpectrumData(spectrumData, 0, FFTWindow.Blackman);
    }

    private void SmoothSpectrumData()
    {
        // Smooth the spectrum data to control reaction speed
        for (int i = 0; i < spectrumSize; i++)
        {
            smoothedSpectrumData[i] = Mathf.Lerp(smoothedSpectrumData[i], spectrumData[i], Time.deltaTime * reactionSpeed);
        }
    }

    private void UpdateVisualizer()
    {
        // Get screen dimensions in world space
        Camera mainCamera = Camera.main;
        float screenWidth = mainCamera.aspect * mainCamera.orthographicSize * 2f; // Full width in world space
        float screenHeight = mainCamera.orthographicSize * 2f; // Full height in world space

        float screenLeft = mainCamera.transform.position.x - screenWidth / 2f; // Left edge
        float screenRight = mainCamera.transform.position.x + screenWidth / 2f; // Right edge
        float screenCenterY = mainCamera.transform.position.y; // Vertical center in world space

        // Update LineRenderer positions to span the full screen width and remain centered
        for (int i = 0; i < spectrumSize; i++)
        {
            // Evenly distribute x positions across the full width of the screen
            float x = Mathf.Lerp(screenLeft, screenRight, (float)i / (spectrumSize - 1));

            // Use smoothed spectrum data for visualizer
            float y = screenCenterY + smoothedSpectrumData[i] * frequencyCurve.Evaluate((float)i / spectrumSize) * heightMultiplier;

            positions[i] = new Vector3(x, y, 0);
        }

        // Apply positions to LineRenderer
        lineRenderer.SetPositions(positions);
    }
}
