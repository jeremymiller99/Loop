using UnityEngine;
using UnityEngine.UI;

public class TimerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text timerText;
    [SerializeField] private Text phaseText;
    
    [Header("UI Settings")]
    [SerializeField] private float fontSize = 36f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.red;
    
    private PlayerManager playerManager;
    
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
    }
    
    void Update()
    {
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
        
        // Assign references
        timerText = timerTextComponent;
        phaseText = phaseTextComponent;
        
        Debug.Log("Timer UI Canvas created successfully!");
    }
}