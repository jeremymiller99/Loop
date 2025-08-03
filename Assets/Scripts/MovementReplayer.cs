using UnityEngine;

public class MovementReplayer : MonoBehaviour
{
    [Header("Replay Settings")]
    [SerializeField] private bool smoothTransitions = true;
    [SerializeField] private float positionLerpSpeed = 10f;
    
    private MovementRecording recordingToReplay;
    private bool isReplaying = false;
    private float replayTimer = 0f;
    
    // Component references
    private Rigidbody2D rb;
    private Player1Controller player1Controller;
    
    // This replayer is now only used for Player 1
    
    // Replay state
    private MovementFrame currentFrame;
    private MovementFrame previousFrame;
    private bool hasFinishedRecording = false;
    private bool hasNotifiedCompletion = false; // Prevent multiple completion notifications

    void Awake()
    {
        // Get component references early to ensure they're available
        rb = GetComponent<Rigidbody2D>();
        player1Controller = GetComponent<Player1Controller>();
    }
    
    // Start method removed - no longer needed since we don't handle Player 2 shooting

    void Update()
    {
        if (!isReplaying || recordingToReplay == null) return;
        
        replayTimer += Time.deltaTime;
        
        // Get the current frame to replay
        currentFrame = recordingToReplay.GetFrameAtTime(replayTimer);
        
        if (currentFrame != null)
        {
            ApplyMovementFrame(currentFrame);
        }
        
        // Check if replay has finished
        if (replayTimer >= recordingToReplay.duration)
        {
            if (!hasFinishedRecording)
            {
                hasFinishedRecording = true;
                
                // Keep the player at the last position but stop active movement
                if (currentFrame != null)
                {
                    ApplyFinalFrame(currentFrame);
                }
                
                // Player 1's replay finishing during Phase 2 means Player 1 reached the goal
                // (since the recording only ended because Player 1 successfully reached the goal in Phase 1)
                if (!hasNotifiedCompletion)
                {
                    hasNotifiedCompletion = true;
                    HandlePlayer1ReplayComplete();
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (!isReplaying || recordingToReplay == null || currentFrame == null) return;
        
        // Apply physics-based movement in FixedUpdate for consistency
        if (!hasFinishedRecording)
        {
            ApplyPhysicsMovement(currentFrame);
        }
    }

    public void StartReplay(MovementRecording recording)
    {
        if (recording == null || recording.frames.Count == 0)
        {
            Debug.LogWarning($"Cannot start replay for {gameObject.name} - no valid recording provided");
            return;
        }
        
        recordingToReplay = recording;
        isReplaying = true;
        replayTimer = 0f;
        hasFinishedRecording = false;
        hasNotifiedCompletion = false; // Reset completion notification flag
        
        // Move player to starting position
        transform.position = recording.startPosition;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
        
        // Disable player input during replay
        if (player1Controller != null)
        {
            player1Controller.SetActive(false);
        }
        
        Debug.Log($"Started replaying movements for {gameObject.name}. Duration: {recording.duration:F2}s, Frames: {recording.frames.Count}");
    }

    public void StopReplay()
    {
        isReplaying = false;
        recordingToReplay = null;
        hasFinishedRecording = false;
        hasNotifiedCompletion = false; // Reset completion notification flag
        replayTimer = 0f;
        
        Debug.Log($"Stopped replaying movements for {gameObject.name}");
    }

    private void ApplyMovementFrame(MovementFrame frame)
    {
        if (frame == null) return;
        
        // Apply position with smooth transitions if enabled
        if (smoothTransitions)
        {
            transform.position = Vector3.Lerp(transform.position, frame.position, positionLerpSpeed * Time.deltaTime);
        }
        else
        {
            transform.position = frame.position;
        }
        
        // Note: Player 2 specific logic (facing direction, shooting) removed
        // since this replayer is now only used for Player 1
    }

    private void ApplyPhysicsMovement(MovementFrame frame)
    {
        if (rb == null || frame == null) return;
        
        // Set velocity to match recorded movement
        if (!smoothTransitions)
        {
            rb.linearVelocity = frame.velocity;
        }
        else
        {
            // Smooth velocity transitions
            rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, frame.velocity, positionLerpSpeed * Time.fixedDeltaTime);
        }
    }

    private void ApplyFinalFrame(MovementFrame frame)
    {
        // Keep player at final position but stop all movement
        transform.position = frame.position;
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    // Player 2 specific methods removed - ApplyFacingDirection and SimulateShoot
    // since this replayer is now only used for Player 1

    public bool IsReplaying()
    {
        return isReplaying;
    }

    public bool HasFinishedRecording()
    {
        return hasFinishedRecording;
    }

    public float GetReplayProgress()
    {
        if (recordingToReplay == null || recordingToReplay.duration == 0) return 0f;
        return Mathf.Clamp01(replayTimer / recordingToReplay.duration);
    }

    public float GetRemainingReplayTime()
    {
        if (recordingToReplay == null) return 0f;
        return Mathf.Max(0f, recordingToReplay.duration - replayTimer);
    }

    // Method to check if there's a valid recording to replay
    public bool HasValidRecording()
    {
        return recordingToReplay != null && recordingToReplay.frames.Count > 0;
    }

    // Method to clear the current replay recording
    public void ClearReplayRecording()
    {
        StopReplay();
        recordingToReplay = null;
    }
    
    // Handle when Player 1's replay completes during Phase 2
    private void HandlePlayer1ReplayComplete()
    {
        Debug.Log("Player 1 replay completed - this means Player 1 reached the goal! Player 2 failed.");
        
        // Trigger event - much more efficient than FindFirstObjectByType
        GameEvents.TriggerPlayer1GhostReachedGoal();
    }
}