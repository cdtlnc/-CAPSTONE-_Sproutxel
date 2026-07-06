using UnityEngine;
using Photon.Pun;

public class MultiplayerSceneSetup : MonoBehaviourPunCallbacks
{
    public static MultiplayerSceneSetup Instance { get; private set; }

    public static bool IsHost => PhotonNetwork.IsMasterClient;
    public static string LocalPlayerName => IsHost ? "Farmer1" : "Farmer2";
    public static string RemotePlayerName => IsHost ? "Farmer2" : "Farmer1";

    [Header("Local player objects")]
    [SerializeField] public Camera localCamera;
    [SerializeField] public Canvas localCanvas;
    [SerializeField] public FarmerHealth localHealth;
    [SerializeField] public CanonFire localCanon;
    [SerializeField] public GoalManager_Multiplayer goalManager;

    [Header("Remote player ghost view")]
    [SerializeField] public GhostFarmView ghostFarmView;

    [Header("Time/weather GameObjects")]
    [SerializeField] public GameObject tickManagerObj;
    [SerializeField] public GameObject eventManagerObj;   // host only
    [SerializeField] public GameObject timeOfDayObj;

    private void Awake() { Instance = this; }

    private void Start()
    {
        bool isHost = PhotonNetwork.IsMasterClient;

        tickManagerObj.SetActive(true);
        timeOfDayObj.SetActive(true);        // always active, client mirrors from network
        eventManagerObj.SetActive(isHost);   // only host runs weather simulation

        if (LevelManager.Instance != null)
            LevelManager.Instance.HideProgressBar();

        Debug.Log($"[SceneSetup] This device is: {LocalPlayerName} | IsHost: {isHost}");
    }

    public override void OnPlayerLeftRoom(Photon.Realtime.Player otherPlayer)
    {
        Debug.Log("[SceneSetup] Opponent disconnected.");
        StartCoroutine(HandleOpponentDisconnect());
    }

    private System.Collections.IEnumerator HandleOpponentDisconnect()
    {
        yield return new WaitForSeconds(2f);
        PhotonNetwork.LeaveRoom();
        LevelManager.Instance.LoadScene("MainMenu", "CrossFade");
    }
}