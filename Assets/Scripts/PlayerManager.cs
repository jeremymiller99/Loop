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
    [SerializeField] private MovementRecorder player1Recorder;     // only record P1
    [SerializeField] private MovementReplayer  player1Replayer;    // replay needed only for P1
    
    [Header("Level Settings")]
    [SerializeField] private bool debugMode = false; // Show debug information
    
    private GamePhase currentPhase = GamePhase.Player1Phase;
    private Vector3 player1StartPosition;
    private Vector3 player2StartPosition;
    
    // Movement recording variables
    private MovementRecording player1LastRecording;
    
    // Manager references
    private GameStateManager gameStateManager;
    private TimerManager timerManager;
    
    // Visual tinting for inactive players
    private Color inactivePlayerTint = new Color(0.4f, 0.4f, 0.4f, 1f); // Dark gray tint
    private Color activePlayerTint = Color.white; // Normal color
    
    void Start()
    {
        // Store starting positions for reset functionality
        if (player1 != null)
            player1StartPosition = player1.transform.position;
        if (player2 != null)
            player2StartPosition = player2.transform.position;
        
        // Initialize manager references
        gameStateManager = GameStateManager.Instance;
        timerManager = TimerManager.Instance;
        
        // Log warnings if managers are missing
        if (gameStateManager == null)
            Debug.LogWarning("PlayerManager: No GameStateManager found in scene!");
        if (timerManager == null)
            Debug.LogWarning("PlayerManager: No TimerManager found in scene!");
        
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
    }
    
    #region Visual Tinting Methods
    
    private void ApplyPlayerTint(GameObject player, Color tintColor)
    {
        if (player == null) return;
        
        // Apply tint to the main sprite renderer
        SpriteRenderer mainRenderer = player.GetComponent<SpriteRenderer>();
        if (mainRenderer != null)
        {
            mainRenderer.color = tintColor;
        }
        
        // Apply tint to all child sprite renderers (weapons, accessories, etc.)
        SpriteRenderer[] childRenderers = player.GetComponentsInChildren<SpriteRenderer>();
        foreach (SpriteRenderer renderer in childRenderers)
        {
            renderer.color = tintColor;
        }
    }
    
    private void SetPlayerActive(GameObject player, bool isActive)
    {
        if (player == null) return;
        
        // Apply visual tint based on active state
        Color targetTint = isActive ? activePlayerTint : inactivePlayerTint;
        ApplyPlayerTint(player, targetTint);
        
        // Enable/disable input control
        if (player == player1?.gameObject)
        {
            player1?.SetActive(isActive);
        }
        else if (player == player2?.gameObject)
        {
            player2?.SetActive(isActive);
        }
    }
    
    #endregion
    
    private void StartPlayer1Phase()
    {
        currentPhase = GamePhase.Player1Phase;
        
        // Reset player positions
        ResetPlayerPositions();
        
        // Player 1 is active and controllable, Player 2 is inactive and grayed
        SetPlayerActive(player1?.gameObject, true);
        SetPlayerActive(player2?.gameObject, false);
        
        // Start recording Player 1's movements
        if (player1Recorder != null)
        {
            player1Recorder.StartRecording();
        }
        
        // Debug information
        if (debugMode && gameStateManager != null)
        {
            bool isFirstPhase = gameStateManager.IsFirstPhase1();
            int currentLoop = gameStateManager.GetCompletedLoops() + 1;
            int totalLoops = gameStateManager.GetRequiredLoops();
            
            string phaseMessage = isFirstPhase ? 
                $"Phase 1: Control Player 1 to reach the goal! (Loop {currentLoop}/{totalLoops} - First attempt)" :
                $"Phase 1: Control Player 1 to reach the goal! (Loop {currentLoop}/{totalLoops} - Player 2 ghost active)";
            
            Debug.Log(phaseMessage);
        }
        
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
        
        // Reset player positions
        ResetPlayerPositions();
        
        // Player 2 is active and controllable, Player 1 is inactive and grayed
        SetPlayerActive(player2?.gameObject, true);
        SetPlayerActive(player1?.gameObject, false);
        
        // Start replaying Player 1's movements from the phase we just completed
        if (player1LastRecording != null && player1Replayer != null)
        {
            Debug.Log("Starting Player 1 replay from Phase 1");
            player1Replayer.StartReplay(player1LastRecording);
        }
        
        // Debug information
        if (debugMode && gameStateManager != null)
        {
            int currentLoop = gameStateManager.GetCompletedLoops() + 1;
            int totalLoops = gameStateManager.GetRequiredLoops();
            Debug.Log($"Phase 2: Control Player 2 to shoot Player 1! (Loop {currentLoop}/{totalLoops} - Player 1 ghost active)");
        }
        
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
        
        // Stop the timer through the manager
        if (timerManager != null)
        {
            timerManager.StopTimer();
        }
        
        // Proceed to Phase 2 of the current loop
        StartPlayer2Phase();
    }
    
    private void OnPlayer2Victory()
    {
        Debug.Log("Player 2 shot Player 1! Loop completed.");
        
        // Stop the timer through the manager
        if (timerManager != null)
        {
            timerManager.StopTimer();
        }
        
        // Stop Player 1's replay immediately
        if (player1Replayer != null)
        {
            player1Replayer.StopReplay();
        }
        
        // Get current state from GameStateManager
        if (gameStateManager != null)
        {
            int completedLoops = gameStateManager.GetCompletedLoops() + 1; // This will be the new count
            int requiredLoops = gameStateManager.GetRequiredLoops();
            
            // Trigger loop completed event (GameStateManager will handle the increment)
            GameEvents.TriggerLoopCompleted(completedLoops, requiredLoops);
            
            // Check if we've completed all required loops
            if (completedLoops >= requiredLoops)
            {
                // Level completed!
                HandleLevelComplete();
            }
            else
            {
                if (debugMode)
                {
                    Debug.Log($"Loop {completedLoops}/{requiredLoops} completed. Starting next loop...");
                }
                
                // Start next loop after short delay
                Invoke(nameof(StartPlayer1Phase), 0.5f);
            }
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
        
        // Restart the timer through the manager
        if (timerManager != null)
        {
            timerManager.StartTimer();
        }
        
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
    

    
    #region Phase Reset and Level Management
    
    private void OnTimerExpired()
    {
        Debug.Log("Time's up! Resetting current phase...");
        
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
        if (player1Replayer != null)
        {
            player1Replayer.StopReplay();
        }
        
        // Restart Phase 2 with same Player 1 recording
        StartPlayer2Phase();
    }
    
    private void HandleLevelComplete()
    {
        if (gameStateManager != null)
        {
            int requiredLoops = gameStateManager.GetRequiredLoops();
            Debug.Log($"Level completed! All {requiredLoops} loops finished successfully.");
        }
        else
        {
            Debug.Log("Level completed!");
        }
        
        // Stop any active recordings/replays
        if (player1Recorder != null)
        {
            player1Recorder.StopRecording();
        }
        if (player1Replayer != null)
        {
            player1Replayer.StopReplay();
        }
        
        // Stop the timer
        if (timerManager != null)
        {
            timerManager.StopTimer();
        }
        
        // Deactivate both players (they'll be grayed out)
        SetPlayerActive(player1?.gameObject, false);
        SetPlayerActive(player2?.gameObject, false);
        
        // Go directly to next level instead of showing UI
        Debug.Log("Automatically transitioning to next level...");
        
        // Use LevelCompleteManager to handle next level logic
        if (LevelCompleteManager.Instance != null)
        {
            // Short delay to let the victory feel satisfying, then go to next level
            Invoke(nameof(TransitionToNextLevel), 1.5f);
        }
        else
        {
            Debug.LogWarning("No LevelCompleteManager found! Cannot transition to next level.");
        }
    }
    
    private void TransitionToNextLevel()
    {
        if (LevelCompleteManager.Instance != null)
        {
            // Check if there's a next level configured
            if (LevelCompleteManager.Instance.HasNextLevel())
            {
                Debug.Log("Going to next level...");
                LevelCompleteManager.Instance.GoToNextLevel();
            }
            else
            {
                Debug.Log("No next level configured. Going to level select...");
                LevelCompleteManager.Instance.GoToLevelSelect();
            }
        }
        else
        {
            Debug.LogError("No LevelCompleteManager instance found!");
        }
    }

    
    // Event handler for Player 1 death in Phase 1
    private void OnPlayer1Died()
    {
        if (currentPhase == GamePhase.Player1Phase)
        {
            Debug.Log("Player 1 died in Phase 1! Resetting Phase 1...");
            
            // Stop the timer through the manager
            if (timerManager != null)
            {
                timerManager.StopTimer();
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
            
            // Stop the timer through the manager
            if (timerManager != null)
            {
                timerManager.StopTimer();
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
            
            // Stop the timer through the manager
            if (timerManager != null)
            {
                timerManager.StopTimer();
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
        GameEvents.OnPlayer1Shot += OnPlayer1Died; // Same handler for both spike and shot deaths
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
        GameEvents.OnPlayer1Shot -= OnPlayer1Died;
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
        
        // No longer need Player 2 recorder
        
        // Log warnings if components are missing
        if (player1 != null && (player1Recorder == null || player1Replayer == null))
        {
            Debug.LogWarning("Player 1 is missing MovementRecorder or MovementReplayer components!");
        }
        
        // No longer need Player 2 recorder component
    }
    
    // Public method to get current loop count (delegates to GameStateManager)
    public int GetCompletedLoops()
    {
        return gameStateManager != null ? gameStateManager.GetCompletedLoops() : 0;
    }
    
    // Public method to get required loops for this level (delegates to GameStateManager)
    public int GetRequiredLoops()
    {
        return gameStateManager != null ? gameStateManager.GetRequiredLoops() : 1;
    }
    
    // Public method to check if we're in the first Phase 1 (delegates to GameStateManager)
    public bool IsFirstPhase1()
    {
        return gameStateManager != null ? gameStateManager.IsFirstPhase1() : true;
    }
    
    // Public method to check if level is completed (delegates to GameStateManager)
    public bool IsLevelCompleted()
    {
        return gameStateManager != null ? gameStateManager.IsLevelCompleted() : false;
    }
    
    // Public method to get current loop progress (delegates to GameStateManager)
    public float GetLoopProgress()
    {
        return gameStateManager != null ? gameStateManager.GetLoopProgress() : 0f;
    }
    
    // Public method to get the active recorder (useful for debugging)
    public MovementRecorder GetActiveRecorder()
    {
        return currentPhase == GamePhase.Player1Phase ? player1Recorder : null;
    }
    
    // Public method to get the active replayer (useful for debugging)
    public MovementReplayer GetActiveReplayer()
    {
        return currentPhase == GamePhase.Player2Phase ? player1Replayer : null;
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