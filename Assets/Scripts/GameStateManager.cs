using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private int requiredLoopsToComplete = 1;
    [SerializeField] private bool debugMode = false;
    
    // Game state tracking
    private float totalPlayTime = 0f;
    private int completedLoops = 0;
    private bool levelCompleted = false;
    private bool isFirstPhase1 = true;
    private int attemptCount = 1; // Start at attempt 1
    
    // Singleton pattern for easy access
    public static GameStateManager Instance { get; private set; }
    
    // Events for game state changes
    public System.Action<float> OnTotalPlayTimeUpdated;
    public System.Action<int, int> OnLoopProgressUpdated;
    public System.Action<bool> OnLevelCompletionChanged;
    public System.Action<int> OnAttemptCountUpdated;
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple GameStateManager instances found! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        
        // Subscribe to game events
        SubscribeToEvents();
    }
    
    void Update()
    {
        // Track total play time only when level is not completed
        if (!levelCompleted)
        {
            totalPlayTime += Time.deltaTime;
            OnTotalPlayTimeUpdated?.Invoke(totalPlayTime);
        }
    }
    
    private void SubscribeToEvents()
    {
        GameEvents.OnLoopCompleted += OnLoopCompleted;
        GameEvents.OnLevelCompleted += OnLevelCompleted;
        GameEvents.OnPlayer1Died += OnPlayerDied;
        GameEvents.OnPlayer1Shot += OnPlayerDied; // Same handler for both spike and shot deaths
        GameEvents.OnPlayer2Died += OnPlayerDied;
        GameEvents.OnPlayer1GhostReachedGoal += OnPlayerDied; // Player 2 failed to eliminate Player 1's replay
        GameEvents.OnTimerExpired += OnPlayerDied;
    }
    
    private void UnsubscribeFromEvents()
    {
        GameEvents.OnLoopCompleted -= OnLoopCompleted;
        GameEvents.OnLevelCompleted -= OnLevelCompleted;
        GameEvents.OnPlayer1Died -= OnPlayerDied;
        GameEvents.OnPlayer1Shot -= OnPlayerDied;
        GameEvents.OnPlayer2Died -= OnPlayerDied;
        GameEvents.OnPlayer1GhostReachedGoal -= OnPlayerDied; // Player 2 failed to eliminate Player 1's replay
        GameEvents.OnTimerExpired -= OnPlayerDied;
    }
    
    private void OnLoopCompleted(int loopsCompleted, int totalRequired)
    {
        completedLoops = loopsCompleted;
        isFirstPhase1 = false; // No longer first phase after completing a loop
        
        OnLoopProgressUpdated?.Invoke(completedLoops, requiredLoopsToComplete);
        
        if (debugMode)
        {
            Debug.Log($"GameStateManager: Loop {completedLoops}/{requiredLoopsToComplete} completed");
        }
    }
    
    private void OnLevelCompleted()
    {
        levelCompleted = true;
        OnLevelCompletionChanged?.Invoke(levelCompleted);
        
        if (debugMode)
        {
            Debug.Log($"GameStateManager: Level completed! Total time: {totalPlayTime:F1}s, Attempts: {attemptCount}");
        }
    }
    
    private void OnPlayerDied()
    {
        attemptCount++;
        OnAttemptCountUpdated?.Invoke(attemptCount);
        
        if (debugMode)
        {
            Debug.Log($"GameStateManager: Player died/failed or Player 2 failed to eliminate Player 1's replay - Attempt #{attemptCount}");
        }
    }
    
    // Public methods to get game state
    public float GetTotalPlayTime() => totalPlayTime;
    public int GetCompletedLoops() => completedLoops;
    public int GetRequiredLoops() => requiredLoopsToComplete;
    public bool IsLevelCompleted() => levelCompleted;
    public bool IsFirstPhase1() => isFirstPhase1;
    public float GetLoopProgress() => requiredLoopsToComplete == 0 ? 0f : (float)completedLoops / requiredLoopsToComplete;
    public int GetAttemptCount() => attemptCount;
    
    // Method to get level completion statistics
    public LevelStats GetLevelStats()
    {
        return new LevelStats
        {
            totalTime = totalPlayTime,
            completedLoops = completedLoops,
            requiredLoops = requiredLoopsToComplete,
            averageTimePerLoop = completedLoops > 0 ? totalPlayTime / completedLoops : 0f,
            isCompleted = levelCompleted,
            attemptCount = attemptCount
        };
    }
    
    // Method to reset game state (for level restart)
    public void ResetGameState()
    {
        totalPlayTime = 0f;
        completedLoops = 0;
        levelCompleted = false;
        isFirstPhase1 = true;
        attemptCount = 1; // Reset to attempt 1
        
        // Notify subscribers of the reset
        OnTotalPlayTimeUpdated?.Invoke(totalPlayTime);
        OnLoopProgressUpdated?.Invoke(completedLoops, requiredLoopsToComplete);
        OnLevelCompletionChanged?.Invoke(levelCompleted);
        OnAttemptCountUpdated?.Invoke(attemptCount);
        
        if (debugMode)
        {
            Debug.Log("GameStateManager: Game state reset");
        }
    }
    
    // Method to set required loops (useful for dynamic level configuration)
    public void SetRequiredLoops(int loops)
    {
        requiredLoopsToComplete = loops;
        OnLoopProgressUpdated?.Invoke(completedLoops, requiredLoopsToComplete);
        
        if (debugMode)
        {
            Debug.Log($"GameStateManager: Required loops set to {loops}");
        }
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
}

// Data structure for level statistics
[System.Serializable]
public struct LevelStats
{
    public float totalTime;
    public int completedLoops;
    public int requiredLoops;
    public float averageTimePerLoop;
    public bool isCompleted;
    public int attemptCount;
}