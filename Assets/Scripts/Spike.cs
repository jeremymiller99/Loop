using UnityEngine;
using UnityEngine.SceneManagement;

public class Spike : MonoBehaviour
{
    [Header("Spike Settings")]
    [SerializeField] private string playerTag = "Player";
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag(playerTag))
        {
            RestartScene();
        }
    }
    
    private void RestartScene()
    {
        // Get the current active scene and reload it
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
