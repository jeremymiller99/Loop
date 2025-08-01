using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum GamePhase
{
    Player1Phase,    // Player 1 tries to reach goal
    Player2Phase     // Player 2 tries to shoot Player 1
}

public class PlayerManager : MonoBehaviour
{
    [Header("Player References")]
    [SerializeField] private Player1Controller player1;
    [SerializeField] private Player2Controller player2;
    
    [Header("Goal Reference")]
    [SerializeField] private GoalTrigger goalTrigger;
    
    [Header("Timer Settings")]
    [SerializeField] private float phaseTimerDuration = 30f; // 30 seconds per phase
    [SerializeField] private Text timerText; // UI Text to display countdown
    [SerializeField] private bool enableTimer = true; // Toggle to enable/disable timer
    
    private GamePhase currentPhase = GamePhase.Player1Phase;
    private Vector3 player1StartPosition;
    private Vector3 player2StartPosition;
    
    // Timer variables
    private float currentTimer;
    private bool timerActive;
    
    void Start()
    {
        // Store starting positions for reset functionality
        if (player1 != null)
            player1StartPosition = player1.transform.position;
        if (player2 != null)
            player2StartPosition = player2.transform.position;
        
        // Set up goal trigger callback
        if (goalTrigger != null)
            goalTrigger.OnGoalReached += HandlePlayer1Victory;
        
        // Start with Player 1 phase
        StartPlayer1Phase();
    }
    
    void Update()
    {
        // Handle debug input to restart level (R key)
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }
        
        // Update timer if active
        if (timerActive && enableTimer)
        {
            currentTimer -= Time.deltaTime;
            UpdateTimerUI();
            
            // Check if time ran out
            if (currentTimer <= 0f)
            {
                HandleTimerExpired();
            }
        }
    }
    
    private void StartPlayer1Phase()
    {
        currentPhase = GamePhase.Player1Phase;
        
        // Player 1 is active and controllable
        if (player1 != null)
            player1.SetActive(true);
        
        // Player 2 is inactive (present but not controllable)
        if (player2 != null)
            player2.SetActive(false);
        
        // Start the timer for this phase
        StartTimer();
        
        Debug.Log("Phase 1: Control Player 1 to reach the goal!");
    }
    
    private void StartPlayer2Phase()
    {
        currentPhase = GamePhase.Player2Phase;
        
        // Reset player positions
        ResetPlayerPositions();
        
        // Player 2 is active and controllable
        if (player2 != null)
            player2.SetActive(true);
        
        // Player 1 is inactive (present but not controllable)
        if (player1 != null)
            player1.SetActive(false);
        
        // Start the timer for this phase
        StartTimer();
        
        Debug.Log("Phase 2: Control Player 2 to shoot Player 1!");
    }
    
    private void ResetPlayerPositions()
    {
        if (player1 != null)
        {
            player1.transform.position = player1StartPosition;
            // Reset any physics state
            Rigidbody2D rb1 = player1.GetComponent<Rigidbody2D>();
            if (rb1 != null)
            {
                rb1.linearVelocity = Vector2.zero;
                rb1.angularVelocity = 0f;
            }
        }
        
        if (player2 != null)
        {
            player2.transform.position = player2StartPosition;
            // Reset any physics state
            Rigidbody2D rb2 = player2.GetComponent<Rigidbody2D>();
            if (rb2 != null)
            {
                rb2.linearVelocity = Vector2.zero;
                rb2.angularVelocity = 0f;
            }
        }
    }
    
    private void HandlePlayer1Victory()
    {
        Debug.Log("Player 1 reached the goal! Switching to Player 2 phase...");
        StopTimer(); // Stop the current phase timer
        StartPlayer2Phase();
    }
    
    public void HandlePlayer2Victory()
    {
        Debug.Log("Player 2 shot Player 1! Player 2 wins!");
        StopTimer(); // Stop the timer when game ends
        // You can add win screen, level complete logic, etc. here
        // For now, we'll restart the level after a delay
        Invoke(nameof(RestartLevel), 2f);
    }
    
    public void RestartCurrentPhase()
    {
        Debug.Log($"Restarting current phase: {currentPhase}");
        
        // Reset player positions without changing phases
        ResetPlayerPositions();
        
        // Reset goal trigger if needed
        if (goalTrigger != null)
        {
            goalTrigger.ResetGoal();
        }
        
        // Restart the timer for this phase
        StartTimer();
        
        Debug.Log($"Phase {currentPhase} has been reset!");
    }
    
    private void RestartLevel()
    {
        // Reload the current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    // Public method to get the currently active player (useful for other scripts)
    public GameObject GetActivePlayer()
    {
        switch (currentPhase)
        {
            case GamePhase.Player1Phase:
                return player1 != null ? player1.gameObject : null;
            case GamePhase.Player2Phase:
                return player2 != null ? player2.gameObject : null;
            default:
                return null;
        }
    }
    
    // Public method to check if a specific player is active
    public bool IsPlayerActive(GameObject player)
    {
        if (player1 != null && player == player1.gameObject)
            return currentPhase == GamePhase.Player1Phase;
        if (player2 != null && player == player2.gameObject)
            return currentPhase == GamePhase.Player2Phase;
        return false;
    }
    
    // Public property to get current phase
    public GamePhase CurrentPhase => currentPhase;
    
    #region Timer Methods
    
    private void StartTimer()
    {
        if (!enableTimer) return;
        
        currentTimer = phaseTimerDuration;
        timerActive = true;
        UpdateTimerUI();
        Debug.Log($"Timer started for {currentPhase}: {phaseTimerDuration} seconds");
    }
    
    private void StopTimer()
    {
        timerActive = false;
        if (timerText != null)
        {
            timerText.text = "";
        }
    }
    
    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            // Format timer as MM:SS
            int minutes = Mathf.FloorToInt(currentTimer / 60f);
            int seconds = Mathf.FloorToInt(currentTimer % 60f);
            timerText.text = $"Time: {minutes:00}:{seconds:00}";
            
            // Change color to red when time is running low (last 10 seconds)
            if (currentTimer <= 10f)
            {
                timerText.color = Color.red;
            }
            else
            {
                timerText.color = Color.white;
            }
        }
    }
    
    private void HandleTimerExpired()
    {
        Debug.Log("Time's up! Both players failed to complete the phase in time!");
        timerActive = false;
        
        // Update UI to show time expired
        if (timerText != null)
        {
            timerText.text = "TIME'S UP!";
            timerText.color = Color.red;
        }
        
        // Both players "die" - restart the level after a short delay
        Invoke(nameof(RestartLevel), 2f);
    }
    
    #endregion
    
    void OnDestroy()
    {
        // Clean up event subscriptions
        if (goalTrigger != null)
            goalTrigger.OnGoalReached -= HandlePlayer1Victory;
    }
}