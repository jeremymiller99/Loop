using UnityEngine;

public class Spike : MonoBehaviour
{
    [Header("Spike Settings")]
    [SerializeField] private string playerTag = "Player";
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object is a player
        if (other.CompareTag(playerTag))
        {
            // Check which player hit the spike and trigger appropriate event
            Player1Controller player1 = other.GetComponent<Player1Controller>();
            Player2Controller player2 = other.GetComponent<Player2Controller>();
            
            if (player1 != null && player1.IsActive)
            {
                // Phase 1: Active Player 1 hit spike
                Debug.Log($"Player 1 hit spike! Triggering death event...");
                GameEvents.TriggerPlayer1Died();
            }
            else if (player2 != null && player2.IsActive)
            {
                // Phase 2: Active Player 2 hit spike
                Debug.Log($"Player 2 hit spike! Triggering death event...");
                GameEvents.TriggerPlayer2Died();
            }
            // Ghost players (inactive/replaying) ignore spikes - they're not really "there"
        }
    }
}
