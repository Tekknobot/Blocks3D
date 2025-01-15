using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class DynamicUIController : MonoBehaviour
{
    [Header("Button Style")]
    public Sprite circularButtonSprite; // Assign a circular sprite from the Inspector

    // Public variables for customization
    [Header("D-Pad Settings")]
    public Vector2 dPadGroupPosition = new Vector2(50, 50); // Position of the D-Pad group
    public float dPadButtonSize = 100f; // Size of individual D-Pad buttons
    public Color dPadButtonColor = Color.gray; // Background color for D-Pad buttons
    public string[] dPadButtonTexts = { "Left", "Right", "Down", "Up" }; // Text for D-Pad buttons

    [Header("Action Buttons Settings")]
    public Vector2 actionButtonsGroupPosition = new Vector2(-50, 50); // Position of the action buttons group
    public Vector2 actionButtonSize = new Vector2(200f, 100f); // Size of action buttons
    public float actionButtonSpacing = 20f; // Spacing between action buttons
    public Color actionButtonColor = Color.blue; // Background color for action buttons
    public string[] actionButtonTexts = { "Rotate Left", "Rotate Right" }; // Text for action buttons

    [Header("Button Text Settings")]
    public Font buttonFont; // Font for button text
    public int buttonFontSize = 20; // Font size for button text
    public Color buttonTextColor = Color.white; // Text color for all buttons

    [Header("Delay Settings")]
    public float canvasCreationDelay = 1f; // Delay before creating the canvas

    private TetriminoController tetriminoController;
    private List<Button> buttons = new List<Button>();

    void Start()
    {
        // Start the delayed canvas creation
        StartCoroutine(CreateCanvasWithDelay());
    }

    private IEnumerator CreateCanvasWithDelay()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(canvasCreationDelay);

        // Create the Canvas
        GameObject canvasGO = new GameObject("GameCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        canvasGO.AddComponent<GraphicRaycaster>();

        // Create D-Pad and Action Buttons Groups
        CreateDPadGroup(canvasGO);
        CreateActionButtonsGroup(canvasGO);

        // Disable buttons initially
        foreach (var button in buttons)
        {
            button.interactable = false;
        }
    }

    void Update()
    {
        // Keep the buttons enabled only when TetriminoController is active
        foreach (var button in buttons)
        {
            button.interactable = tetriminoController != null;
        }
    }

    public void SetActiveTetrimino(TetriminoController controller)
    {
        tetriminoController = controller;
    }

    // Create the D-Pad group
    void CreateDPadGroup(GameObject canvasGO)
    {
        // D-Pad Parent
        GameObject dPadGroup = new GameObject("DPadGroup");
        dPadGroup.transform.SetParent(canvasGO.transform);

        RectTransform dPadGroupRect = dPadGroup.AddComponent<RectTransform>();
        dPadGroupRect.anchorMin = new Vector2(0, 0); // Anchor to bottom-left
        dPadGroupRect.anchorMax = new Vector2(0, 0); // Anchor to bottom-left
        dPadGroupRect.pivot = new Vector2(0, 0);     // Pivot at bottom-left
        dPadGroupRect.anchoredPosition = dPadGroupPosition; // Customizable position
        dPadGroupRect.sizeDelta = new Vector2(300f, 300f); // Arbitrary size for grouping

        // Create D-Pad Buttons
        float buttonOffset = dPadButtonSize; // Button offset for positioning
        CreateButton(dPadGroup, dPadButtonTexts[0], new Vector2(-buttonOffset, 0), dPadButtonSize, dPadButtonColor, () => MoveAction(Vector3.left));
        CreateButton(dPadGroup, dPadButtonTexts[1], new Vector2(buttonOffset, 0), dPadButtonSize, dPadButtonColor, () => MoveAction(Vector3.right));
        CreateButton(dPadGroup, dPadButtonTexts[2], new Vector2(0, -buttonOffset), dPadButtonSize, dPadButtonColor, () => MoveAction(Vector3.down));
        CreateButton(dPadGroup, dPadButtonTexts[3], new Vector2(0, buttonOffset), dPadButtonSize, dPadButtonColor, () => HardDropAction());
    }

    // Create the Action Buttons group
    void CreateActionButtonsGroup(GameObject canvasGO)
    {
        // Action Buttons Parent
        GameObject actionButtonsGroup = new GameObject("ActionButtonsGroup");
        actionButtonsGroup.transform.SetParent(canvasGO.transform);

        RectTransform actionGroupRect = actionButtonsGroup.AddComponent<RectTransform>();
        actionGroupRect.anchorMin = new Vector2(1, 0); // Anchor to bottom-right
        actionGroupRect.anchorMax = new Vector2(1, 0); // Anchor to bottom-right
        actionGroupRect.pivot = new Vector2(1, 0);     // Pivot at bottom-right
        actionGroupRect.anchoredPosition = actionButtonsGroupPosition; // Customizable position

        // Create Action Buttons
        Vector2 leftButtonPosition = new Vector2(-actionButtonSize.x - actionButtonSpacing, 0);
        Vector2 rightButtonPosition = new Vector2(0, 0);

        CreateButton(actionButtonsGroup, actionButtonTexts[0], leftButtonPosition, actionButtonSize.x, actionButtonColor, () => RotateLeftAction());
        CreateButton(actionButtonsGroup, actionButtonTexts[1], rightButtonPosition, actionButtonSize.x, actionButtonColor, () => RotateRightAction());
    }

    // General button creation method
    void CreateButton(GameObject parent, string name, Vector2 position, float buttonSize, Color buttonColor, System.Action onClickAction)
    {
        // Verify the parent is not null
        if (parent == null)
        {
            Debug.LogError($"Parent is null for button: {name}");
            return;
        }

        GameObject buttonGO = new GameObject(name + "Button");
        buttonGO.transform.SetParent(parent.transform);

        // Add RectTransform
        RectTransform rect = buttonGO.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(buttonSize, buttonSize);
        rect.anchoredPosition = position;

        // Add Button
        Button button = buttonGO.AddComponent<Button>();
        button.onClick.AddListener(() => onClickAction());
        buttons.Add(button);

        // Add Image and set to circular sprite if assigned
        Image image = buttonGO.AddComponent<Image>();
        image.color = buttonColor;

        if (circularButtonSprite != null)
        {
            image.sprite = circularButtonSprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
        }
        else
        {
            Debug.LogWarning($"Circular sprite not assigned for button: {name}. Using default color.");
        }

        // Add Text
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform);

        Text text = textGO.AddComponent<Text>();
        text.text = name;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = buttonFont != null ? buttonFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = buttonFontSize;
        text.color = buttonTextColor;

        // Adjust RectTransform (already added automatically by Unity)
        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.sizeDelta = rect.sizeDelta;
        textRect.anchoredPosition = Vector2.zero;
    }

    // Actions with null safety for TetriminoController
    void MoveAction(Vector3 direction)
    {
        tetriminoController?.Move(direction);
    }

    void RotateLeftAction()
    {
        tetriminoController?.RotateLeft();
    }

    void RotateRightAction()
    {
        tetriminoController?.RotateRight();
    }

    void HardDropAction()
    {
        tetriminoController?.HardDrop();
    }
}
