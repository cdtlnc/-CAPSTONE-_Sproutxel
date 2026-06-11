using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuUI : MonoBehaviour
{
    [Header("Menus")]
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private OptionsMenuUI optionsMenu;

    [SerializeField] private string transitionName = "CrossFade";

    [SerializeField] private bool isPaused;
    [SerializeField] public string nextLevel;

    // PAUSE
    public void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0.0001f;
        isPaused = true;
    }

    // RESUME
    public void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    // MAIN MENU
    public void Home()
    {
        

        Time.timeScale = 1f;
        pauseMenu.SetActive(false);

        LevelManager.Instance.LoadScene("MainMenu", "CrossFade");
    }

    // OPTIONS
    public void OpenOptions()
    {
        pauseMenu.SetActive(false);
        optionsMenu.Open();
    }

    public void CloseOptions()
    {
        optionsMenu.Close();
        pauseMenu.SetActive(true);
    }

    // RESTART
    public void Restart()
    {
        Time.timeScale = 1f;
        pauseMenu.SetActive(false);

        string currentScene = SceneManager.GetActiveScene().name;
        LevelManager.Instance.LoadScene(currentScene, transitionName);
    }
    public void SetPaused()
    {
        isPaused = true;
    }
    public void NextLevel()
    {
        
        LevelManager.Instance.LoadScene(nextLevel, transitionName);
    }

}

