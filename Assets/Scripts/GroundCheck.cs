using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    private Player1Controller player1Controller;
    private Player2Controller player2Controller;
    private int groundContactCount = 0; // Track multiple ground contacts
    private bool wasGrounded = false; // Track previous grounded state for landing detection
    private CameraShake cameraShake; // Reference to camera shake script

    void Start()
    {
        // Try to get either player controller from the parent
        player1Controller = GetComponentInParent<Player1Controller>();
        player2Controller = GetComponentInParent<Player2Controller>();
        
        // Find camera shake script in the scene
        cameraShake = FindFirstObjectByType<CameraShake>();
        
        // Initialize wasGrounded based on current state
        wasGrounded = (player1Controller != null && player1Controller.IsGrounded) || 
                     (player2Controller != null && player2Controller.IsGrounded);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object is ground or another player
        if (other.CompareTag("Ground") || other.CompareTag("Player"))
        {
            // Don't count self as ground
            if (other.transform.parent != transform.parent)
            {
                groundContactCount++;
                SetGrounded(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Check if the object is ground or another player
        if (other.CompareTag("Ground") || other.CompareTag("Player"))
        {
            // Don't count self as ground
            if (other.transform.parent != transform.parent)
            {
                groundContactCount--;
                if (groundContactCount <= 0)
                {
                    groundContactCount = 0; // Ensure it doesn't go negative
                    SetGrounded(false);
                }
            }
        }
    }
    
    private void SetGrounded(bool grounded)
    {
        // Check for landing (was not grounded, now is grounded)
        bool isLanding = !wasGrounded && grounded;
        
        // Set grounded state for whichever player controller exists
        if (player1Controller != null)
        {
            player1Controller.SetGrounded(grounded);
            
            // Trigger camera shake on landing for active player only
            if (isLanding && player1Controller.IsActive && cameraShake != null)
            {
                cameraShake.TriggerLandingShake();
            }
        }
        else if (player2Controller != null)
        {
            player2Controller.SetGrounded(grounded);
            
            // Trigger camera shake on landing for active player only
            if (isLanding && player2Controller.IsActive && cameraShake != null)
            {
                cameraShake.TriggerLandingShake();
            }
        }
        
        // Update wasGrounded for next frame
        wasGrounded = grounded;
    }
} 