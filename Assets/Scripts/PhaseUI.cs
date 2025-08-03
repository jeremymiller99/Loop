using UnityEngine;
using UnityEngine.UI;

public class PhaseUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text phaseText;
    
    [Header("UI Settings")]
    [SerializeField] private float fontSize = 25f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color player1Color = new Color(0.3f, 0.8f, 1f); // Light blue
    [SerializeField] private Color player2Color = new Color(1f, 0.6f, 0.3f); // Light orange
    
    [Header("Phase Messages")]
    [SerializeField] private string player1Message = "Player 1: Reach the Goal!";
    [SerializeField] private string player2Message = "Player 2: Eliminate Player 1!";
    [SerializeField] private string levelCompleteMessage = "Level Complete!";
    
    // Component references
    private PlayerManager playerManager;
    private GameStateManager gameStateManager;
    
    // Singleton pattern
    public static PhaseUI Instance { get; private set; }
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple PhaseUI instances found! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Find component references
        playerManager = FindFirstObjectByType<PlayerManager>();
        gameStateManager = GameStateManager.Instance;
        
        if (playerManager == null)
        {
            Debug.LogWarning("PhaseUI: No PlayerManager found in scene!");
        }
        if (gameStateManager == null)
        {
            Debug.LogWarning("PhaseUI: No GameStateManager found in scene!");
        }
        
        // Set up UI text if not assigned
        if (phaseText == null)
        {
            phaseText = GetComponent<Text>();
        }
        
        // Configure text appearance
        ConfigureUIElements();
        
        // Subscribe to events
        SubscribeToEvents();
        
        // Initialize display
        UpdatePhaseText();
    }
    
    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    private void ConfigureUIElements()
    {
        if (phaseText != null)
        {
            phaseText.fontSize = (int)fontSize;
            phaseText.color = normalColor;
            phaseText.alignment = TextAnchor.MiddleCenter;
        }
    }
    
    private void SubscribeToEvents()
    {
        // Subscribe to phase change events
        GameEvents.OnPhaseStarted += OnPhaseStarted;
        GameEvents.OnLevelCompleted += OnLevelCompleted;
        
        if (gameStateManager != null)
        {
            gameStateManager.OnLevelCompletionChanged += OnLevelCompletionChanged;
        }
    }
    
    private void UnsubscribeFromEvents()
    {
        GameEvents.OnPhaseStarted -= OnPhaseStarted;
        GameEvents.OnLevelCompleted -= OnLevelCompleted;
        
        if (gameStateManager != null)
        {
            gameStateManager.OnLevelCompletionChanged -= OnLevelCompletionChanged;
        }
    }
    
    private void OnPhaseStarted(GamePhase phase)
    {
        UpdatePhaseText();
    }
    
    private void OnLevelCompleted()
    {
        UpdatePhaseText();
    }
    
    private void OnLevelCompletionChanged(bool isCompleted)
    {
        UpdatePhaseText();
    }
    
    private void UpdatePhaseText()
    {
        if (phaseText == null) return;
        
        // Check if level is completed
        if (gameStateManager != null && gameStateManager.IsLevelCompleted())
        {
            phaseText.text = levelCompleteMessage;
            phaseText.color = Color.green;
            return;
        }
        
        // Determine current phase based on PlayerManager state
        if (playerManager != null)
        {
            if (playerManager.CurrentPhase == GamePhase.Player1Phase)
            {
                phaseText.text = player1Message;
                phaseText.color = player1Color;
            }
            else if (playerManager.CurrentPhase == GamePhase.Player2Phase)
            {
                phaseText.text = player2Message;
                phaseText.color = player2Color;
            }
            else
            {
                phaseText.text = "Waiting...";
                phaseText.color = normalColor;
            }
        }
        else
        {
            phaseText.text = "Phase Unknown";
            phaseText.color = normalColor;
        }
    }
    
    // Public method to manually refresh the display
    public void RefreshDisplay()
    {
        UpdatePhaseText();
    }
    
    // Public method to set custom phase message
    public void SetCustomMessage(string message, Color? color = null)
    {
        if (phaseText != null)
        {
            phaseText.text = message;
            if (color.HasValue)
            {
                phaseText.color = color.Value;
            }
        }
    }
}