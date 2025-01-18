using UnityEngine;

public class RotateTetriminoLeft : MonoBehaviour
{
    [Header("Dynamic UI Controller Reference")]
    public DynamicUIController uiController;

    private void OnMouseDown()
    {
        if (uiController != null)
        {
            uiController.RotateLeftAction();
        }
        else
        {
            Debug.LogWarning("UI Controller is not assigned!");
        }
    }
}
