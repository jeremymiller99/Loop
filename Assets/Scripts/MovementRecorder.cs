using UnityEngine;

public class MovementRecorder : MonoBehaviour
{
    [Header("Recording Settings")]
    [SerializeField] private float recordingFrameRate = 60f; // Frames per second to record
    
    private MovementRecording currentRecording;
    private bool isRecording = false;
    private float recordTimer = 0f;
    private float frameInterval;
    
    // Component references
    private Rigidbody2D rb;
    private Player1Controller player1Controller;
    private Player2Controller player2Controller;
    private Camera playerCamera;
    
    // Player type detection
    private bool isPlayer1;
    private bool isPlayer2;

    void Awake()
    {
        frameInterval = 1f / recordingFrameRate;
        
        // Initialize recording first - this must happen before any other component tries to use it
        currentRecording = new MovementRecording();
        
        // Get component references
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
        if (!isRecording) return;
        
        recordTimer += Time.deltaTime;
        
        // Record at specified frame rate
        if (recordTimer >= frameInterval)
        {
            RecordCurrentFrame();
            recordTimer = 0f;
        }
    }

    public void StartRecording()
    {
        // Safety check - ensure currentRecording is initialized
        if (currentRecording == null)
        {
            Debug.LogWarning($"MovementRecorder on {gameObject.name}: currentRecording was null, initializing now");
            currentRecording = new MovementRecording();
        }
        
        isRecording = true;
        recordTimer = 0f;
        currentRecording.Clear();
        currentRecording.startPosition = transform.position;
        
        Debug.Log($"Started recording movements for {gameObject.name}");
    }

    public void StopRecording()
    {
        if (!isRecording) return;
        
        isRecording = false;
        currentRecording.isComplete = true;
        
        Debug.Log($"Stopped recording movements for {gameObject.name}. Duration: {currentRecording.duration:F2}s, Frames: {currentRecording.frames.Count}");
    }

    private void RecordCurrentFrame()
    {
        if (rb == null) return;
        
        float currentTime = recordTimer + (currentRecording.frames.Count * frameInterval);
        Vector3 currentPosition = transform.position;
        Vector3 currentVelocity = rb.linearVelocity;
        
        // Get ground state from the controllers
        bool grounded = false;
        if (isPlayer1 && player1Controller != null)
        {
            grounded = player1Controller.IsGrounded;
        }
        else if (isPlayer2 && player2Controller != null)
        {
            grounded = player2Controller.IsGrounded;
        }
        
        // Record input states
        float horizontalInput = Input.GetAxisRaw("Horizontal");
        bool jumpInput = Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.UpArrow);
        bool shootInput = false;
        Vector3 mousePosition = Vector3.zero;
        bool facingRight = true;
        
        // Player 2 specific inputs
        if (isPlayer2)
        {
            shootInput = Input.GetMouseButton(0);
            if (playerCamera != null)
            {
                mousePosition = Input.mousePosition;
                mousePosition = playerCamera.ScreenToWorldPoint(mousePosition);
                mousePosition.z = 0f;
            }
            
            // Get facing direction from Player2Controller
            facingRight = player2Controller.IsFacingRight;
        }
        
        MovementFrame frame = new MovementFrame(
            currentTime,
            currentPosition,
            currentVelocity,
            grounded,
            horizontalInput,
            jumpInput,
            shootInput,
            mousePosition,
            facingRight
        );
        
        currentRecording.AddFrame(frame);
    }



    public MovementRecording GetRecording()
    {
        return currentRecording;
    }

    public MovementRecording GetCompletedRecording()
    {
        if (currentRecording.isComplete)
            return currentRecording;
        return null;
    }

    public bool HasRecording()
    {
        return currentRecording != null && currentRecording.frames.Count > 0;
    }

    public bool IsRecording()
    {
        return isRecording;
    }

    public void ClearRecording()
    {
        currentRecording.Clear();
        Debug.Log($"Cleared recording for {gameObject.name}");
    }

    // Method to manually set a recording (useful for loading previous recordings)
    public void SetRecording(MovementRecording recording)
    {
        currentRecording = recording;
        Debug.Log($"Set recording for {gameObject.name} - Duration: {recording.duration:F2}s, Frames: {recording.frames.Count}");
    }
}