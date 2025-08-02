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
    private Player2Controller player2Controller;
    private Camera playerCamera;
    
    // Player type detection
    private bool isPlayer1;
    private bool isPlayer2;
    
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
        player2Controller = GetComponent<Player2Controller>();
        
        // Determine player type
        isPlayer1 = player1Controller != null;
        isPlayer2 = player2Controller != null;
    }
    
    void Start()
    {
        // Get camera references in Start (after scene is fully loaded)
        playerCamera = Camera.main;
        
        if (playerCamera == null)
            playerCamera = FindFirstObjectByType<Camera>();
    }

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
                
                // If this is Player 1's replay finishing during Phase 2, it means Player 1 reached the goal
                // (since the recording only ended because Player 1 successfully reached the goal in Phase 1)
                if (isPlayer1 && !hasNotifiedCompletion)
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
        if (isPlayer1 && player1Controller != null)
        {
            player1Controller.SetActive(false);
        }
        else if (isPlayer2 && player2Controller != null)
        {
            player2Controller.SetActive(false);
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
        
        // Apply facing direction for Player 2
        if (isPlayer2)
        {
            ApplyFacingDirection(frame.facingRight);
            
            // Apply shooting if this frame has shooting input
            if (frame.shootInput)
            {
                SimulateShoot(frame.mousePosition);
            }
        }
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

    private void ApplyFacingDirection(bool shouldFaceRight)
    {
        if (!isPlayer2) return;
        
        bool currentlyFacingRight = transform.localScale.x > 0;
        
        if (shouldFaceRight != currentlyFacingRight)
        {
            // Flip the player
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }

    private void SimulateShoot(Vector3 targetPosition)
    {
        if (!isPlayer2 || player2Controller == null) return;
        
        // Use the SimulateShoot method from Player2Controller
        player2Controller.SimulateShoot(targetPosition);
    }

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