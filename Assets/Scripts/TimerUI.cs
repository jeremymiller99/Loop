using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class TimerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text timerText;
    [SerializeField] private Text phaseText;
    [SerializeField] private Text loopCounterText;
    [SerializeField] private Text loopCompletePopupText;
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private Text levelCompleteDetailsText;
    [SerializeField] private Button levelCompleteCloseButton;
    
    [Header("UI Settings")]
    [SerializeField] private float fontSize = 36f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private Color loopCompleteColor = Color.green;
    [SerializeField] private float popupAnimationDuration = 2f;
    
    private PlayerManager playerManager;
    private float totalPlayTime = 0f;
    
    void Start()
    {
        // Find the PlayerManager in the scene
        playerManager = FindFirstObjectByType<PlayerManager>();
        
        if (playerManager == null)
        {
            Debug.LogWarning("TimerUI: No PlayerManager found in scene!");
        }
        
        // Set up UI text if not assigned
        if (timerText == null)
        {
            timerText = GetComponentInChildren<Text>();
        }
        
        // Configure text appearance
        ConfigureUIElements();
        
        // Subscribe to game events
        SubscribeToEvents();
        
        // Initialize loop counter
        UpdateLoopCounter();
        
        // Hide popup elements initially
        if (loopCompletePopupText != null)
            loopCompletePopupText.gameObject.SetActive(false);
        if (levelCompletePanel != null)
            levelCompletePanel.SetActive(false);
    }
    
    void Update()
    {
        // Track total play time
        if (playerManager != null && !playerManager.IsLevelCompleted())
        {
            totalPlayTime += Time.deltaTime;
        }
        
        // Update phase text if available
        if (phaseText != null && playerManager != null)
        {
            switch (playerManager.CurrentPhase)
            {
                case GamePhase.Player1Phase:
                    phaseText.text = "Player 1: Reach the Goal!";
                    break;
                case GamePhase.Player2Phase:
                    phaseText.text = "Player 2: Shoot Player 1!";
                    break;
            }
        }
    }
    
    private void ConfigureUIElements()
    {
        if (timerText != null)
        {
            timerText.fontSize = (int)fontSize;
            timerText.color = normalColor;
            timerText.alignment = TextAnchor.MiddleCenter;
        }
        
        if (phaseText != null)
        {
            phaseText.fontSize = (int)(fontSize * 0.7f);
            phaseText.color = normalColor;
            phaseText.alignment = TextAnchor.MiddleCenter;
        }
        
        if (loopCounterText != null)
        {
            loopCounterText.fontSize = (int)(fontSize * 0.8f);
            loopCounterText.color = normalColor;
            loopCounterText.alignment = TextAnchor.MiddleCenter;
        }
        
        if (loopCompletePopupText != null)
        {
            loopCompletePopupText.fontSize = (int)(fontSize * 1.5f);
            loopCompletePopupText.color = loopCompleteColor;
            loopCompletePopupText.alignment = TextAnchor.MiddleCenter;
        }
    }
    
    private void SubscribeToEvents()
    {
        GameEvents.OnLoopCompleted += OnLoopCompleted;
        GameEvents.OnLevelCompleted += OnLevelCompleted;
    }
    
    private void UnsubscribeFromEvents()
    {
        GameEvents.OnLoopCompleted -= OnLoopCompleted;
        GameEvents.OnLevelCompleted -= OnLevelCompleted;
    }
    
    private void UpdateLoopCounter()
    {
        if (loopCounterText != null && playerManager != null)
        {
            int completedLoops = playerManager.GetCompletedLoops();
            int requiredLoops = playerManager.GetRequiredLoops();
            loopCounterText.text = $"{completedLoops}/{requiredLoops} Loops";
        }
    }
    
    private void OnLoopCompleted(int completedLoops, int totalRequired)
    {
        UpdateLoopCounter();
        StartCoroutine(ShowLoopCompletePopup());
    }
    
    private void OnLevelCompleted()
    {
        ShowLevelCompletePanel();
    }
    
    private IEnumerator ShowLoopCompletePopup()
    {
        if (loopCompletePopupText == null) yield break;
        
        GameObject popupObject = loopCompletePopupText.gameObject;
        RectTransform popupRect = loopCompletePopupText.GetComponent<RectTransform>();
        
        // Show and reset popup
        popupObject.SetActive(true);
        loopCompletePopupText.text = "LOOP COMPLETE!";
        loopCompletePopupText.color = loopCompleteColor;
        
        // Initial state for animation
        Vector3 originalScale = popupRect.localScale;
        Color originalColor = loopCompletePopupText.color;
        
        popupRect.localScale = Vector3.zero;
        loopCompletePopupText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        
        // Animate scale and fade in
        float animTime = 0f;
        float halfDuration = popupAnimationDuration * 0.5f;
        
        while (animTime < halfDuration)
        {
            animTime += Time.deltaTime;
            float progress = animTime / halfDuration;
            
            // Scale animation with bounce effect
            float scale = Mathf.Sin(progress * Mathf.PI) * 1.2f;
            popupRect.localScale = originalScale * scale;
            
            // Fade in
            float alpha = progress;
            loopCompletePopupText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            
            yield return null;
        }
        
        // Hold at full visibility
        popupRect.localScale = originalScale;
        loopCompletePopupText.color = originalColor;
        
        yield return new WaitForSeconds(halfDuration);
        
        // Animate fade out
        animTime = 0f;
        while (animTime < halfDuration)
        {
            animTime += Time.deltaTime;
            float progress = animTime / halfDuration;
            
            // Fade out
            float alpha = 1f - progress;
            loopCompletePopupText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            
            yield return null;
        }
        
        // Hide popup
        popupObject.SetActive(false);
    }
    
    private void ShowLevelCompletePanel()
    {
        if (levelCompletePanel == null) return;
        
        levelCompletePanel.SetActive(true);
        
        if (levelCompleteDetailsText != null && playerManager != null)
        {
            int minutes = Mathf.FloorToInt(totalPlayTime / 60f);
            int seconds = Mathf.FloorToInt(totalPlayTime % 60f);
            int completedLoops = playerManager.GetCompletedLoops();
            int requiredLoops = playerManager.GetRequiredLoops();
            
            string details = $"LEVEL COMPLETE!\n\n" +
                           $"Total Time: {minutes:00}:{seconds:00}\n" +
                           $"Loops Completed: {completedLoops}/{requiredLoops}\n" +
                           $"Average Time per Loop: {(totalPlayTime / completedLoops):F1}s";
            
            levelCompleteDetailsText.text = details;
        }
        
        // Set up close button
        if (levelCompleteCloseButton != null)
        {
            levelCompleteCloseButton.onClick.RemoveAllListeners();
            levelCompleteCloseButton.onClick.AddListener(HideLevelCompletePanel);
        }
    }
    
    private void HideLevelCompletePanel()
    {
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }
    }
    
    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    // Helper method to create UI Canvas automatically
    [ContextMenu("Create Timer UI Canvas")]
    public void CreateTimerCanvas()
    {
        // Check if Canvas already exists
        Canvas existingCanvas = FindFirstObjectByType<Canvas>();
        if (existingCanvas != null)
        {
            Debug.Log("Canvas already exists in scene!");
            return;
        }
        
        // Create Canvas
        GameObject canvasObj = new GameObject("Timer Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // Ensure it's on top
        
        // Add CanvasScaler for responsive UI
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        // Add GraphicRaycaster for UI interaction
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // Create Timer Text
        GameObject timerTextObj = new GameObject("Timer Text");
        timerTextObj.transform.SetParent(canvasObj.transform, false);
        
        Text timerTextComponent = timerTextObj.AddComponent<Text>();
        timerTextComponent.text = "Time: 00:30";
        timerTextComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        timerTextComponent.fontSize = (int)fontSize;
        timerTextComponent.color = normalColor;
        timerTextComponent.alignment = TextAnchor.MiddleCenter;
        
        // Position timer at top center
        RectTransform timerRect = timerTextComponent.GetComponent<RectTransform>();
        timerRect.anchorMin = new Vector2(0.5f, 1f);
        timerRect.anchorMax = new Vector2(0.5f, 1f);
        timerRect.anchoredPosition = new Vector2(0, -50);
        timerRect.sizeDelta = new Vector2(300, 60);
        
        // Create Phase Text
        GameObject phaseTextObj = new GameObject("Phase Text");
        phaseTextObj.transform.SetParent(canvasObj.transform, false);
        
        Text phaseTextComponent = phaseTextObj.AddComponent<Text>();
        phaseTextComponent.text = "Player 1: Reach the Goal!";
        phaseTextComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        phaseTextComponent.fontSize = (int)(fontSize * 0.7f);
        phaseTextComponent.color = normalColor;
        phaseTextComponent.alignment = TextAnchor.MiddleCenter;
        
        // Position phase text below timer
        RectTransform phaseRect = phaseTextComponent.GetComponent<RectTransform>();
        phaseRect.anchorMin = new Vector2(0.5f, 1f);
        phaseRect.anchorMax = new Vector2(0.5f, 1f);
        phaseRect.anchoredPosition = new Vector2(0, -110);
        phaseRect.sizeDelta = new Vector2(400, 40);
        
        // Create Loop Counter Text
        GameObject loopCounterObj = new GameObject("Loop Counter Text");
        loopCounterObj.transform.SetParent(canvasObj.transform, false);
        
        Text loopCounterComponent = loopCounterObj.AddComponent<Text>();
        loopCounterComponent.text = "0/1 Loops";
        loopCounterComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        loopCounterComponent.fontSize = (int)(fontSize * 0.8f);
        loopCounterComponent.color = normalColor;
        loopCounterComponent.alignment = TextAnchor.MiddleCenter;
        
        // Position loop counter below phase text
        RectTransform loopCounterRect = loopCounterComponent.GetComponent<RectTransform>();
        loopCounterRect.anchorMin = new Vector2(0.5f, 1f);
        loopCounterRect.anchorMax = new Vector2(0.5f, 1f);
        loopCounterRect.anchoredPosition = new Vector2(0, -160);
        loopCounterRect.sizeDelta = new Vector2(200, 30);
        
        // Create Loop Complete Popup Text (center screen)
        GameObject loopPopupObj = new GameObject("Loop Complete Popup");
        loopPopupObj.transform.SetParent(canvasObj.transform, false);
        
        Text loopPopupComponent = loopPopupObj.AddComponent<Text>();
        loopPopupComponent.text = "LOOP COMPLETE!";
        loopPopupComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        loopPopupComponent.fontSize = (int)(fontSize * 1.5f);
        loopPopupComponent.color = loopCompleteColor;
        loopPopupComponent.alignment = TextAnchor.MiddleCenter;
        
        // Position popup in center of screen
        RectTransform loopPopupRect = loopPopupComponent.GetComponent<RectTransform>();
        loopPopupRect.anchorMin = new Vector2(0.5f, 0.5f);
        loopPopupRect.anchorMax = new Vector2(0.5f, 0.5f);
        loopPopupRect.anchoredPosition = Vector2.zero;
        loopPopupRect.sizeDelta = new Vector2(600, 80);
        
        // Create Level Complete Panel
        GameObject levelCompleteObj = new GameObject("Level Complete Panel");
        levelCompleteObj.transform.SetParent(canvasObj.transform, false);
        
        // Add background image to panel
        Image panelBackground = levelCompleteObj.AddComponent<Image>();
        panelBackground.color = new Color(0f, 0f, 0f, 0.8f); // Semi-transparent black
        
        // Position panel to cover full screen
        RectTransform levelCompleteRect = levelCompleteObj.GetComponent<RectTransform>();
        levelCompleteRect.anchorMin = Vector2.zero;
        levelCompleteRect.anchorMax = Vector2.one;
        levelCompleteRect.offsetMin = Vector2.zero;
        levelCompleteRect.offsetMax = Vector2.zero;
        
        // Create details text inside panel
        GameObject detailsTextObj = new GameObject("Level Details Text");
        detailsTextObj.transform.SetParent(levelCompleteObj.transform, false);
        
        Text detailsTextComponent = detailsTextObj.AddComponent<Text>();
        detailsTextComponent.text = "LEVEL COMPLETE!\n\nTotal Time: 00:00\nLoops Completed: 0/1";
        detailsTextComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        detailsTextComponent.fontSize = (int)(fontSize * 0.9f);
        detailsTextComponent.color = Color.white;
        detailsTextComponent.alignment = TextAnchor.MiddleCenter;
        
        // Position details text in center
        RectTransform detailsRect = detailsTextComponent.GetComponent<RectTransform>();
        detailsRect.anchorMin = new Vector2(0.5f, 0.5f);
        detailsRect.anchorMax = new Vector2(0.5f, 0.5f);
        detailsRect.anchoredPosition = new Vector2(0, 50);
        detailsRect.sizeDelta = new Vector2(600, 200);
        
        // Create close button
        GameObject closeButtonObj = new GameObject("Close Button");
        closeButtonObj.transform.SetParent(levelCompleteObj.transform, false);
        
        Button closeButtonComponent = closeButtonObj.AddComponent<Button>();
        Image closeButtonImage = closeButtonObj.AddComponent<Image>();
        closeButtonImage.color = new Color(0.2f, 0.6f, 0.2f, 0.8f); // Green button
        
        // Position close button below details
        RectTransform closeButtonRect = closeButtonComponent.GetComponent<RectTransform>();
        closeButtonRect.anchorMin = new Vector2(0.5f, 0.5f);
        closeButtonRect.anchorMax = new Vector2(0.5f, 0.5f);
        closeButtonRect.anchoredPosition = new Vector2(0, -80);
        closeButtonRect.sizeDelta = new Vector2(120, 40);
        
        // Add text to close button
        GameObject closeButtonTextObj = new GameObject("Close Button Text");
        closeButtonTextObj.transform.SetParent(closeButtonObj.transform, false);
        
        Text closeButtonText = closeButtonTextObj.AddComponent<Text>();
        closeButtonText.text = "Close";
        closeButtonText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        closeButtonText.fontSize = (int)(fontSize * 0.6f);
        closeButtonText.color = Color.white;
        closeButtonText.alignment = TextAnchor.MiddleCenter;
        
        RectTransform closeButtonTextRect = closeButtonText.GetComponent<RectTransform>();
        closeButtonTextRect.anchorMin = Vector2.zero;
        closeButtonTextRect.anchorMax = Vector2.one;
        closeButtonTextRect.offsetMin = Vector2.zero;
        closeButtonTextRect.offsetMax = Vector2.zero;
        
        // Assign references
        timerText = timerTextComponent;
        phaseText = phaseTextComponent;
        loopCounterText = loopCounterComponent;
        loopCompletePopupText = loopPopupComponent;
        levelCompletePanel = levelCompleteObj;
        levelCompleteDetailsText = detailsTextComponent;
        levelCompleteCloseButton = closeButtonComponent;
        
        Debug.Log("Timer UI Canvas created successfully!");
    }
}