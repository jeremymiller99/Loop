using UnityEngine;

public class TimerManager : MonoBehaviour
{
    [Header("Timer Settings")]
    [SerializeField] private float phaseTimerDuration = 30f;
    [SerializeField] private bool enableTimer = true;
    [SerializeField] private bool debugMode = false;
    
    // Timer state
    private float currentTimer;
    private bool timerActive;
    private GamePhase currentPhase;
    
    // Singleton pattern for easy access
    public static TimerManager Instance { get; private set; }
    
    // Events for timer updates
    public System.Action<float, float> OnTimerUpdated; // currentTime, maxTime
    public System.Action<float> OnTimerWarning; // time remaining when warning should show
    public System.Action OnTimerExpired;
    
    void Awake()
    {
        // Singleton setup
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple TimerManager instances found! Destroying duplicate.");
            Destroy(gameObject);
            return;
        }
        
        // Subscribe to game events
        SubscribeToEvents();
    }
    
    void Update()
    {
        // Update timer if active
        if (timerActive && enableTimer)
        {
            currentTimer -= Time.deltaTime;
            
            // Notify subscribers of timer update
            OnTimerUpdated?.Invoke(currentTimer, phaseTimerDuration);
            
            // Check for warning threshold (last 10 seconds)
            if (currentTimer <= 10f && currentTimer > 9.9f)
            {
                OnTimerWarning?.Invoke(currentTimer);
            }
            
            // Check if time ran out
            if (currentTimer <= 0f)
            {
                timerActive = false;
                OnTimerExpired?.Invoke();
                GameEvents.TriggerTimerExpired();
                
                if (debugMode)
                {
                    Debug.Log($"TimerManager: Timer expired for {currentPhase}");
                }
            }
        }
    }
    
    private void SubscribeToEvents()
    {
        GameEvents.OnPhaseStarted += OnPhaseStarted;
    }
    
    private void UnsubscribeFromEvents()
    {
        GameEvents.OnPhaseStarted -= OnPhaseStarted;
    }
    
    private void OnPhaseStarted(GamePhase phase)
    {
        currentPhase = phase;
        StartTimer();
        
        if (debugMode)
        {
            Debug.Log($"TimerManager: Started timer for {phase}");
        }
    }
    
    public void StartTimer()
    {
        if (!enableTimer) return;
        
        currentTimer = phaseTimerDuration;
        timerActive = true;
        
        // Notify subscribers that timer started
        OnTimerUpdated?.Invoke(currentTimer, phaseTimerDuration);
        
        if (debugMode)
        {
            Debug.Log($"TimerManager: Timer started for {currentPhase}: {phaseTimerDuration} seconds");
        }
    }
    
    public void StopTimer()
    {
        timerActive = false;
        
        if (debugMode)
        {
            Debug.Log($"TimerManager: Timer stopped for {currentPhase}");
        }
    }
    
    public void PauseTimer()
    {
        timerActive = false;
        
        if (debugMode)
        {
            Debug.Log($"TimerManager: Timer paused for {currentPhase}");
        }
    }
    
    public void ResumeTimer()
    {
        if (!enableTimer) return;
        
        timerActive = true;
        
        if (debugMode)
        {
            Debug.Log($"TimerManager: Timer resumed for {currentPhase}");
        }
    }
    
    public void AddTime(float seconds)
    {
        currentTimer += seconds;
        OnTimerUpdated?.Invoke(currentTimer, phaseTimerDuration);
        
        if (debugMode)
        {
            Debug.Log($"TimerManager: Added {seconds} seconds to timer");
        }
    }
    
    public void SetTimerDuration(float duration)
    {
        phaseTimerDuration = duration;
        
        if (debugMode)
        {
            Debug.Log($"TimerManager: Timer duration set to {duration} seconds");
        }
    }
    
    // Public getters
    public float GetCurrentTime() => currentTimer;
    public float GetMaxTime() => phaseTimerDuration;
    public bool IsTimerActive() => timerActive;
    public bool IsTimerEnabled() => enableTimer;
    public float GetTimeRemaining() => currentTimer;
    public float GetTimeElapsed() => phaseTimerDuration - currentTimer;
    public float GetTimerProgress() => phaseTimerDuration > 0 ? (phaseTimerDuration - currentTimer) / phaseTimerDuration : 0f;
    
    // Method to get formatted time string
    public string GetFormattedTime()
    {
        if (currentTimer <= 0) return "00:00";
        
        int minutes = Mathf.FloorToInt(currentTimer / 60f);
        int seconds = Mathf.FloorToInt(currentTimer % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
    
    // Method to get formatted time with prefix
    public string GetFormattedTimeWithPrefix(string prefix = "Time: ")
    {
        return prefix + GetFormattedTime();
    }
    
    // Method to check if timer is in warning state
    public bool IsInWarningState(float warningThreshold = 10f)
    {
        return timerActive && currentTimer <= warningThreshold;
    }
    
    // Method to enable/disable timer
    public void SetTimerEnabled(bool enabled)
    {
        enableTimer = enabled;
        
        if (!enabled && timerActive)
        {
            StopTimer();
        }
        
        if (debugMode)
        {
            Debug.Log($"TimerManager: Timer enabled = {enabled}");
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