using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LevelCompleteUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject levelCompletePanel;
    [SerializeField] private Text levelCompleteTitleText;
    [SerializeField] private Text levelCompleteRankText;
    [SerializeField] private Text levelCompleteMessageText;
    [SerializeField] private Text levelCompleteDetailsText;
    [SerializeField] private Button levelCompleteCloseButton;
    
    [Header("UI Settings")]
    [SerializeField] private float fontSize = 36f;
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private bool enablePanelAnimation = true;
    
    [Header("Audio")]
    [SerializeField] private AudioClip showPanelSound;
    [SerializeField] private AudioClip hidePanelSound;
    
    // Component references
    private LevelCompleteManager levelCompleteManager;
    private AudioSource audioSource;
    
    // Singleton pattern
    public static LevelCompleteUI Instance { get; private set; }
    
    // Animation state
    private bool isAnimating = false;
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple LevelCompleteUI instances found! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        // Find component references
        levelCompleteManager = LevelCompleteManager.Instance;
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        if (levelCompleteManager == null)
        {
            Debug.LogWarning("LevelCompleteUI: No LevelCompleteManager found in scene!");
        }
        
        // Configure UI elements
        ConfigureUIElements();
        
        // Subscribe to events
        SubscribeToEvents();
        
        // Set up button click event
        if (levelCompleteCloseButton != null)
        {
            levelCompleteCloseButton.onClick.AddListener(OnCloseButtonClicked);
        }
        
        // Hide panel initially
        if (levelCompletePanel != null)
        {
            levelCompletePanel.SetActive(false);
        }
    }
    
    void OnDestroy()
    {
        UnsubscribeFromEvents();
        
        if (levelCompleteCloseButton != null)
        {
            levelCompleteCloseButton.onClick.RemoveListener(OnCloseButtonClicked);
        }
    }
    
    private void ConfigureUIElements()
    {
        if (levelCompleteTitleText != null)
        {
            levelCompleteTitleText.fontSize = (int)(fontSize * 1.2f);
            levelCompleteTitleText.color = Color.white;
            levelCompleteTitleText.alignment = TextAnchor.MiddleCenter;
        }
        
        if (levelCompleteRankText != null)
        {
            levelCompleteRankText.fontSize = (int)(fontSize * 2f);
            levelCompleteRankText.alignment = TextAnchor.MiddleCenter;
        }
        
        if (levelCompleteMessageText != null)
        {
            levelCompleteMessageText.fontSize = (int)(fontSize * 0.8f);
            levelCompleteMessageText.alignment = TextAnchor.MiddleCenter;
        }
        
        if (levelCompleteDetailsText != null)
        {
            levelCompleteDetailsText.fontSize = (int)(fontSize * 0.7f);
            levelCompleteDetailsText.color = Color.white;
            levelCompleteDetailsText.alignment = TextAnchor.MiddleCenter;
        }
    }
    
    private void SubscribeToEvents()
    {
        // Disabled: UI no longer shows - game goes directly to next level
        // GameEvents.OnLevelCompleted += OnLevelCompleted;
    }
    
    private void UnsubscribeFromEvents()
    {
        // Disabled: UI no longer shows - game goes directly to next level
        // GameEvents.OnLevelCompleted -= OnLevelCompleted;
    }
    
    private void OnLevelCompleted()
    {
        ShowLevelCompletePanel();
    }
    
    private void OnCloseButtonClicked()
    {
        if (levelCompleteManager != null)
        {
            levelCompleteManager.ContinuePlaying();
        }
        else
        {
            HideLevelCompletePanel();
        }
    }
    
    public void ShowLevelCompletePanel()
    {
        if (levelCompletePanel == null || isAnimating) return;
        
        // Update panel content with current stats
        UpdatePanelContent();
        
        // Play show sound
        if (showPanelSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(showPanelSound);
        }
        
        // Show panel
        levelCompletePanel.SetActive(true);
        
        // Start animation if enabled
        if (enablePanelAnimation)
        {
            StartCoroutine(AnimateShowPanel());
        }
    }
    
    public void HideLevelCompletePanel()
    {
        if (levelCompletePanel == null || isAnimating) return;
        
        // Play hide sound
        if (hidePanelSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hidePanelSound);
        }
        
        // Start animation if enabled
        if (enablePanelAnimation)
        {
            StartCoroutine(AnimateHidePanel());
        }
        else
        {
            levelCompletePanel.SetActive(false);
        }
    }
    
    private void UpdatePanelContent()
    {
        if (levelCompleteManager == null) return;
        
        // Get enhanced level stats
        EnhancedLevelStats stats = levelCompleteManager.GetEnhancedLevelStats();
        
        // Update title
        if (levelCompleteTitleText != null)
        {
            levelCompleteTitleText.text = "LEVEL COMPLETE!";
        }
        
        // Update rank display
        if (levelCompleteRankText != null)
        {
            levelCompleteRankText.text = stats.performance.rankText;
            levelCompleteRankText.color = stats.performance.rankColor;
        }
        
        // Update message
        if (levelCompleteMessageText != null)
        {
            levelCompleteMessageText.text = stats.performance.message;
            levelCompleteMessageText.color = stats.performance.rankColor;
        }
        
        // Update time details
        if (levelCompleteDetailsText != null)
        {
            levelCompleteDetailsText.text = $"Time: {stats.formattedTime}\\nAttempts: {stats.attemptCount}";
        }
    }
    
    private IEnumerator AnimateShowPanel()
    {
        isAnimating = true;
        
        // Start with panel scaled down
        levelCompletePanel.transform.localScale = Vector3.zero;
        
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / animationDuration;
            
            // Ease out animation
            t = 1f - (1f - t) * (1f - t);
            
            levelCompletePanel.transform.localScale = Vector3.one * t;
            yield return null;
        }
        
        levelCompletePanel.transform.localScale = Vector3.one;
        isAnimating = false;
    }
    
    private IEnumerator AnimateHidePanel()
    {
        isAnimating = true;
        
        float elapsed = 0f;
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = 1f - (elapsed / animationDuration);
            
            // Ease in animation
            t = t * t;
            
            levelCompletePanel.transform.localScale = Vector3.one * t;
            yield return null;
        }
        
        levelCompletePanel.SetActive(false);
        levelCompletePanel.transform.localScale = Vector3.one;
        isAnimating = false;
    }
    
    // Public method to set custom content
    public void SetCustomContent(string title, string rank, Color rankColor, string message, string details)
    {
        if (levelCompleteTitleText != null)
            levelCompleteTitleText.text = title;
        
        if (levelCompleteRankText != null)
        {
            levelCompleteRankText.text = rank;
            levelCompleteRankText.color = rankColor;
        }
        
        if (levelCompleteMessageText != null)
        {
            levelCompleteMessageText.text = message;
            levelCompleteMessageText.color = rankColor;
        }
        
        if (levelCompleteDetailsText != null)
            levelCompleteDetailsText.text = details;
    }
    
    // Public method to check if panel is currently visible
    public bool IsVisible => levelCompletePanel != null && levelCompletePanel.activeInHierarchy;
}