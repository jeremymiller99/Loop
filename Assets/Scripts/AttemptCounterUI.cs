using UnityEngine;
using UnityEngine.UI;

public class AttemptCounterUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text attemptCounterText;
    
    [Header("UI Settings")]
    [SerializeField] private float fontSize = 22f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private float transparency = 0.8f;
    
    // Component references
    private GameStateManager gameStateManager;
    
    // Singleton pattern
    public static AttemptCounterUI Instance { get; private set; }
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple AttemptCounterUI instances found! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Find game state manager
        gameStateManager = GameStateManager.Instance;
        
        if (gameStateManager == null)
        {
            Debug.LogWarning("AttemptCounterUI: No GameStateManager found in scene!");
        }
        
        // Set up UI text if not assigned
        if (attemptCounterText == null)
        {
            attemptCounterText = GetComponent<Text>();
        }
        
        // Configure text appearance
        ConfigureUIElements();
        
        // Subscribe to events
        SubscribeToEvents();
        
        // Initialize display
        UpdateAttemptCounter();
    }
    
    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    private void ConfigureUIElements()
    {
        if (attemptCounterText != null)
        {
            attemptCounterText.fontSize = (int)fontSize;
            attemptCounterText.color = new Color(normalColor.r, normalColor.g, normalColor.b, transparency);
            attemptCounterText.alignment = TextAnchor.UpperRight;
        }
    }
    
    private void SubscribeToEvents()
    {
        if (gameStateManager != null)
        {
            gameStateManager.OnAttemptCountUpdated += OnAttemptCountUpdated;
        }
    }
    
    private void UnsubscribeFromEvents()
    {
        if (gameStateManager != null)
        {
            gameStateManager.OnAttemptCountUpdated -= OnAttemptCountUpdated;
        }
    }
    
    private void OnAttemptCountUpdated(int attemptCount)
    {
        UpdateAttemptCounter();
    }
    
    private void UpdateAttemptCounter()
    {
        if (attemptCounterText == null || gameStateManager == null) return;
        
        LevelStats stats = gameStateManager.GetLevelStats();
        attemptCounterText.text = stats.attemptCount.ToString();
    }
    
    // Public method to manually refresh the display
    public void RefreshDisplay()
    {
        UpdateAttemptCounter();
    }
}