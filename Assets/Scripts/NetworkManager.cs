using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    [Header("Scene to load when match starts")]
    [SerializeField] private string gameSceneName = "MultiplayerGame";

    [Header("Panels")]
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject multiplayerPanel;
    [SerializeField] private GameObject waitingPanel;

    [Header("Multiplayer panel UI")]
    [SerializeField] private TMP_InputField roomCodeInput;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button joinRoomButton;
    [SerializeField] private TMP_Text statusText;

    [Header("Waiting panel UI")]
    [SerializeField] private TMP_Text waitingText;
    [SerializeField] private Button cancelButton;

    private const int MAX_PLAYERS = 2;

    private void Start()
    {
        PhotonNetwork.NickName = "Farmer";
        PhotonNetwork.AutomaticallySyncScene = true;

        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (multiplayerPanel != null) multiplayerPanel.SetActive(false);
        if (waitingPanel != null) waitingPanel.SetActive(false);

        SetStatus("Connecting to server...");
        DisableButtons();
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "asia";
        PhotonNetwork.ConnectUsingSettings();
    }

    // PANEL NAVIGATIONS

    public void OpenMultiplayerPanel()
    {
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (multiplayerPanel != null) multiplayerPanel.SetActive(true);
        if (waitingPanel != null) waitingPanel.SetActive(false);
    }

    public void CloseMultiplayerPanel()
    {
        if (multiplayerPanel != null) multiplayerPanel.SetActive(false);
        if (waitingPanel != null) waitingPanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
    }

    // ROOM ACTIONS

    public void OnCreateRoomClicked()
    {
        string code = GetRoomCode();
        if (code.Length == 0) { SetStatus("Enter a room code first."); return; }

        SetStatus($"Creating room '{code}'...");
        DisableButtons();

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = MAX_PLAYERS,
            IsVisible = false,
            IsOpen = true
        };

        PhotonNetwork.CreateRoom(code, options);
    }

    public void OnJoinRoomClicked()
    {
        string code = GetRoomCode();
        if (code.Length == 0) { SetStatus("Enter a room code first."); return; }

        SetStatus($"Joining room '{code}'...");
        DisableButtons();
        PhotonNetwork.JoinRoom(code);
    }

    public void OnCancelClicked()
    {
        PhotonNetwork.LeaveRoom();
        if (waitingPanel != null) waitingPanel.SetActive(false);
        if (multiplayerPanel != null) multiplayerPanel.SetActive(true);
        SetStatus("Cancelled.");
        EnableButtons();
    }

    // PHOTON CALLBACKS

    public override void OnConnectedToMaster()
    {
        Debug.Log("[NetworkManager] Connected.");
        SetStatus("Connected. Enter a room code.");
        EnableButtons();
        PhotonNetwork.JoinLobby();
    }

    public override void OnCreatedRoom()
    {
        string code = PhotonNetwork.CurrentRoom.Name;
        if (multiplayerPanel != null) multiplayerPanel.SetActive(false);
        if (waitingPanel != null) waitingPanel.SetActive(true);
        if (waitingText != null)
            waitingText.text = $"Room created!\nShare this code:\n\n<size=48><b>{code}</b></size>\n\nWaiting for opponent...";
    }

    public override void OnJoinedRoom()
    {
        string code = PhotonNetwork.CurrentRoom.Name;
        int count = PhotonNetwork.CurrentRoom.PlayerCount;

        if (multiplayerPanel != null) multiplayerPanel.SetActive(false);
        if (waitingPanel != null) waitingPanel.SetActive(true);
        if (waitingText != null)
            waitingText.text = $"Joined room '{code}'.\nWaiting for host to start...";

        if (count >= MAX_PLAYERS) TryStartGame();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        int count = PhotonNetwork.CurrentRoom.PlayerCount;
        if (waitingText != null) waitingText.text = "Opponent connected!\nStarting game...";
        if (count >= MAX_PLAYERS) TryStartGame();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        SetStatus($"Room '{GetRoomCode()}' already exists. Try joining instead.");
        EnableButtons();
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        SetStatus("Room not found. Check the code and try again.");
        EnableButtons();
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        SetStatus($"Disconnected: {cause}. Reconnecting...");
        DisableButtons();
        PhotonNetwork.ConnectUsingSettings();
    }

    // GAME START

    private void TryStartGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        PhotonNetwork.CurrentRoom.IsOpen = false;
        PhotonNetwork.LoadLevel(gameSceneName);
    }

    // HELPERS

    private string GetRoomCode()
    {
        if (roomCodeInput == null) return "";
        return roomCodeInput.text.Trim().ToUpper();
    }

    private void SetStatus(string msg)
    {
        if (statusText != null) statusText.text = msg;
    }

    private void EnableButtons()
    {
        if (createRoomButton != null) createRoomButton.interactable = true;
        if (joinRoomButton != null) joinRoomButton.interactable = true;
    }

    private void DisableButtons()
    {
        if (createRoomButton != null) createRoomButton.interactable = false;
        if (joinRoomButton != null) joinRoomButton.interactable = false;
    }
}