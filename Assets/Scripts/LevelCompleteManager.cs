using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelCompleteManager : MonoBehaviour
{
    [Header("Level Navigation")]
    [SerializeField] private string nextLevelSceneName = "";
    [SerializeField] private string mainMenuSceneName = "MainMenu";
    [SerializeField] private string levelSelectSceneName = "LevelSelect";
    
    [Header("Performance Thresholds (seconds)")]
    [SerializeField] private float sRankTime = 15f;    // S rank - under 15 seconds
    [SerializeField] private float aRankTime = 25f;    // A rank - under 25 seconds
    [SerializeField] private float bRankTime = 40f;    // B rank - under 40 seconds
    [SerializeField] private float cRankTime = 60f;    // C rank - under 60 seconds
    // D rank - anything over C rank time
    
    [Header("Audio")]
    [SerializeField] private AudioClip levelCompleteSound;
    [SerializeField] private AudioClip buttonClickSound;
    
    // Singleton pattern
    public static LevelCompleteManager Instance { get; private set; }
    
    private AudioSource audioSource;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
                audioSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void OnLevelComplete()
    {
        // Play completion sound
        if (levelCompleteSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(levelCompleteSound);
        }
        
        Debug.Log("LevelCompleteManager: Level completed!");
    }
    
    public LevelPerformance CalculatePerformance(float completionTime)
    {
        PerformanceRank rank;
        string rankText;
        Color rankColor;
        string message;
        
        if (completionTime <= sRankTime)
        {
            rank = PerformanceRank.S;
            rankText = "S";
            rankColor = new Color(1f, 0.8f, 0f); // Gold
            message = "INCREDIBLE!";
        }
        else if (completionTime <= aRankTime)
        {
            rank = PerformanceRank.A;
            rankText = "A";
            rankColor = new Color(0.8f, 1f, 0.2f); // Lime Green
            message = "EXCELLENT!";
        }
        else if (completionTime <= bRankTime)
        {
            rank = PerformanceRank.B;
            rankText = "B";
            rankColor = new Color(0.2f, 0.8f, 1f); // Sky Blue
            message = "GREAT JOB!";
        }
        else if (completionTime <= cRankTime)
        {
            rank = PerformanceRank.C;
            rankText = "C";
            rankColor = new Color(1f, 0.6f, 0.2f); // Orange
            message = "GOOD WORK!";
        }
        else
        {
            rank = PerformanceRank.D;
            rankText = "D";
            rankColor = new Color(0.8f, 0.3f, 0.3f); // Red
            message = "KEEP TRYING!";
        }
        
        return new LevelPerformance
        {
            rank = rank,
            rankText = rankText,
            rankColor = rankColor,
            message = message,
            completionTime = completionTime
        };
    }
    
    // Button Actions
    public void RestartLevel()
    {
        PlayButtonSound();
        Debug.Log("Restarting current level...");
        
        // Reset game state
        if (GameStateManager.Instance != null)
        {
            GameStateManager.Instance.ResetGameState();
        }
        
        // Reload current scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void GoToNextLevel()
    {
        PlayButtonSound();
        
        if (!string.IsNullOrEmpty(nextLevelSceneName))
        {
            Debug.Log($"Loading next level: {nextLevelSceneName}");
            SceneManager.LoadScene(nextLevelSceneName);
        }
        else
        {
            Debug.LogWarning("Next level scene name not set! Going to level select instead.");
            GoToLevelSelect();
        }
    }
    
    public void GoToLevelSelect()
    {
        PlayButtonSound();
        Debug.Log($"Going to level select: {levelSelectSceneName}");
        SceneManager.LoadScene(levelSelectSceneName);
    }
    
    public void GoToMainMenu()
    {
        PlayButtonSound();
        Debug.Log($"Going to main menu: {mainMenuSceneName}");
        SceneManager.LoadScene(mainMenuSceneName);
    }
    
    public void ContinuePlaying()
    {
        PlayButtonSound();
        Debug.Log("Continuing to play...");
        
        // Just hide the level complete panel and continue
        if (LevelCompleteUI.Instance != null)
        {
            LevelCompleteUI.Instance.HideLevelCompletePanel();
        }
    }
    
    private void PlayButtonSound()
    {
        if (buttonClickSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(buttonClickSound);
        }
    }
    
    // Method to check if next level exists
    public bool HasNextLevel()
    {
        return !string.IsNullOrEmpty(nextLevelSceneName);
    }
    
    // Method to get level stats with performance analysis
    public EnhancedLevelStats GetEnhancedLevelStats()
    {
        if (GameStateManager.Instance == null)
        {
            return new EnhancedLevelStats
            {
                completionTime = 0f,
                performance = CalculatePerformance(0f),
                formattedTime = "00:00",
                attemptCount = 1
            };
        }
        
        LevelStats basicStats = GameStateManager.Instance.GetLevelStats();
        LevelPerformance performance = CalculatePerformance(basicStats.totalTime);
        
        // Format time as MM:SS
        int minutes = Mathf.FloorToInt(basicStats.totalTime / 60f);
        int seconds = Mathf.FloorToInt(basicStats.totalTime % 60f);
        string formattedTime = $"{minutes:00}:{seconds:00}";
        
        return new EnhancedLevelStats
        {
            completionTime = basicStats.totalTime,
            performance = performance,
            formattedTime = formattedTime,
            attemptCount = basicStats.attemptCount
        };
    }
}

// Enums and Data Structures
public enum PerformanceRank
{
    S, A, B, C, D
}

[System.Serializable]
public struct LevelPerformance
{
    public PerformanceRank rank;
    public string rankText;
    public Color rankColor;
    public string message;
    public float completionTime;
}

[System.Serializable]
public struct EnhancedLevelStats
{
    public float completionTime;
    public LevelPerformance performance;
    public string formattedTime;
    public int attemptCount;
}