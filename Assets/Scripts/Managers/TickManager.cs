using System;
using UnityEngine;
using Photon.Pun;

public class TickManager : MonoBehaviourPun
{
    public class OnTickEventArgs : EventArgs
    {
        public int tick;
    }

    public static event EventHandler<OnTickEventArgs> OnTick;
    public static event EventHandler<OnTickEventArgs> OnTimeCycleTick;
    public static event EventHandler<OnTickEventArgs> OnEventTick;
    public static event EventHandler<OnTickEventArgs> OnPlantCalcTick;

    [Header("Tick Timers")]
    [SerializeField] public int timeCycleTickTimer = 2;
    [SerializeField] public int eventTickTimer = 4;
    [SerializeField] public int plantCalcTickTimer = 6;

    [Header("Tick Speed")]
    [SerializeField] public float _TICK_TIMER_MAX = 0.2f;

    private int _tick;
    private float _tickTimer;

    void Awake()
    {
        _tick = 0;
    }

    private void Start()
    {
        TimeOfDayUI time = GameObject.FindAnyObjectByType<TimeOfDayUI>();
        if (time != null) time.GetTickSpeed(_TICK_TIMER_MAX);

        CopyCatDayNight dih = GameObject.FindAnyObjectByType<CopyCatDayNight>();
        if (dih != null) dih.GetTickSpeed(_TICK_TIMER_MAX);
    }

    void Update()
    {
        // MULTIPLAYER CHECK: Only the host controls the universal time ticker
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
        {
            return;
        }

        _tickTimer += Time.deltaTime;

        if (_tickTimer < _TICK_TIMER_MAX)
        {
            return;
        }

        _tickTimer -= _TICK_TIMER_MAX;
        _tick++;

        // Broadcast the ticks to the local Host systems
        TriggerTickEvents(_tick);

        // Send the tick over the network so Player 2 stays perfectly in sync!
        if (PhotonNetwork.IsConnected)
        {
            photonView.RPC("RPC_SyncTick", RpcTarget.Others, _tick);
        }
    }

    [PunRPC]
    private void RPC_SyncTick(int incomingTick)
    {
        _tick = incomingTick;
        TriggerTickEvents(_tick);
    }

    private void TriggerTickEvents(int currentTick)
    {
        if (OnTick != null) OnTick(this, new OnTickEventArgs { tick = currentTick });

        if (currentTick % timeCycleTickTimer == 0)
        {
            if (OnTimeCycleTick != null) OnTimeCycleTick(this, new OnTickEventArgs { tick = currentTick });
        }

        if (currentTick % eventTickTimer == 0)
        {
            if (OnEventTick != null) OnEventTick(this, new OnTickEventArgs { tick = currentTick });
        }

        if (currentTick % plantCalcTickTimer == 0)
        {
            if (OnPlantCalcTick != null) OnPlantCalcTick(this, new OnTickEventArgs { tick = currentTick });
        }
    }
}