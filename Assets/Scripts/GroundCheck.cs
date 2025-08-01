using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    private Player1Controller playerController;

    void Start()
    {
        // Get the Player1Controller from the parent
        playerController = GetComponentInParent<Player1Controller>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[GroundCheck] Trigger ENTER: {other.name} (Tag: {other.tag})");
        
        if (other.CompareTag("Ground"))
        {
            Debug.Log($"[GroundCheck] Ground detected! Setting grounded = true");
            if (playerController != null)
            {
                playerController.SetGrounded(true);
            }
            else
            {
                Debug.LogError("[GroundCheck] Player1Controller is null!");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log($"[GroundCheck] Trigger EXIT: {other.name} (Tag: {other.tag})");
        
        if (other.CompareTag("Ground"))
        {
            Debug.Log($"[GroundCheck] Ground lost! Setting grounded = false");
            if (playerController != null)
            {
                playerController.SetGrounded(false);
            }
            else
            {
                Debug.LogError("[GroundCheck] Player1Controller is null!");
            }
        }
    }
} 