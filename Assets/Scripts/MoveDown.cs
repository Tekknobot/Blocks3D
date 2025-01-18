using UnityEngine;

public class MoveDownButton : MonoBehaviour
{
    private bool isHolding = false; // Tracks whether the button is being held down
    public DynamicUIController dynamicUIController; // Reference to the UI controller for calling the method
    public float holdInterval = 0.1f; // Interval between each movement while holding the button
    private float lastHoldTime; // Tracks the time of the last movement

    private void Update()
    {
        if (isHolding && Time.time - lastHoldTime >= holdInterval)
        {
            MoveDownContinuously();
            lastHoldTime = Time.time;
        }
    }

    private void OnMouseDown()
    {
        isHolding = true; // Start continuous movement when the button is pressed
        lastHoldTime = Time.time; // Reset the hold time to ensure immediate movement
        MoveDownContinuously(); // Perform an immediate move on click
    }

    private void OnMouseUp()
    {
        isHolding = false; // Stop continuous movement when the button is released
    }

    private void MoveDownContinuously()
    {
        if (dynamicUIController != null)
        {
            dynamicUIController.MoveDownContinuously();
        }
    }
}
