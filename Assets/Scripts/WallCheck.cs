using UnityEngine;

public class WallCheck : MonoBehaviour
{
    private Player1Controller playerController;

    void Start()
    {
        // Get the Player1Controller from the parent
        playerController = GetComponentInParent<Player1Controller>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        bool isRightWall = transform.localPosition.x > 0;
        string side = isRightWall ? "RIGHT" : "LEFT";
        
        Debug.Log($"[WallCheck-{side}] Trigger ENTER: {other.name} (Tag: {other.tag})");
        
        if (other.CompareTag("Ground"))
        {
            Debug.Log($"[WallCheck-{side}] Wall detected! Setting wall contact = true");
            if (playerController != null)
            {
                playerController.SetWallContact(isRightWall, true);
            }
            else
            {
                Debug.LogError($"[WallCheck-{side}] Player1Controller is null!");
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        bool isRightWall = transform.localPosition.x > 0;
        string side = isRightWall ? "RIGHT" : "LEFT";
        
        Debug.Log($"[WallCheck-{side}] Trigger EXIT: {other.name} (Tag: {other.tag})");
        
        if (other.CompareTag("Ground"))
        {
            Debug.Log($"[WallCheck-{side}] Wall lost! Setting wall contact = false");
            if (playerController != null)
            {
                playerController.SetWallContact(isRightWall, false);
            }
            else
            {
                Debug.LogError($"[WallCheck-{side}] Player1Controller is null!");
            }
        }
    }
} 