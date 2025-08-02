using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GamePhase
{
    Player1Phase,    // Player 1 tries to reach goal
    Player2Phase     // Player 2 tries to shoot Player 1
}

public class PlayerManager : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private Player1Controller player1;
    [SerializeField] private Player2Controller player2;
    
    [Header("Goal Reference")]
    [SerializeField] private GoalTrigger goalTrigger;
    
    [Header("Movement Recording Components")]
    [SerializeField] private MovementRecorder player1Recorder;
    [SerializeField] private MovementRecorder player2Recorder;
    [SerializeField] private MovementReplayer player1Replayer;
    [SerializeField] private MovementReplayer player2Replayer;
    
    [Header("Timer Settings")]
    [SerializeField] private float phaseTimerDuration = 30f; // 30 seconds per phase
    [SerializeField] private Text timerText; // UI Text to display countdown
    [SerializeField] private bool enableTimer = true; // Toggle to enable/disable timer
    
    [Header("Level Settings")]
    [SerializeField] private int requiredLoopsToComplete = 1; // How many loops needed to complete this level
    [SerializeField] private bool debugMode = false; // Show debug information
    
    private GamePhase currentPhase = GamePhase.Player1Phase;
    private Vector3 player1StartPosition;
    private Vector3 player2StartPosition;
    
    // Timer variables
    private float currentTimer;
    private bool timerActive;
    
    // Movement recording variables
    private MovementRecording player1LastRecording;
    private MovementRecording player2LastRecording;
    
    // Loop tracking variables
    private int completedLoops = 0; // How many complete loops (Phase 1 + Phase 2) have been finished
    private bool isFirstPhase1 = true; // Track if this is the very first Phase 1 (no recordings exist yet)
    private bool levelCompleted = false;
    
    void Start()
    {
        // Store starting positions for reset functionality
        if (player1 != null)
            player1StartPosition = player1.transform.position;
        if (player2 != null)
            player2StartPosition = player2.transform.position;
        
        // Set up movement recording/replay components if not assigned
        SetupRecordingComponents();
        
        // Subscribe to game events
        SubscribeToEvents();
        
        // Start with Player 1 phase
        StartPlayer1Phase();
    }
    
    void Update()
    {
        // Handle debug input to restart level (R key)
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }
        
        // Update timer if active
        if (timerActive && enableTimer)
        {
            currentTimer -= Time.deltaTime;
            UpdateTimerUI();
            
            // Check if time ran out
            if (currentTimer <= 0f)
            {
                timerActive = false;
                GameEvents.TriggerTimerExpired();
            }
        }
    }
    
    private void StartPlayer1Phase()
    {
        currentPhase = GamePhase.Player1Phase;
        
        // Reset player positions
        ResetPlayerPositions();
        
        // Player 1 is active and controllable
        if (player1 != null)
            player1.SetActive(true);
        
        // Player 2 setup - either inactive (first Phase 1) or replaying previous recording
        if (player2 != null)
        {
            player2.SetActive(false);
            
            // If we have a recording from a previous loop's Player 2 phase, replay it
            if (!isFirstPhase1 && player2LastRecording != null && player2Replayer != null)
            {
                Debug.Log("Starting Player 2 replay from previous loop");
                player2Replayer.StartReplay(player2LastRecording);
            }
            else if (player2Replayer != null)
            {
                player2Replayer.StopReplay();
            }
        }
        
        // Start recording Player 1's movements
        if (player1Recorder != null)
        {
            player1Recorder.StartRecording();
        }
        
        // Start the timer for this phase
        StartTimer();
        
        // Debug information
        string phaseMessage = isFirstPhase1 ? 
            $"Phase 1: Control Player 1 to reach the goal! (Loop 1/{requiredLoopsToComplete} - First attempt)" :
            $"Phase 1: Control Player 1 to reach the goal! (Loop {completedLoops + 1}/{requiredLoopsToComplete} - Player 2 ghost active)";
        
        if (debugMode)
        {
            Debug.Log(phaseMessage);
        }
        
        // Update UI with current loop info
        UpdateLoopUI();
        
        // Trigger phase started event
        GameEvents.TriggerPhaseStarted(GamePhase.Player1Phase);
    }
    
    private void StartPlayer2Phase()
    {
        currentPhase = GamePhase.Player2Phase;
        
        // Stop recording Player 1's movements and save the recording
        if (player1Recorder != null)
        {
            player1Recorder.StopRecording();
            player1LastRecording = player1Recorder.GetCompletedRecording();
        }
        
        // Mark that we're no longer in the first Phase 1
        isFirstPhase1 = false;
        
        // Reset player positions
        ResetPlayerPositions();
        
        // Player 2 is active and controllable
        if (player2 != null)
            player2.SetActive(true);
        
        // Player 1 setup - replay the recording we just made
        if (player1 != null)
        {
            player1.SetActive(false);
            
            // Start replaying Player 1's movements from the phase we just completed
            if (player1LastRecording != null && player1Replayer != null)
            {
                Debug.Log("Starting Player 1 replay from Phase 1");
                player1Replayer.StartReplay(player1LastRecording);
            }
        }
        
        // Start recording Player 2's movements
        if (player2Recorder != null)
        {
            player2Recorder.StartRecording();
        }
        
        // Start the timer for this phase
        StartTimer();
        
        if (debugMode)
        {
            Debug.Log($"Phase 2: Control Player 2 to shoot Player 1! (Loop {completedLoops + 1}/{requiredLoopsToComplete} - Player 1 ghost active)");
        }
        
        // Update UI with current loop info
        UpdateLoopUI();
        
        // Trigger phase started event
        GameEvents.TriggerPhaseStarted(GamePhase.Player2Phase);
    }
    
    private void ResetPlayerPositions()
    {
        if (player1 != null)
        {
            player1.transform.position = player1StartPosition;
            // Reset any physics state
            Rigidbody2D rb1 = player1.GetComponent<Rigidbody2D>();
            if (rb1 != null)
            {
                rb1.linearVelocity = Vector2.zero;
                rb1.angularVelocity = 0f;
            }
        }
        
        if (player2 != null)
        {
            player2.transform.position = player2StartPosition;
            // Reset any physics state
            Rigidbody2D rb2 = player2.GetComponent<Rigidbody2D>();
            if (rb2 != null)
            {
                rb2.linearVelocity = Vector2.zero;
                rb2.angularVelocity = 0f;
            }
        }
    }
    
    private void OnPlayer1ReachedGoal()
    {
        Debug.Log("Player 1 reached the goal! Switching to Player 2 phase...");
        StopTimer(); // Stop the current phase timer
        
        // Immediate visual feedback
        if (timerText != null)
        {
            timerText.text = "GOAL REACHED!";
            timerText.color = Color.blue;
        }
        
        // Proceed to Phase 2 of the current loop
        StartPlayer2Phase();
    }
    
    private void OnPlayer2Victory()
    {
        Debug.Log("Player 2 shot Player 1! Loop completed.");
        StopTimer(); // Stop the timer when phase ends
        
        // Stop recording Player 2's movements and save the recording
        if (player2Recorder != null)
        {
            player2Recorder.StopRecording();
            player2LastRecording = player2Recorder.GetCompletedRecording();
        }
        
        // Stop Player 1's replay immediately
        if (player1Replayer != null)
        {
            player1Replayer.StopReplay();
        }
        
        // Increment completed loops count
        completedLoops++;
        
        // Trigger loop completed event
        GameEvents.TriggerLoopCompleted(completedLoops, requiredLoopsToComplete);
        
        // Check if we've completed all required loops
        if (completedLoops >= requiredLoopsToComplete)
        {
            // Level completed!
            HandleLevelComplete();
        }
        else
        {
            // Need more loops - start next loop (Phase 1)
            if (timerText != null)
            {
                timerText.text = $"LOOP {completedLoops}/{requiredLoopsToComplete} COMPLETE!";
                timerText.color = Color.green;
            }
            
            if (debugMode)
            {
                Debug.Log($"Loop {completedLoops}/{requiredLoopsToComplete} completed. Starting next loop...");
            }
            
            // Start next loop after short delay
            Invoke(nameof(StartPlayer1Phase), 0.5f);
        }
    }
    
    public void RestartCurrentPhase()
    {
        Debug.Log($"Restarting current phase: {currentPhase}");
        
        // Reset player positions without changing phases
        ResetPlayerPositions();
        
        // Reset goal trigger if needed
        if (goalTrigger != null)
        {
            goalTrigger.ResetGoal();
        }
        
        // Restart the timer for this phase
        StartTimer();
        
        Debug.Log($"Phase {currentPhase} has been reset!");
    }
    
    private void RestartLevel()
    {
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    // Public method to get the currently active player (useful for other scripts)
    public GameObject GetActivePlayer()
    {
        switch (currentPhase)
        {
            case GamePhase.Player1Phase:
                return player1 != null ? player1.gameObject : null;
            case GamePhase.Player2Phase:
                return player2 != null ? player2.gameObject : null;
            default:
                return null;
        }
    }
    
    // Public method to check if a specific player is active
    public bool IsPlayerActive(GameObject player)
    {
        if (player1 != null && player == player1.gameObject)
            return currentPhase == GamePhase.Player1Phase;
        if (player2 != null && player == player2.gameObject)
            return currentPhase == GamePhase.Player2Phase;
        return false;
    }
    
    // Public property to get current phase
    public GamePhase CurrentPhase => currentPhase;
    
    #region Timer Methods
    
    private void StartTimer()
    {
        if (!enableTimer) return;
        
        currentTimer = phaseTimerDuration;
        timerActive = true;
        UpdateTimerUI();
        Debug.Log($"Timer started for {currentPhase}: {phaseTimerDuration} seconds");
    }
    
    private void StopTimer()
    {
        timerActive = false;
        if (timerText != null)
        {
            timerText.text = "";
        }
    }
    
    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            // Format timer as MM:SS
            int minutes = Mathf.FloorToInt(currentTimer / 60f);
            int seconds = Mathf.FloorToInt(currentTimer % 60f);
            timerText.text = $"Time: {minutes:00}:{seconds:00}";
            
            // Change color to red when time is running low (last 10 seconds)
            if (currentTimer <= 10f)
            {
                timerText.color = Color.red;
            }
            else
            {
                timerText.color = Color.white;
            }
        }
    }
    
    private void OnTimerExpired()
    {
        Debug.Log("Time's up! Resetting current phase...");
        
        // Update UI to show time expired
        if (timerText != null)
        {
            timerText.text = "TIME'S UP!";
            timerText.color = Color.red;
        }
        
        // Reset the current phase based on the new flow requirements
        if (currentPhase == GamePhase.Player1Phase)
        {
            // Player 1 failed to reach goal - reset Phase 1
            if (debugMode)
            {
                Debug.Log("Player 1 failed to reach goal in time. Resetting Phase 1.");
            }
            
            Invoke(nameof(ResetPhase1), 1f); // Short delay to show "TIME'S UP!" message
        }
        else
        {
            // Player 2 failed to shoot Player 1 - reset Phase 2
            if (debugMode)
            {
                Debug.Log("Player 2 failed to shoot Player 1 in time. Resetting Phase 2.");
            }
            
            Invoke(nameof(ResetPhase2), 1f); // Short delay to show "TIME'S UP!" message
        }
    }
    
    #endregion
    
    #region Phase Reset and Level Management
    
    private void ResetPhase1()
    {
        // Reset Phase 1 - Player 1 tries again to reach the goal
        if (debugMode)
        {
            Debug.Log("Resetting Phase 1...");
        }
        
        // Stop any active recordings/replays
        if (player1Recorder != null)
        {
            player1Recorder.StopRecording();
        }
        if (player2Replayer != null)
        {
            player2Replayer.StopReplay();
        }
        
        // Restart Phase 1 with same conditions
        StartPlayer1Phase();
    }
    
    private void ResetPhase2()
    {
        // Reset Phase 2 - Player 2 tries again to shoot Player 1
        if (debugMode)
        {
            Debug.Log("Resetting Phase 2...");
        }
        
        // Stop any active recordings/replays
        if (player2Recorder != null)
        {
            player2Recorder.StopRecording();
        }
        if (player1Replayer != null)
        {
            player1Replayer.StopReplay();
        }
        
        // Restart Phase 2 with same Player 1 recording
        StartPlayer2Phase();
    }
    
    private void HandleLevelComplete()
    {
        levelCompleted = true;
        
        if (timerText != null)
        {
            timerText.text = "LEVEL COMPLETE!";
            timerText.color = Color.yellow;
        }
        
        Debug.Log($"Level completed! All {requiredLoopsToComplete} loops finished successfully.");
        
        // Stop any active recordings/replays
        if (player1Recorder != null)
        {
            player1Recorder.StopRecording();
        }
        if (player2Recorder != null)
        {
            player2Recorder.StopRecording();
        }
        if (player1Replayer != null)
        {
            player1Replayer.StopReplay();
        }
        if (player2Replayer != null)
        {
            player2Replayer.StopReplay();
        }
        
        // Deactivate players
        if (player1 != null)
        {
            player1.SetActive(false);
        }
        if (player2 != null)
        {
            player2.SetActive(false);
        }
        
        // Trigger level completed event
        GameEvents.TriggerLevelCompleted();
        
        // Here you could add level transition logic, scores, etc.
        // For now, we'll just wait for manual restart or next level trigger
        Debug.Log("Waiting for next level or restart...");
        
        // Example: Auto-transition to next level after delay
        // Invoke(nameof(LoadNextLevel), 3f);
    }
    
    private void UpdateLoopUI()
    {
        // Update UI to show current loop progress (if you have additional UI elements)
        // This method can be expanded to update progress bars, loop counters, etc.
        
        if (debugMode && timerText != null)
        {
            // During gameplay, don't override the timer text, but you could add separate UI elements
            // For now, this is just a placeholder for future UI enhancements
        }
    }
    
    // Event handler for Player 1 death in Phase 1
    private void OnPlayer1Died()
    {
        if (currentPhase == GamePhase.Player1Phase)
        {
            Debug.Log("Player 1 died in Phase 1! Resetting Phase 1...");
            StopTimer();
            
            if (timerText != null)
            {
                timerText.text = "PLAYER 1 DIED!";
                timerText.color = Color.red;
            }
            
            Invoke(nameof(ResetPhase1), 1f);
        }
    }
    
    // Event handler for Player 2 death in Phase 2
    private void OnPlayer2Died()
    {
        if (currentPhase == GamePhase.Player2Phase)
        {
            Debug.Log("Player 2 died in Phase 2! Resetting Phase 2...");
            StopTimer();
            
            if (timerText != null)
            {
                timerText.text = "PLAYER 2 DIED!";
                timerText.color = Color.red;
            }
            
            Invoke(nameof(ResetPhase2), 1f);
        }
    }
    
    // Event handler for Player 1 ghost reaching goal in Phase 2 (Player 2 failure)
    private void OnPlayer1GhostReachedGoal()
    {
        if (currentPhase == GamePhase.Player2Phase)
        {
            Debug.Log("Player 1 (ghost) reached the goal! Player 2 failed. Resetting Phase 2...");
            StopTimer();
            
            if (timerText != null)
            {
                timerText.text = "PLAYER 2 FAILED!";
                timerText.color = Color.red;
            }
            
            Invoke(nameof(ResetPhase2), 1f);
        }
    }
    
    #endregion
    
    #region Event Management
    
    private void SubscribeToEvents()
    {
        // Subscribe to all relevant game events
        GameEvents.OnPlayer1ReachedGoal += OnPlayer1ReachedGoal;
        GameEvents.OnPlayer1GhostReachedGoal += OnPlayer1GhostReachedGoal;
        GameEvents.OnPlayer1Died += OnPlayer1Died;
        GameEvents.OnPlayer2Victory += OnPlayer2Victory;
        GameEvents.OnPlayer2Died += OnPlayer2Died;
        GameEvents.OnTimerExpired += OnTimerExpired;
        
        // Optional: Subscribe to debug messages if debug mode is enabled
        if (debugMode)
        {
            GameEvents.OnDebugMessage += OnDebugMessage;
        }
        
        Debug.Log("PlayerManager: Subscribed to game events");
    }
    
    private void UnsubscribeFromEvents()
    {
        // Unsubscribe from all events to prevent memory leaks
        GameEvents.OnPlayer1ReachedGoal -= OnPlayer1ReachedGoal;
        GameEvents.OnPlayer1GhostReachedGoal -= OnPlayer1GhostReachedGoal;
        GameEvents.OnPlayer1Died -= OnPlayer1Died;
        GameEvents.OnPlayer2Victory -= OnPlayer2Victory;
        GameEvents.OnPlayer2Died -= OnPlayer2Died;
        GameEvents.OnTimerExpired -= OnTimerExpired;
        
        if (debugMode)
        {
            GameEvents.OnDebugMessage -= OnDebugMessage;
        }
        
        Debug.Log("PlayerManager: Unsubscribed from game events");
    }
    
    private void OnDebugMessage(string message)
    {
        if (debugMode)
        {
            Debug.Log($"[GameEvents] {message}");
        }
    }
    
    #endregion
    
    #region Recording Setup
    
    private void SetupRecordingComponents()
    {
        // Auto-assign recording components if not manually assigned
        if (player1 != null)
        {
            if (player1Recorder == null)
                player1Recorder = player1.GetComponent<MovementRecorder>();
            if (player1Replayer == null)
                player1Replayer = player1.GetComponent<MovementReplayer>();
        }
        
        if (player2 != null)
        {
            if (player2Recorder == null)
                player2Recorder = player2.GetComponent<MovementRecorder>();
            if (player2Replayer == null)
                player2Replayer = player2.GetComponent<MovementReplayer>();
        }
        
        // Log warnings if components are missing
        if (player1 != null && (player1Recorder == null || player1Replayer == null))
        {
            Debug.LogWarning("Player 1 is missing MovementRecorder or MovementReplayer components!");
        }
        
        if (player2 != null && (player2Recorder == null || player2Replayer == null))
        {
            Debug.LogWarning("Player 2 is missing MovementRecorder or MovementReplayer components!");
        }
    }
    
    // Public method to get current loop count (useful for UI or debugging)
    public int GetCompletedLoops()
    {
        return completedLoops;
    }
    
    // Public method to get required loops for this level
    public int GetRequiredLoops()
    {
        return requiredLoopsToComplete;
    }
    
    // Public method to check if we're in the first Phase 1
    public bool IsFirstPhase1()
    {
        return isFirstPhase1;
    }
    
    // Public method to check if level is completed
    public bool IsLevelCompleted()
    {
        return levelCompleted;
    }
    
    // Public method to get current loop progress (useful for UI)
    public float GetLoopProgress()
    {
        if (requiredLoopsToComplete == 0) return 0f;
        return (float)completedLoops / requiredLoopsToComplete;
    }
    
    // Public method to get the active recorder (useful for debugging)
    public MovementRecorder GetActiveRecorder()
    {
        switch (currentPhase)
        {
            case GamePhase.Player1Phase:
                return player1Recorder;
            case GamePhase.Player2Phase:
                return player2Recorder;
            default:
                return null;
        }
    }
    
    // Public method to get the active replayer (useful for debugging)
    public MovementReplayer GetActiveReplayer()
    {
        switch (currentPhase)
        {
            case GamePhase.Player1Phase:
                return player2Replayer; // Player 2 replays during Player 1's turn
            case GamePhase.Player2Phase:
                return player1Replayer; // Player 1 replays during Player 2's turn
            default:
                return null;
        }
    }
    
    #endregion
    
    void OnDestroy()
    {
        // Clean up event subscriptions
        UnsubscribeFromEvents();
        
        // Clear all game events when PlayerManager is destroyed (e.g., scene change)
        GameEvents.ClearAllEvents();
    }
}