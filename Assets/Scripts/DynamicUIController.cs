using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.EventSystems; // Required for Pointer Events

public class DynamicUIController : MonoBehaviour
{
    [Header("Button Style")]
    public Sprite circularButtonSprite; // Assign a circular sprite from the Inspector

    [Header("D-Pad Settings")]
    public Vector2 dPadGroupPosition = new Vector2(50, 50);
    public float dPadButtonSize = 100f;
    public Color dPadButtonColor = Color.gray;
    public string[] dPadButtonTexts = { "Left", "Right", "Down", "Up" };

    [Header("Action Buttons Settings")]
    public Vector2 actionButtonsGroupPosition = new Vector2(-50, 50);
    public Vector2 actionButtonSize = new Vector2(200f, 100f);
    public float actionButtonSpacing = 20f;
    public Color actionButtonColor = Color.blue;
    public string[] actionButtonTexts = { "Rotate Left", "Rotate Right" };
    public float actionButtonScaleFactor = 1.0f; // Scale factor to adjust overall size


    [Header("Button Text Settings")]
    public Font buttonFont;
    public int buttonFontSize = 20;
    public Color buttonTextColor = Color.white;

    [Header("Delay Settings")]
    public float canvasCreationDelay = 1f;

    private TetriminoController tetriminoController;
    private List<Button> buttons = new List<Button>();
    private Coroutine holdCoroutine;
    [Header("Continuous Move Settings")]
    public float continuousMoveDelay = 0.1f; // Delay between continuous moves, adjustable in the Inspector
    public float dPadButtonSizeRatio = 0.1f; // Ratio relative to screen height

    void Start()
    {
        StartCoroutine(CreateCanvasWithDelay());
    }

    private IEnumerator CreateCanvasWithDelay()
    {
        yield return new WaitForSeconds(canvasCreationDelay);

        GameObject canvasGO = new GameObject("GameCanvas");
        Canvas canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;

        // Use a balanced reference resolution
        scaler.referenceResolution = new Vector2(1080, 1920); // Common for mobile devices
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        // Match width or height based on the aspect ratio
        scaler.matchWidthOrHeight = Screen.width > Screen.height ? 0 : 1;

        canvasGO.AddComponent<GraphicRaycaster>();

        // Always create the D-Pad group
        CreateDPadGroup(canvasGO);

        // Check for mobile browsers based on aspect ratio or screen size
        bool isMobileBrowser = (Screen.height > Screen.width && Screen.height < 3000); // Adjust as needed
        if (isMobileBrowser)
        {
            CreateActionButtonsGroup(canvasGO);
        }
        else
        {
            Debug.Log("Action buttons are not displayed on non-mobile browsers.");
        }

        foreach (var button in buttons)
        {
            button.interactable = false;
        }
    }


    void Update()
    {
        foreach (var button in buttons)
        {
            button.interactable = tetriminoController != null;
        }
    }

    public void SetActiveTetrimino(TetriminoController controller)
    {
        Debug.Log("Setting Active Tetrimino...");
        if (holdCoroutine != null)
        {
            Debug.Log("Stopping any running HoldButton coroutine before assigning a new Tetrimino.");
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }
        tetriminoController = controller;
        Debug.Log($"New Active Tetrimino set: {tetriminoController?.gameObject.name}");
    }

    void CreateDPadGroup(GameObject canvasGO)
    {
        GameObject dPadGroup = new GameObject("DPadGroup");
        dPadGroup.transform.SetParent(canvasGO.transform);

        RectTransform dPadGroupRect = dPadGroup.AddComponent<RectTransform>();
        dPadGroupRect.anchorMin = new Vector2(0, 0);
        dPadGroupRect.anchorMax = new Vector2(0, 0);
        dPadGroupRect.pivot = new Vector2(0, 0);
        dPadGroupRect.anchoredPosition = dPadGroupPosition;
        dPadGroupRect.sizeDelta = new Vector2(300f, 300f);

        // Scale button size dynamically based on screen height
        float dynamicDPadButtonSize = Screen.height * dPadButtonSizeRatio;

        // Adjust button offset based on dynamic size
        float buttonOffset = dynamicDPadButtonSize;

        // Create DPad buttons
        CreateButton(dPadGroup, dPadButtonTexts[0], new Vector2(-buttonOffset, 0), dynamicDPadButtonSize, dPadButtonColor, () => MoveAction(Vector3.left));
        CreateButton(dPadGroup, dPadButtonTexts[1], new Vector2(buttonOffset, 0), dynamicDPadButtonSize, dPadButtonColor, () => MoveAction(Vector3.right));
        CreateButton(dPadGroup, dPadButtonTexts[2], new Vector2(0, -buttonOffset), dynamicDPadButtonSize, dPadButtonColor, () => MoveAction(Vector3.down), () => MoveDownContinuously());
        CreateButton(dPadGroup, dPadButtonTexts[3], new Vector2(0, buttonOffset), dynamicDPadButtonSize, dPadButtonColor, () => HardDropAction());
    }

    void CreateActionButtonsGroup(GameObject canvasGO)
    {
        GameObject actionButtonsGroup = new GameObject("ActionButtonsGroup");
        actionButtonsGroup.transform.SetParent(canvasGO.transform);

        RectTransform actionGroupRect = actionButtonsGroup.AddComponent<RectTransform>();
        actionGroupRect.anchorMin = new Vector2(0.5f, 0); // Center horizontally
        actionGroupRect.anchorMax = new Vector2(0.5f, 0);
        actionGroupRect.pivot = new Vector2(0.5f, 0);
        actionGroupRect.anchoredPosition = new Vector2(0, actionButtonsGroupPosition.y); // Keep y position consistent

        // Dynamically calculate action button sizes
        float dynamicButtonWidth = Screen.width * 0.2f * actionButtonScaleFactor;
        float dynamicButtonHeight = Screen.height * 0.1f * actionButtonScaleFactor;

        // Adjust spacing proportionally
        float scaledButtonSpacing = actionButtonSpacing * actionButtonScaleFactor;

        // Center the buttons around the spacing
        float totalWidth = dynamicButtonWidth * 2 + scaledButtonSpacing;
        float leftButtonX = -totalWidth / 2 + dynamicButtonWidth / 2;
        float rightButtonX = leftButtonX + dynamicButtonWidth + scaledButtonSpacing;

        // Create action buttons
        CreateButton(actionButtonsGroup, actionButtonTexts[0], new Vector2(leftButtonX, 0), dynamicButtonWidth, actionButtonColor, () => RotateLeftAction());
        CreateButton(actionButtonsGroup, actionButtonTexts[1], new Vector2(rightButtonX, 0), dynamicButtonWidth, actionButtonColor, () => RotateRightAction());
    }


    void CreateButton(GameObject parent, string name, Vector2 position, float buttonSize, Color buttonColor, System.Action onClickAction, System.Action onHoldAction = null)
    {
        GameObject buttonGO = new GameObject(name + "Button");
        buttonGO.transform.SetParent(parent.transform);

        RectTransform rect = buttonGO.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(buttonSize, buttonSize);
        rect.anchoredPosition = position;

        Button button = buttonGO.AddComponent<Button>();
        button.onClick.AddListener(() => onClickAction());
        buttons.Add(button);

        Image image = buttonGO.AddComponent<Image>();
        image.color = buttonColor;
        if (circularButtonSprite != null)
        {
            image.sprite = circularButtonSprite;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
        }

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(buttonGO.transform);

        Text text = textGO.AddComponent<Text>();
        text.text = name;
        text.alignment = TextAnchor.MiddleCenter;
        text.font = buttonFont != null ? buttonFont : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = buttonFontSize;
        text.color = buttonTextColor;

        RectTransform textRect = textGO.GetComponent<RectTransform>();
        textRect.sizeDelta = rect.sizeDelta;
        textRect.anchoredPosition = Vector2.zero;

        if (onHoldAction != null)
        {
            EventTrigger trigger = buttonGO.AddComponent<EventTrigger>();

            EventTrigger.Entry pointerDownEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            pointerDownEntry.callback.AddListener((_) =>
            {
                Debug.Log($"Starting HoldButton coroutine for {name}");
                holdCoroutine = StartCoroutine(HoldButton(onHoldAction));
            });
            trigger.triggers.Add(pointerDownEntry);

            EventTrigger.Entry pointerUpEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            pointerUpEntry.callback.AddListener((_) =>
            {
                Debug.Log($"Stopping HoldButton coroutine for {name}");
                if (holdCoroutine != null)
                {
                    StopCoroutine(holdCoroutine);
                    holdCoroutine = null;
                }
            });
            trigger.triggers.Add(pointerUpEntry);
        }
    }

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

    void MoveDownContinuously()
    {
        tetriminoController?.Move(Vector3.down);
    }

    private IEnumerator HoldButton(System.Action onHoldAction)
    {
        while (true)
        {
            onHoldAction?.Invoke();
            yield return new WaitForSeconds(continuousMoveDelay);
        }
    }
}
