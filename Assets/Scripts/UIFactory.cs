using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public static class UIFactory
{
    // Default UI settings
    private static readonly Vector2 DefaultCanvasResolution = new Vector2(1920, 1080);
    private static readonly float DefaultFontSize = 36f;
    private static readonly Color DefaultNormalColor = Color.white;
    
    public static GameObject CreateMainUICanvas(
        float fontSize = 36f,
        Color normalColor = default)
    {
        // Use default colors if not specified
        if (normalColor == default) normalColor = DefaultNormalColor;
        
        // Check if Canvas already exists
        Canvas existingCanvas = Object.FindFirstObjectByType<Canvas>();
        if (existingCanvas != null)
        {
            Debug.Log("UIFactory: Canvas already exists in scene!");
            return existingCanvas.gameObject;
        }
        
        // Create Canvas
        GameObject canvasObj = new GameObject("Main UI Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // Ensure it's on top
        
        // Add CanvasScaler for responsive UI
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = DefaultCanvasResolution;
        scaler.matchWidthOrHeight = 0.5f;
        
        // Add GraphicRaycaster for UI interaction
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Ensure there's an EventSystem for UI interaction
        if (Object.FindFirstObjectByType<EventSystem>() == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<EventSystem>();
            eventSystemObj.AddComponent<StandaloneInputModule>();
            Debug.Log("UIFactory: Created EventSystem for UI interaction");
        }
        
        // Create individual UI components with their respective scripts
        CreateTimerUIComponent(canvasObj.transform, fontSize, normalColor);
        CreatePhaseUIComponent(canvasObj.transform, fontSize, normalColor);
        CreateLoopCounterUIComponent(canvasObj.transform, fontSize, normalColor);
        CreateAttemptCounterUIComponent(canvasObj.transform, fontSize, normalColor);
        CreateLevelCompleteUIComponent(canvasObj.transform, fontSize);
        
        Debug.Log("UIFactory: Main UI Canvas created successfully with separated UI components!");
        return canvasObj;
    }
    
    // New methods to create individual UI components with their management scripts
    public static GameObject CreateTimerUIComponent(Transform parent, float fontSize, Color color)
    {
        Text timerText = CreateTimerText(parent, fontSize, color);
        GameObject timerUIObj = new GameObject("Timer UI Manager");
        timerUIObj.transform.SetParent(parent, false);
        
        TimerUI timerUI = timerUIObj.AddComponent<TimerUI>();
        // Use reflection to set the private timerText field
        var field = typeof(TimerUI).GetField("timerText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(timerUI, timerText);
        
        return timerUIObj;
    }
    
    public static GameObject CreatePhaseUIComponent(Transform parent, float fontSize, Color color)
    {
        Text phaseText = CreatePhaseText(parent, fontSize, color);
        GameObject phaseUIObj = new GameObject("Phase UI Manager");
        phaseUIObj.transform.SetParent(parent, false);
        
        PhaseUI phaseUI = phaseUIObj.AddComponent<PhaseUI>();
        // Use reflection to set the private phaseText field
        var field = typeof(PhaseUI).GetField("phaseText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(phaseUI, phaseText);
        
        return phaseUIObj;
    }
    
    public static GameObject CreateLoopCounterUIComponent(Transform parent, float fontSize, Color color)
    {
        Text loopCounterText = CreateLoopCounterText(parent, fontSize, color);
        GameObject loopCounterUIObj = new GameObject("Loop Counter UI Manager");
        loopCounterUIObj.transform.SetParent(parent, false);
        
        LoopCounterUI loopCounterUI = loopCounterUIObj.AddComponent<LoopCounterUI>();
        // Use reflection to set the private loopCounterText field
        var field = typeof(LoopCounterUI).GetField("loopCounterText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(loopCounterUI, loopCounterText);
        
        return loopCounterUIObj;
    }
    
    public static GameObject CreateAttemptCounterUIComponent(Transform parent, float fontSize, Color color)
    {
        Text attemptCounterText = CreateAttemptCounterText(parent, fontSize, color);
        GameObject attemptCounterUIObj = new GameObject("Attempt Counter UI Manager");
        attemptCounterUIObj.transform.SetParent(parent, false);
        
        AttemptCounterUI attemptCounterUI = attemptCounterUIObj.AddComponent<AttemptCounterUI>();
        // Use reflection to set the private attemptCounterText field
        var field = typeof(AttemptCounterUI).GetField("attemptCounterText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        field?.SetValue(attemptCounterUI, attemptCounterText);
        
        return attemptCounterUIObj;
    }
    
    public static GameObject CreateLevelCompleteUIComponent(Transform parent, float fontSize)
    {
        var levelCompleteComponents = CreateLevelCompletePanel(parent, fontSize);
        GameObject levelCompleteUIObj = new GameObject("Level Complete UI Manager");
        levelCompleteUIObj.transform.SetParent(parent, false);
        
        LevelCompleteUI levelCompleteUI = levelCompleteUIObj.AddComponent<LevelCompleteUI>();
        
        // Use reflection to set the private fields
        var panelField = typeof(LevelCompleteUI).GetField("levelCompletePanel", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        panelField?.SetValue(levelCompleteUI, levelCompleteComponents.panel);
        
        var titleField = typeof(LevelCompleteUI).GetField("levelCompleteTitleText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        titleField?.SetValue(levelCompleteUI, levelCompleteComponents.titleText);
        
        var rankField = typeof(LevelCompleteUI).GetField("levelCompleteRankText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        rankField?.SetValue(levelCompleteUI, levelCompleteComponents.rankText);
        
        var messageField = typeof(LevelCompleteUI).GetField("levelCompleteMessageText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        messageField?.SetValue(levelCompleteUI, levelCompleteComponents.messageText);
        
        var detailsField = typeof(LevelCompleteUI).GetField("levelCompleteDetailsText", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        detailsField?.SetValue(levelCompleteUI, levelCompleteComponents.detailsText);
        
        var buttonField = typeof(LevelCompleteUI).GetField("levelCompleteCloseButton", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        buttonField?.SetValue(levelCompleteUI, levelCompleteComponents.closeButton);
        
        return levelCompleteUIObj;
    }
    
    private static Text CreateTimerText(Transform parent, float fontSize, Color color)
    {
        GameObject timerTextObj = new GameObject("Timer Text");
        timerTextObj.transform.SetParent(parent, false);
        
        Text timerTextComponent = timerTextObj.AddComponent<Text>();
        timerTextComponent.text = "Time: 00:30";
        timerTextComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        timerTextComponent.fontSize = (int)fontSize;
        timerTextComponent.color = color;
        timerTextComponent.alignment = TextAnchor.MiddleCenter;
        
        // Position timer at top center
        RectTransform timerRect = timerTextComponent.GetComponent<RectTransform>();
        timerRect.anchorMin = new Vector2(0.5f, 1f);
        timerRect.anchorMax = new Vector2(0.5f, 1f);
        timerRect.anchoredPosition = new Vector2(0, -50);
        timerRect.sizeDelta = new Vector2(300, 60);
        
        return timerTextComponent;
    }
    
    private static Text CreatePhaseText(Transform parent, float fontSize, Color color)
    {
        GameObject phaseTextObj = new GameObject("Phase Text");
        phaseTextObj.transform.SetParent(parent, false);
        
        Text phaseTextComponent = phaseTextObj.AddComponent<Text>();
        phaseTextComponent.text = "Player 1: Reach the Goal!";
        phaseTextComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        phaseTextComponent.fontSize = (int)(fontSize * 0.7f);
        phaseTextComponent.color = color;
        phaseTextComponent.alignment = TextAnchor.MiddleCenter;
        
        // Position phase text below timer
        RectTransform phaseRect = phaseTextComponent.GetComponent<RectTransform>();
        phaseRect.anchorMin = new Vector2(0.5f, 1f);
        phaseRect.anchorMax = new Vector2(0.5f, 1f);
        phaseRect.anchoredPosition = new Vector2(0, -110);
        phaseRect.sizeDelta = new Vector2(400, 40);
        
        return phaseTextComponent;
    }
    
    private static Text CreateLoopCounterText(Transform parent, float fontSize, Color color)
    {
        GameObject loopCounterObj = new GameObject("Loop Counter Text");
        loopCounterObj.transform.SetParent(parent, false);
        
        Text loopCounterComponent = loopCounterObj.AddComponent<Text>();
        loopCounterComponent.text = "0/1 Loops";
        loopCounterComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        loopCounterComponent.fontSize = (int)(fontSize * 0.8f);
        loopCounterComponent.color = color;
        loopCounterComponent.alignment = TextAnchor.MiddleCenter;
        
        // Position loop counter below phase text
        RectTransform loopCounterRect = loopCounterComponent.GetComponent<RectTransform>();
        loopCounterRect.anchorMin = new Vector2(0.5f, 1f);
        loopCounterRect.anchorMax = new Vector2(0.5f, 1f);
        loopCounterRect.anchoredPosition = new Vector2(0, -160);
        loopCounterRect.sizeDelta = new Vector2(200, 30);
        
        return loopCounterComponent;
    }
    
    private static Text CreateAttemptCounterText(Transform parent, float fontSize, Color color)
    {
        GameObject attemptCounterObj = new GameObject("Attempt Counter Text");
        attemptCounterObj.transform.SetParent(parent, false);
        
        Text attemptCounterComponent = attemptCounterObj.AddComponent<Text>();
        attemptCounterComponent.text = "1";
        attemptCounterComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        attemptCounterComponent.fontSize = (int)(fontSize * 0.6f);
        attemptCounterComponent.color = color;
        attemptCounterComponent.alignment = TextAnchor.UpperRight;
        
        // Position attempt counter in top-right corner
        RectTransform attemptCounterRect = attemptCounterComponent.GetComponent<RectTransform>();
        attemptCounterRect.anchorMin = new Vector2(1f, 1f);
        attemptCounterRect.anchorMax = new Vector2(1f, 1f);
        attemptCounterRect.anchoredPosition = new Vector2(-20, -20);
        attemptCounterRect.sizeDelta = new Vector2(50, 30); // Smaller width for just the number
        
        return attemptCounterComponent;
    }
    

    
    private static LevelCompleteComponents CreateLevelCompletePanel(Transform parent, float fontSize)
    {
        // Create Level Complete Panel
        GameObject levelCompleteObj = new GameObject("Level Complete Panel");
        levelCompleteObj.transform.SetParent(parent, false);
        
        // Add background image to panel
        Image panelBackground = levelCompleteObj.AddComponent<Image>();
        panelBackground.color = new Color(0f, 0f, 0f, 0.85f); // Slightly more opaque
        
        // Position panel to cover full screen
        RectTransform levelCompleteRect = levelCompleteObj.GetComponent<RectTransform>();
        levelCompleteRect.anchorMin = Vector2.zero;
        levelCompleteRect.anchorMax = Vector2.one;
        levelCompleteRect.offsetMin = Vector2.zero;
        levelCompleteRect.offsetMax = Vector2.zero;
        
        // Create main content container
        GameObject contentContainer = new GameObject("Content Container");
        contentContainer.transform.SetParent(levelCompleteObj.transform, false);
        
        RectTransform contentRect = contentContainer.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.anchoredPosition = Vector2.zero;
        contentRect.sizeDelta = new Vector2(500, 400);
        
        // Create title text
        Text titleText = CreateText(contentContainer.transform, "Title Text", "LEVEL COMPLETE!", fontSize * 1.2f, Color.white);
        RectTransform titleRect = titleText.GetComponent<RectTransform>();
        titleRect.anchoredPosition = new Vector2(0, 150);
        titleRect.sizeDelta = new Vector2(400, 60);
        
        // Create performance rank display
        Text rankText = CreateText(contentContainer.transform, "Rank Text", "S", fontSize * 2f, Color.yellow);
        RectTransform rankRect = rankText.GetComponent<RectTransform>();
        rankRect.anchoredPosition = new Vector2(0, 80);
        rankRect.sizeDelta = new Vector2(100, 80);
        
        // Create performance message
        Text messageText = CreateText(contentContainer.transform, "Message Text", "INCREDIBLE!", fontSize * 0.8f, Color.green);
        RectTransform messageRect = messageText.GetComponent<RectTransform>();
        messageRect.anchoredPosition = new Vector2(0, 35);
        messageRect.sizeDelta = new Vector2(300, 30);
        
        // Create time display
        Text timeText = CreateText(contentContainer.transform, "Time Text", "Time: 00:00", fontSize * 0.7f, Color.white);
        RectTransform timeRect = timeText.GetComponent<RectTransform>();
        timeRect.anchoredPosition = new Vector2(0, -10);
        timeRect.sizeDelta = new Vector2(300, 30);
        
        // Create single close button
        var continueButton = CreateStyledButton(contentContainer.transform, "Continue Button", "Continue", fontSize * 0.6f, 
            new Color(0.2f, 0.6f, 0.2f, 0.9f), Color.white, new Vector2(0, -100), new Vector2(120, 40));
        
        // Hide panel initially
        levelCompleteObj.SetActive(false);
        
        return new LevelCompleteComponents
        {
            panel = levelCompleteObj,
            detailsText = timeText, // We'll use this for time display
            closeButton = continueButton, // Main action button
            
            // New enhanced components
            titleText = titleText,
            rankText = rankText,
            messageText = messageText
        };
    }
    
    // Helper method to create a simple text element
    public static Text CreateText(Transform parent, string name, string text, float fontSize, Color color, TextAnchor alignment = TextAnchor.MiddleCenter)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        
        Text textComponent = textObj.AddComponent<Text>();
        textComponent.text = text;
        textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textComponent.fontSize = (int)fontSize;
        textComponent.color = color;
        textComponent.alignment = alignment;
        
        return textComponent;
    }
    
    // Helper method to create a simple button
    public static Button CreateButton(Transform parent, string name, string buttonText, float fontSize, Color backgroundColor, Color textColor)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        Button buttonComponent = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = backgroundColor;
        
        // Add text to button
        Text buttonTextComponent = CreateText(buttonObj.transform, "Button Text", buttonText, fontSize, textColor);
        
        // Set button text to fill button
        RectTransform buttonTextRect = buttonTextComponent.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;
        
        return buttonComponent;
    }
    
    // Helper method to create a styled button with position and size
    public static Button CreateStyledButton(Transform parent, string name, string buttonText, float fontSize, 
        Color backgroundColor, Color textColor, Vector2 position, Vector2 size)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);
        
        Button buttonComponent = buttonObj.AddComponent<Button>();
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = backgroundColor;
        
        // Position and size the button
        RectTransform buttonRect = buttonComponent.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = size;
        
        // Add text to button
        Text buttonTextComponent = CreateText(buttonObj.transform, "Button Text", buttonText, fontSize, textColor);
        
        // Set button text to fill button
        RectTransform buttonTextRect = buttonTextComponent.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;
        
        return buttonComponent;
    }
}

// Data structures to hold UI components (kept for backward compatibility)
[System.Serializable]
public class TimerUIComponents
{
    public Text timerText;
    public Text phaseText;
    public Text loopCounterText;
    public Text attemptCounterText;
    public GameObject levelCompletePanel;
    public Text levelCompleteDetailsText;
    public Button levelCompleteCloseButton;
    
    // Enhanced level complete UI components
    public Text levelCompleteTitleText;
    public Text levelCompleteRankText;
    public Text levelCompleteMessageText;
}

// New data structure for UI managers
[System.Serializable]
public class UIManagers
{
    public TimerUI timerUI;
    public PhaseUI phaseUI;
    public LoopCounterUI loopCounterUI;
    public AttemptCounterUI attemptCounterUI;
    public LevelCompleteUI levelCompleteUI;
}

[System.Serializable]
public struct LevelCompleteComponents
{
    public GameObject panel;
    public Text detailsText;
    public Button closeButton;
    
    // Enhanced UI components
    public Text titleText;
    public Text rankText;
    public Text messageText;
}