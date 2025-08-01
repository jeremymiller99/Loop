using UnityEngine;
using System;

public class GoalTrigger : MonoBehaviour
{
    [Header("Goal Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool onlyPlayer1CanTrigger = true;
    
    // Event that PlayerManager will subscribe to
    public event Action OnGoalReached;
    
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
            
            // Also check if Player 1 is actually active (in case both players are present)
            if (!player1.IsActive) return;
        }
        
        // Goal reached!
        goalTriggered = true;
        Debug.Log($"Goal reached by {other.name}!");
        
        // Notify subscribers (PlayerManager)
        OnGoalReached?.Invoke();
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