// broadcasts currentTime (float, minutes 0-1440) alongside
// the existing state so TimeOfDayUI.SetFromNetwork() can show the exact
// in-game time on the client instead of a rough day/night estimate.
// NetworkTimeState also stores currentTime for other scripts to read.

using UnityEngine;
using Photon.Pun;
using ExitGames.Client.Photon;
using Photon.Realtime;

public class NetworkTimeSync : MonoBehaviourPunCallbacks
{
    private const byte TIME_SYNC_EVENT = 1;

    private TimeOfDayUI _timeOfDayUI;

    private void Start()
    {
        _timeOfDayUI = FindFirstObjectByType<TimeOfDayUI>();
    }

    private void OnEnable()
    {
        PhotonNetwork.NetworkingClient.EventReceived += OnPhotonEvent;
        TickManager.OnTimeCycleTick += OnTimeCycleTick;
    }

    private void OnDisable()
    {
        PhotonNetwork.NetworkingClient.EventReceived -= OnPhotonEvent;
        TickManager.OnTimeCycleTick -= OnTimeCycleTick;
    }

    // only the host sends time sync events
    private void OnTimeCycleTick(object sender, TickManager.OnTickEventArgs e)
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (_timeOfDayUI == null)
            _timeOfDayUI = FindFirstObjectByType<TimeOfDayUI>();

        object[] data = new object[]
        {
            TimeOfDayUI.isDrySeason,          // bool
            TimeOfDayUI.isDay,                // bool
            (int)EventManager._weatherEvent,  // int
            EventManager.isInfested,          // bool
            _timeOfDayUI != null              // float — exact in-game time in minutes
                ? _timeOfDayUI.GetCurrentTime()
                : 0f
        };

        RaiseEventOptions options = new RaiseEventOptions
        {
            Receivers = ReceiverGroup.Others
        };

        PhotonNetwork.RaiseEvent(TIME_SYNC_EVENT, data, options, SendOptions.SendReliable);
    }

    // both devices receive events
    // client applies them, host ignores its own
    private void OnPhotonEvent(EventData photonEvent)
    {
        if (photonEvent.Code != TIME_SYNC_EVENT) return;
        if (PhotonNetwork.IsMasterClient) return;

        object[] data = (object[])photonEvent.CustomData;

        bool isDrySeason = (bool)data[0];
        bool isDay = (bool)data[1];
        int weatherEvent = (int)data[2];
        bool isInfested = (bool)data[3];
        float currentTime = (float)data[4];

        // apply to static state holder
        NetworkTimeState.Apply(isDrySeason, isDay, weatherEvent, isInfested, currentTime);

        // push directly into TimeOfDayUI so the clock and light update on the client
        if (_timeOfDayUI == null)
            _timeOfDayUI = FindFirstObjectByType<TimeOfDayUI>();

        if (_timeOfDayUI != null)
            _timeOfDayUI.SetFromNetwork(isDrySeason, isDay, currentTime);
    }
}

// static holder read by RainScript, GrowthManager_Multiplayer, etc. on client
public static class NetworkTimeState
{
    public static bool isDrySeason { get; private set; } = true;
    public static bool isDay { get; private set; } = true;
    public static int weatherEvent { get; private set; } = 0;
    public static bool isInfested { get; private set; } = false;
    public static float currentTime { get; private set; } = 300f; // 5am default

    public static void Apply(bool dry, bool day, int weather, bool bugs, float time)
    {
        isDrySeason = dry;
        isDay = day;
        weatherEvent = weather;
        isInfested = bugs;
        currentTime = time;
    }
}