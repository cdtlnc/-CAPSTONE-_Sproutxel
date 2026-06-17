using UnityEngine;
using UnityEngine.SceneManagement;
using static System.TimeZoneInfo;

public class WINLEVELMultiplayer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OpenMenu()
    {
        LevelManager.Instance.LoadScene("MainMenu", "CrossFade");
    }
    public void ResetLevel()
    {
        string currentScene = SceneManager.GetActiveScene().name;
    
        LevelManager.Instance.LoadScene(currentScene, "CrossFade");
    }
}