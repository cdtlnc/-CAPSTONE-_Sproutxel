using Unity.VectorGraphics;
using UnityEngine;
using static System.TimeZoneInfo;

public class MenuManager : MonoBehaviour
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject levelSelectPanel;
    [SerializeField] private GameObject multiplayerPanel;
    [SerializeField] private GameObject compendiumPanel;
    [SerializeField] private OptionsMenuUI optionsMenu;
    [SerializeField] private CompendiumViewer CompendiumMenu;


    // LEVEL SELECT
    public void OpenLevelSelect()
    {
        PlaySFX();
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
    }

    public void CloseLevelSelect()
    {
        PlaySFX();
        levelSelectPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // MULTIPLAYER
    public void OpenMultiplayer()
    {
        PlaySFX();
        LevelManager.Instance.LoadScene("Multiplayer_Level", "CrossFade");
    }

    public void CloseMultiplayer()
    {
        PlaySFX();
        multiplayerPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // OPTIONS
    public void OpenOptions()
    {
        PlaySFX();
        mainMenuPanel.SetActive(false);
        optionsMenu.Open();
    }

    public void CloseOptions()
    {
        PlaySFX();
        optionsMenu.Close();
        mainMenuPanel.SetActive(true);
    }

    public void OpenCompendium()
    {
        PlaySFX();
        mainMenuPanel.SetActive(false);
        compendiumPanel.SetActive(true);
    }

    // QUIT GAME
    public void QuitGame()
    {
        Application.Quit();
    }

    public void PlaySFX()
    {
        FindFirstObjectByType<AudioManager>().Play("Tap1");
    }
}
