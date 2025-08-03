using UnityEngine;

/// <summary>
/// Controls camera behavior to maintain proper aspect ratios and field of view
/// for different screen resolutions, especially important for web builds on itch.io
/// </summary>
public class CameraAspectRatioController : MonoBehaviour
{
    [Header("Aspect Ratio Settings")]
    [SerializeField] private float targetAspectRatio = 1920f / 1080f; // Your game's aspect ratio (16:9)
    [SerializeField] private bool maintainHeight = true; // If true, maintains height and adjusts width
    
    [Header("Orthographic Settings")]
    [SerializeField] private float baseOrthographicSize = 10f; // Your original orthographic size
    [SerializeField] private float minOrthographicSize = 8f;
    [SerializeField] private float maxOrthographicSize = 15f;
    
    [Header("Letterbox/Pillarbox Settings")]
    [SerializeField] private bool enableLetterboxing = true;
    [SerializeField] private Color letterboxColor = Color.black;
    
    private Camera cam;
    private float initialOrthographicSize;
    private Rect initialCameraRect;
    
    void Start()
    {
        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("CameraAspectRatioController requires a Camera component!");
            enabled = false;
            return;
        }
        
        initialOrthographicSize = cam.orthographicSize;
        initialCameraRect = cam.rect;
        
        // If baseOrthographicSize is 0, use the current camera's orthographic size
        if (baseOrthographicSize <= 0)
        {
            baseOrthographicSize = initialOrthographicSize;
        }
        
        AdjustCamera();
    }
    
    void Update()
    {
        // Check if screen size changed (useful for editor testing)
        AdjustCamera();
    }
    
    void AdjustCamera()
    {
        float currentAspectRatio = (float)Screen.width / Screen.height;
        
        // Debug logging
        Debug.Log($"CameraAspectRatioController: Screen {Screen.width}x{Screen.height}, Current AR: {currentAspectRatio:F3}, Target AR: {targetAspectRatio:F3}");
        
        if (enableLetterboxing)
        {
            AdjustCameraWithLetterboxing(currentAspectRatio);
        }
        else
        {
            AdjustOrthographicSize(currentAspectRatio);
        }
        
        // Debug current camera settings
        Debug.Log($"Camera Rect: {cam.rect}, Orthographic Size: {cam.orthographicSize}");
    }
    
    /// <summary>
    /// Adjusts camera with letterboxing/pillarboxing to maintain exact aspect ratio
    /// </summary>
    void AdjustCameraWithLetterboxing(float currentAspectRatio)
    {
        if (Mathf.Approximately(currentAspectRatio, targetAspectRatio))
        {
            // Perfect match
            cam.rect = new Rect(0, 0, 1, 1);
            cam.orthographicSize = baseOrthographicSize;
        }
        else if (currentAspectRatio > targetAspectRatio)
        {
            // Screen is wider than target - add pillarboxing (black bars on sides)
            float scaleWidth = targetAspectRatio / currentAspectRatio;
            float offsetX = (1f - scaleWidth) / 2f;
            
            cam.rect = new Rect(offsetX, 0, scaleWidth, 1);
            cam.orthographicSize = baseOrthographicSize;
        }
        else
        {
            // Screen is taller than target - add letterboxing (black bars on top/bottom)
            float scaleHeight = currentAspectRatio / targetAspectRatio;
            float offsetY = (1f - scaleHeight) / 2f;
            
            cam.rect = new Rect(0, offsetY, 1, scaleHeight);
            cam.orthographicSize = baseOrthographicSize;
        }
        
        // Set camera background color for letterbox/pillarbox areas
        cam.backgroundColor = letterboxColor;
    }
    
    /// <summary>
    /// Adjusts orthographic size to show more/less content based on aspect ratio
    /// </summary>
    void AdjustOrthographicSize(float currentAspectRatio)
    {
        cam.rect = new Rect(0, 0, 1, 1); // Full screen
        
        if (maintainHeight)
        {
            // Maintain the same visible height, adjust width by changing orthographic size
            float aspectRatioRatio = targetAspectRatio / currentAspectRatio;
            float newOrthographicSize = baseOrthographicSize * aspectRatioRatio;
            
            // Clamp to min/max values
            newOrthographicSize = Mathf.Clamp(newOrthographicSize, minOrthographicSize, maxOrthographicSize);
            cam.orthographicSize = newOrthographicSize;
        }
        else
        {
            // Maintain the same visible width, adjust height
            float aspectRatioRatio = currentAspectRatio / targetAspectRatio;
            float newOrthographicSize = baseOrthographicSize / aspectRatioRatio;
            
            // Clamp to min/max values
            newOrthographicSize = Mathf.Clamp(newOrthographicSize, minOrthographicSize, maxOrthographicSize);
            cam.orthographicSize = newOrthographicSize;
        }
    }
    
    /// <summary>
    /// Set target aspect ratio at runtime
    /// </summary>
    public void SetTargetAspectRatio(float aspectRatio)
    {
        targetAspectRatio = aspectRatio;
        AdjustCamera();
    }
    
    /// <summary>
    /// Set base orthographic size at runtime
    /// </summary>
    public void SetBaseOrthographicSize(float size)
    {
        baseOrthographicSize = size;
        AdjustCamera();
    }
    
    /// <summary>
    /// Toggle letterboxing on/off
    /// </summary>
    public void SetLetterboxing(bool enabled)
    {
        enableLetterboxing = enabled;
        AdjustCamera();
    }
    
    /// <summary>
    /// Get current screen aspect ratio
    /// </summary>
    public float GetCurrentAspectRatio()
    {
        return (float)Screen.width / Screen.height;
    }
    
    /// <summary>
    /// Check if current aspect ratio matches target
    /// </summary>
    public bool IsTargetAspectRatio()
    {
        return Mathf.Approximately(GetCurrentAspectRatio(), targetAspectRatio);
    }
    
    void OnValidate()
    {
        // Clamp values in inspector
        minOrthographicSize = Mathf.Max(0.1f, minOrthographicSize);
        maxOrthographicSize = Mathf.Max(minOrthographicSize + 0.1f, maxOrthographicSize);
        baseOrthographicSize = Mathf.Clamp(baseOrthographicSize, minOrthographicSize, maxOrthographicSize);
    }
}