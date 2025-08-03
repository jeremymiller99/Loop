using UnityEngine;
using UnityEngine.UI;

public class LoopCounterUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Text loopCounterText;
    
    [Header("UI Settings")]
    [SerializeField] private float fontSize = 29f;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color completedColor = Color.green;
    [SerializeField] private Color highlightColor = Color.yellow;
    
    [Header("Display Format")]
    [SerializeField] private string loopFormat = "{0}/{1} Loops";
    [SerializeField] private string singleLoopFormat = "{0}/{1} Loop";
    
    // Component references
    private GameStateManager gameStateManager;
    
    // Singleton pattern
    public static LoopCounterUI Instance { get; private set; }
    
    // Animation variables
    private float animationTimer = 0f;
    private bool isAnimating = false;
    private readonly float animationDuration = 1f;
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple LoopCounterUI instances found! Destroying duplicate.");
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
            Debug.LogWarning("LoopCounterUI: No GameStateManager found in scene!");
        }
        
        // Set up UI text if not assigned
        if (loopCounterText == null)
        {
            loopCounterText = GetComponent<Text>();
        }
        
        // Configure text appearance
        ConfigureUIElements();
        
        // Subscribe to events
        SubscribeToEvents();
        
        // Initialize display
        UpdateLoopCounter();
    }
    
    void Update()
    {
        // Handle loop completion animation
        if (isAnimating)
        {
            UpdateAnimation();
        }
    }
    
    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }
    
    private void ConfigureUIElements()
    {
        if (loopCounterText != null)
        {
            loopCounterText.fontSize = (int)fontSize;
            loopCounterText.color = normalColor;
            loopCounterText.alignment = TextAnchor.MiddleCenter;
        }
    }
    
    private void SubscribeToEvents()
    {
        GameEvents.OnLoopCompleted += OnLoopCompleted;
        
        if (gameStateManager != null)
        {
            gameStateManager.OnLoopProgressUpdated += OnLoopProgressUpdated;
        }
    }
    
    private void UnsubscribeFromEvents()
    {
        GameEvents.OnLoopCompleted -= OnLoopCompleted;
        
        if (gameStateManager != null)
        {
            gameStateManager.OnLoopProgressUpdated -= OnLoopProgressUpdated;
        }
    }
    
    private void OnLoopCompleted(int completedLoops, int totalRequired)
    {
        // Start completion animation
        StartLoopCompletionAnimation();
        UpdateLoopCounter();
    }
    
    private void OnLoopProgressUpdated(int completedLoops, int requiredLoops)
    {
        UpdateLoopCounter();
    }
    
    private void UpdateLoopCounter()
    {
        if (loopCounterText == null || gameStateManager == null) return;
        
        int completedLoops = gameStateManager.GetCompletedLoops();
        int requiredLoops = gameStateManager.GetRequiredLoops();
        
        // Choose appropriate format based on required loops
        string format = requiredLoops == 1 ? singleLoopFormat : loopFormat;
        loopCounterText.text = string.Format(format, completedLoops, requiredLoops);
        
        // Update color based on progress
        if (!isAnimating)
        {
            if (completedLoops >= requiredLoops)
            {
                loopCounterText.color = completedColor;
            }
            else
            {
                loopCounterText.color = normalColor;
            }
        }
    }
    
    private void StartLoopCompletionAnimation()
    {
        isAnimating = true;
        animationTimer = 0f;
    }
    
    private void UpdateAnimation()
    {
        animationTimer += Time.deltaTime;
        
        if (animationTimer < animationDuration)
        {
            // Pulse between highlight and normal color
            float t = Mathf.PingPong(animationTimer * 4f, 1f);
            loopCounterText.color = Color.Lerp(highlightColor, completedColor, t);
            
            // Scale pulse effect
            float scale = 1f + 0.1f * Mathf.Sin(animationTimer * 8f);
            loopCounterText.transform.localScale = Vector3.one * scale;
        }
        else
        {
            // End animation
            isAnimating = false;
            loopCounterText.transform.localScale = Vector3.one;
            UpdateLoopCounter(); // Restore normal color
        }
    }
    
    // Public method to manually refresh the display
    public void RefreshDisplay()
    {
        UpdateLoopCounter();
    }
    
    // Public method to trigger animation manually
    public void TriggerCompletionAnimation()
    {
        StartLoopCompletionAnimation();
    }
}