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

    private void Start()
    {
        AudioManager.instance.Play("MainMenu");
    }
    // LEVEL SELECT
    public void OpenLevelSelect()
    {
        AudioManager.instance.Stop("MainMenu");
        AudioManager.instance.Play("LevelSelectMenu");
        PlaySFX();
        mainMenuPanel.SetActive(false);
        levelSelectPanel.SetActive(true);
    }

    public void CloseLevelSelect()
    {
        AudioManager.instance.Stop("LevelSelectMenu");
        AudioManager.instance.Play("MainMenu");
        PlaySFX();
        levelSelectPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    // MULTIPLAYER
    public void OpenMultiplayer()
    {
        AudioManager.instance.Stop("MainMenu");
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
        AudioManager.instance.Play("TapSound1");
    }
}
