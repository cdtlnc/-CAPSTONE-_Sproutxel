using Unity.VectorGraphics;
using UnityEngine;
using static System.TimeZoneInfo;
using TMPro; // Required for handling the Name Input Field
using UnityEngine.UI; // Required for controlling button interactivity
using Photon.Pun; // Required for Photon network methods
using Photon.Realtime; // Required for RoomOptions

public class MenuManager : MonoBehaviourPunCallbacks // Changed to MonoBehaviourPunCallbacks for Photon events
{
    [Header("Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject levelSelectPanel;
    [SerializeField] private GameObject multiplayerPanel; // Use this for your Name/Connect overlay panel
    [SerializeField] private GameObject compendiumPanel;
    [SerializeField] private OptionsMenuUI optionsMenu;
    [SerializeField] private CompendiumViewer CompendiumMenu;

    [Header("Photon UI Extensions")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private Button joinMatchButton;
    [SerializeField] private TMP_Text networkStatusText;

    private void Start()
    {
        AudioManager.instance.Play("MainMenu");

        // Disable connection button until safely connected to Photon Master Server
        if (joinMatchButton != null) joinMatchButton.interactable = false;
        if (networkStatusText != null) networkStatusText.text = "Initializing server connection...";

        // Step 1: Establish background connection to master cloud using AppSettings
        PhotonNetwork.ConnectUsingSettings();
    }

    // PHOTON CALLBACKS
    public override void OnConnectedToMaster()
    {
        if (networkStatusText != null) networkStatusText.text = "Server Online. Ready to connect!";
        if (joinMatchButton != null) joinMatchButton.interactable = true;
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        if (networkStatusText != null) networkStatusText.text = "Creating a brand new farming lobby...";
        RoomOptions options = new RoomOptions { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(null, options);
    }

    public override void OnJoinedRoom()
    {
        if (networkStatusText != null) networkStatusText.text = "Lobby Found! Cultivating field...";

        // Handle audio changes right before leaving the menu
        AudioManager.instance.Stop("MainMenu");
        AudioManager.instance.Play("SproutxelBGMusic");

        // Hand over scene loading to Photon so all connected users travel together seamlessly
        PhotonNetwork.LoadLevel("Multiplayer_Level");
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
        PlaySFX();
        // Step 2: Instead of running away to the scene instantly, activate your connection UI panel
        mainMenuPanel.SetActive(false);
        multiplayerPanel.SetActive(true);
    }

    // Link this function directly to the OnClick() event of the 'Join Match' button inside your multiplayer panel
    public void StartNetworkMatchmaking()
    {
        PlaySFX();

        if (nameInputField != null && string.IsNullOrEmpty(nameInputField.text))
        {
            if (networkStatusText != null) networkStatusText.text = "Farmer name cannot be blank!";
            return;
        }

        // Secure name choice into Photon network instance memory
        PhotonNetwork.NickName = nameInputField.text;

        if (networkStatusText != null) networkStatusText.text = "Searching for active farming plot...";
        if (joinMatchButton != null) joinMatchButton.interactable = false;

        // Step 3: Begin searching matchmaking queues
        PhotonNetwork.JoinRandomRoom();
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