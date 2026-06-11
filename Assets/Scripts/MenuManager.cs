using Unity.VectorGraphics;
using UnityEngine;
using static System.TimeZoneInfo;

public class MenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject levelSelectPanel;
    [SerializeField] private GameObject multiplayerPanel;
    [SerializeField] private OptionsMenuUI optionsMenu;


    // LEVEL SELECT
    public void OpenLevelSelect()
    {
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
    }

    public void CloseLevelSelect()
    {
        levelSelectPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // MULTIPLAYER
    public void OpenMultiplayer()
    {
        LevelManager.Instance.LoadScene("Multiplayer_Level", "CrossFade");
    }

    public void CloseMultiplayer()
    {
        multiplayerPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // OPTIONS
    public void OpenOptions()
    {
        mainMenuPanel.SetActive(false);
        optionsMenu.Open();
    }

    public void CloseOptions()
    {
        optionsMenu.Close();
        mainMenuPanel.SetActive(true);
    }

    // QUIT GAME
    public void QuitGame()
    {
        Application.Quit();
    }
}
