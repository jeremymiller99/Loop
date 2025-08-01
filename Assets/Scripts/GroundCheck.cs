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
        if (other.CompareTag("Ground"))
        {
            if (playerController != null)
            {
                playerController.SetGrounded(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Ground"))
        {
            if (playerController != null)
            {
                playerController.SetGrounded(false);
            }
        }
    }
} 