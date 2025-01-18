using UnityEngine;

public class RotateTetriminoRight : MonoBehaviour
{
    [Header("Dynamic UI Controller Reference")]
    public DynamicUIController uiController;

    private void OnMouseDown()
    {
        if (uiController != null)
        {
            uiController.RotateRightAction();
        }
        else
        {
            Debug.LogWarning("UI Controller is not assigned!");
        }
    }
}
