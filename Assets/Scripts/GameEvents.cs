using UnityEngine;
using System;

/// <summary>
/// Centralized event system for the game to avoid expensive FindFirstObjectByType calls
/// and create loose coupling between components
/// </summary>
public static class GameEvents
{
    // Player 1 Events
    public static event Action OnPlayer1ReachedGoal;
    public static event Action OnPlayer1GhostReachedGoal; // When Player 1's replay completes in Phase 2
    public static event Action OnPlayer1Died; // When Player 1 hits spike in Phase 1
    
    // Player 2 Events  
    public static event Action OnPlayer2Victory; // When Player 2 shoots Player 1
    public static event Action OnPlayer2Died; // When Player 2 hits spike in Phase 2
    
    // Timer Events
    public static event Action OnTimerExpired;
    
    // Phase Events
    public static event Action<GamePhase> OnPhaseStarted;
    public static event Action<GamePhase> OnPhaseReset;
    
    // Level Events
    public static event Action<int, int> OnLoopCompleted; // (completedLoops, totalRequired)
    public static event Action OnLevelCompleted;
    
    // Debug Events
    public static event Action<string> OnDebugMessage; // For centralized debug logging
    
    #region Trigger Methods
    
    public static void TriggerPlayer1ReachedGoal()
    {
        OnPlayer1ReachedGoal?.Invoke();
        TriggerDebugMessage("Player 1 reached goal in Phase 1");
    }
    
    public static void TriggerPlayer1GhostReachedGoal()
    {
        OnPlayer1GhostReachedGoal?.Invoke();
        TriggerDebugMessage("Player 1 ghost reached goal in Phase 2 - Player 2 failed");
    }
    
    public static void TriggerPlayer1Died()
    {
        OnPlayer1Died?.Invoke();
        TriggerDebugMessage("Player 1 died in Phase 1");
    }
    
    public static void TriggerPlayer2Victory()
    {
        OnPlayer2Victory?.Invoke();
        TriggerDebugMessage("Player 2 shot Player 1 - victory!");
    }
    
    public static void TriggerPlayer2Died()
    {
        OnPlayer2Died?.Invoke();
        TriggerDebugMessage("Player 2 died in Phase 2");
    }
    
    public static void TriggerTimerExpired()
    {
        OnTimerExpired?.Invoke();
        TriggerDebugMessage("Timer expired");
    }
    
    public static void TriggerPhaseStarted(GamePhase phase)
    {
        OnPhaseStarted?.Invoke(phase);
        TriggerDebugMessage($"Phase {phase} started");
    }
    
    public static void TriggerPhaseReset(GamePhase phase)
    {
        OnPhaseReset?.Invoke(phase);
        TriggerDebugMessage($"Phase {phase} reset");
    }
    
    public static void TriggerLoopCompleted(int completedLoops, int totalRequired)
    {
        OnLoopCompleted?.Invoke(completedLoops, totalRequired);
        TriggerDebugMessage($"Loop {completedLoops}/{totalRequired} completed");
    }
    
    public static void TriggerLevelCompleted()
    {
        OnLevelCompleted?.Invoke();
        TriggerDebugMessage("Level completed!");
    }
    
    public static void TriggerDebugMessage(string message)
    {
        OnDebugMessage?.Invoke(message);
    }
    
    #endregion
    
    #region Cleanup
    
    /// <summary>
    /// Clear all event subscriptions - call this when changing scenes or restarting
    /// </summary>
    public static void ClearAllEvents()
    {
        OnPlayer1ReachedGoal = null;
        OnPlayer1GhostReachedGoal = null;
        OnPlayer1Died = null;
        OnPlayer2Victory = null;
        OnPlayer2Died = null;
        OnTimerExpired = null;
        OnPhaseStarted = null;
        OnPhaseReset = null;
        OnLoopCompleted = null;
        OnLevelCompleted = null;
        OnDebugMessage = null;
        
        Debug.Log("GameEvents: All events cleared");
    }
    
    #endregion
}