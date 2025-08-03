using UnityEngine;

public class ResolutionManager : MonoBehaviour
{
    [Header("Resolution Settings")]
    [SerializeField] private int targetWidth = 960;  // Half of 1920
    [SerializeField] private int targetHeight = 640; // Half of 1280
    [SerializeField] private bool fullscreen = false;
    [SerializeField] private bool setOnStart = true;
    
    void Start()
    {
        if (setOnStart)
        {
            SetHalfResolution();
        }
    }
    
    void Update()
    {
        // Allow runtime resolution changes with keyboard shortcuts
        if (Input.GetKeyDown(KeyCode.F1))
        {
            SetHalfResolution();
        }
        else if (Input.GetKeyDown(KeyCode.F2))
        {
            SetFullResolution();
        }
        else if (Input.GetKeyDown(KeyCode.F11))
        {
            ToggleFullscreen();
        }
    }
    
    public void SetHalfResolution()
    {
        Screen.SetResolution(targetWidth, targetHeight, fullscreen);
        Debug.Log($"Resolution set to {targetWidth}x{targetHeight} (Half size)");
    }
    
    public void SetFullResolution()
    {
        Screen.SetResolution(targetWidth * 2, targetHeight * 2, fullscreen);
        Debug.Log($"Resolution set to {targetWidth * 2}x{targetHeight * 2} (Full size)");
    }
    
    public void ToggleFullscreen()
    {
        fullscreen = !fullscreen;
        Screen.SetResolution(Screen.width, Screen.height, fullscreen);
        Debug.Log($"Fullscreen toggled: {fullscreen}");
    }
    
    public void SetCustomResolution(int width, int height)
    {
        Screen.SetResolution(width, height, fullscreen);
        Debug.Log($"Resolution set to {width}x{height}");
    }
}