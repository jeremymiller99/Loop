using UnityEngine;
using System;

public class GoalTrigger : MonoBehaviour
{
    [Header("Goal Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool onlyPlayer1CanTrigger = true;
    
    // Note: Now using centralized GameEvents system instead of individual events
    
    private bool goalTriggered = false;
    
    void Start()
    {
        // Ensure this object has a trigger collider
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
        else
        {
            Debug.LogWarning("GoalTrigger needs a Collider2D component set as a trigger!");
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        // Prevent multiple triggers
        if (goalTriggered) return;
        
        // Check if it's a player
        if (!other.CompareTag(playerTag)) return;
        
        // If we only want Player 1 to trigger this, check the name or specific component
        if (onlyPlayer1CanTrigger)
        {
            Player1Controller player1 = other.GetComponent<Player1Controller>();
            if (player1 == null) return;
            
            // Only trigger for active Player 1 (Phase 1)
            // In Phase 2, Player 1 replay completion is handled by MovementReplayer
            if (player1.IsActive)
            {
                // Phase 1: Player 1 is actively controlled and reached goal
                goalTriggered = true;
                Debug.Log($"Goal reached by active Player 1 in Phase 1!");
                
                // Trigger event - much more efficient than FindFirstObjectByType
                GameEvents.TriggerPlayer1ReachedGoal();
            }
            // Note: Phase 2 goal detection is now handled by MovementReplayer.HandlePlayer1ReplayComplete()
            // when the Player 1 recording finishes, which is more reliable than trigger detection
        }
        else
        {
            // General case - any player can trigger
            goalTriggered = true;
            Debug.Log($"Goal reached by {other.name}!");
            
            // Trigger generic goal event
            GameEvents.TriggerPlayer1ReachedGoal();
        }
    }
    
    // Method to reset the goal (useful when restarting phases)
    public void ResetGoal()
    {
        goalTriggered = false;
    }
    
    // Method to set whether only Player 1 can trigger (useful for different game modes)
    public void SetPlayer1Only(bool player1Only)
    {
        onlyPlayer1CanTrigger = player1Only;
    }
}