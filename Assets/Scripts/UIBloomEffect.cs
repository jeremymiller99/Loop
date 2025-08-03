using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Canvas))]
public class UIBloomEffect : MonoBehaviour
{
    [Header("UI Bloom Settings")]
    [Tooltip("Camera that will render the UI with post-processing")]
    public Camera uiCamera;
    
    [Tooltip("Should we create a UI camera automatically?")]
    public bool autoCreateUICamera = true;
    
    [Tooltip("Layer for UI elements to be affected by bloom")]
    public LayerMask uiBloomLayer = 1 << 5; // Default to UI layer
    
    [Header("Camera Settings")]
    [Tooltip("Depth of the UI camera (should be higher than main camera)")]
    public int cameraDepth = 10;
    
    [Tooltip("Distance from main camera to place UI camera")]
    public float cameraDistance = 1000f;
    
    private Canvas canvas;
    private Camera mainCamera;
    
    void Start()
    {
        canvas = GetComponent<Canvas>();
        mainCamera = Camera.main;
        
        if (mainCamera == null)
        {
            Debug.LogError("UIBloomEffect: No main camera found!");
            return;
        }
        
        SetupUIForBloom();
    }
    
    void SetupUIForBloom()
    {
        // Method 1: Screen Space Camera approach
        if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // Change to Screen Space Camera
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = mainCamera;
            canvas.planeDistance = 10f;
            
            Debug.Log("UIBloomEffect: Changed Canvas to Screen Space - Camera mode");
        }
        
        // Method 2: Create dedicated UI camera (if requested)
        if (autoCreateUICamera && uiCamera == null)
        {
            CreateUICamera();
        }
    }
    
    void CreateUICamera()
    {
        // Create UI Camera GameObject
        GameObject uiCameraObj = new GameObject("UI Bloom Camera");
        uiCamera = uiCameraObj.AddComponent<Camera>();
        
        // Position relative to main camera
        uiCameraObj.transform.position = mainCamera.transform.position + Vector3.forward * cameraDistance;
        uiCameraObj.transform.rotation = mainCamera.transform.rotation;
        
        // Configure UI Camera
        uiCamera.depth = cameraDepth;
        uiCamera.clearFlags = CameraClearFlags.Nothing; // Don't clear, render on top
        uiCamera.cullingMask = uiBloomLayer; // Only render UI layer
        
        // Copy post-processing from main camera
        var mainCameraData = mainCamera.GetUniversalAdditionalCameraData();
        var uiCameraData = uiCamera.GetUniversalAdditionalCameraData();
        
        if (mainCameraData != null && uiCameraData != null)
        {
            uiCameraData.renderPostProcessing = true;
            uiCameraData.antialiasing = mainCameraData.antialiasing;
            uiCameraData.antialiasingQuality = mainCameraData.antialiasingQuality;
        }
        
        // Set canvas to use this camera
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = uiCamera;
        canvas.planeDistance = 10f;
        
        Debug.Log("UIBloomEffect: Created dedicated UI camera with post-processing");
    }
    
    public void ToggleBloomEffect(bool enable)
    {
        if (enable)
        {
            SetupUIForBloom();
        }
        else
        {
            // Revert to overlay mode
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.worldCamera = null;
        }
    }
    
    public void SetUILayer(int layer)
    {
        // Change all UI objects to the specified layer
        Transform[] children = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            child.gameObject.layer = layer;
        }
        
        if (uiCamera != null)
        {
            uiCamera.cullingMask = 1 << layer;
        }
    }
    
    void OnValidate()
    {
        // Update settings in real-time during development
        if (Application.isPlaying && canvas != null)
        {
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera && canvas.worldCamera != null)
            {
                canvas.planeDistance = 10f;
            }
        }
    }
}