using UnityEngine;
using UnityEngine.SceneManagement;

public class UINavigation : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string levelSelectScene = "LevelSelect";
    [SerializeField] private string level1Scene = "L1";
    [SerializeField] private string level2Scene = "L2"; 
    [SerializeField] private string level3Scene = "L3";
    [SerializeField] private string level4Scene = "L4";
    [SerializeField] private string level5Scene = "L5";

    public void OnPlayButtonPressed()
    {
        // Load first level directly, bypassing level select
        SceneManager.LoadScene(level1Scene);
    }

    public void OnExitButtonPressed() 
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuScene);
    }

    public void LoadLevel1()
    {
        SceneManager.LoadScene(level1Scene);
    }

    public void LoadLevel2()
    {
        SceneManager.LoadScene(level2Scene);
    }

    public void LoadLevel3()
    {
        SceneManager.LoadScene(level3Scene);
    }

    public void LoadLevel4()
    {
        SceneManager.LoadScene(level4Scene);
    }

    public void LoadLevel5()
    {
        SceneManager.LoadScene(level5Scene);
    }
}
