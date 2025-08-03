using UnityEngine;
using UnityEngine.UI;

public class TimerUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text timerText;
    
    [Header("UI Settings")]
    [SerializeField] private float fontSize = 36f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color warningColor = Color.red;
    [SerializeField] private float warningThreshold = 10f; // Time remaining when timer turns red
    
    // Component references
    private TimerManager timerManager;
    
    // Singleton pattern
    public static TimerUI Instance { get; private set; }
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple TimerUI instances found! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Find timer manager
        timerManager = TimerManager.Instance;
        
        if (timerManager == null)
        {
            Debug.LogWarning("TimerUI: No TimerManager found in scene!");
        }
        
        // Set up UI text if not assigned
        if (timerText == null)
        {
            timerText = GetComponent<Text>();
        }
        
        // Configure text appearance
        ConfigureUIElements();
        
        // Subscribe to events
        SubscribeToEvents();
    }
    
    void Update()
    {
        // Update timer display
        UpdateTimerDisplay();
    }
    
    void OnDestroy()
    {
        UnsubscribeFromEvents();
        
        // Clear singleton reference
        if (Instance == this)
        {
            Instance = null;
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
    }
    
    private void SubscribeToEvents()
    {
        // Subscribe to timer manager events
        if (timerManager != null)
        {
            timerManager.OnTimerUpdated += OnTimerUpdated;
            timerManager.OnTimerWarning += OnTimerWarning;
        }
    }
    
    private void UnsubscribeFromEvents()
    {
        // Unsubscribe from timer manager events
        if (timerManager != null)
        {
            timerManager.OnTimerUpdated -= OnTimerUpdated;
            timerManager.OnTimerWarning -= OnTimerWarning;
        }
    }
    
    private void UpdateTimerDisplay()
    {
        if (timerText == null || timerManager == null) return;
        
        // Get formatted time from timer manager
        timerText.text = timerManager.GetFormattedTimeWithPrefix("Time: ");
        
        // Update color based on warning state or remaining time
        float remainingTime = timerManager.GetTimeRemaining();
        if (timerManager.IsInWarningState() || remainingTime <= warningThreshold)
        {
            timerText.color = warningColor;
        }
        else
        {
            timerText.color = normalColor;
        }
    }
    
    // Event handlers for timer manager events
    private void OnTimerUpdated(float currentTime, float maxTime)
    {
        // Timer display is handled in UpdateTimerDisplay()
        // This could be used for additional timer-based animations
    }
    
    private void OnTimerWarning(float timeRemaining)
    {
        // Update color immediately when warning is triggered
        if (timerText != null)
        {
            timerText.color = warningColor;
        }
    }
    
    // Public method to manually refresh the display
    public void RefreshDisplay()
    {
        UpdateTimerDisplay();
    }
    
    // Public method to set custom timer text (useful for game over scenarios)
    public void SetCustomTimerText(string text, Color? color = null)
    {
        if (timerText != null)
        {
            timerText.text = text;
            if (color.HasValue)
            {
                timerText.color = color.Value;
            }
        }
    }
}