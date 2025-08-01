using UnityEngine;

public class Spike : MonoBehaviour
{
    [Header("Spike Settings")]
    [SerializeField] private string playerTag = "Player";
    
    private PlayerManager playerManager;
    
    void Start()
    {
        // Find the PlayerManager in the scene
        playerManager = FindFirstObjectByType<PlayerManager>();
        if (playerManager == null)
        {
            Debug.LogWarning("Spike: Could not find PlayerManager in scene!");
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object is a player
        if (other.CompareTag(playerTag))
        {
            // Only reset if it's the currently active player
            GameObject activePlayer = playerManager != null ? playerManager.GetActivePlayer() : null;
            if (activePlayer != null && other.gameObject == activePlayer)
            {
                Debug.Log($"Active player {other.name} hit spike! Resetting current phase...");
                ResetCurrentPhase();
            }
        }
    }
    
    private void ResetCurrentPhase()
    {
        if (playerManager == null) return;
        
        // Tell PlayerManager to restart the current phase (reset positions, etc.)
        playerManager.RestartCurrentPhase();
    }
}
