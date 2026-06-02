using System;
using UnityEngine;

public class TickManager : MonoBehaviour
{
    public class OnTickEventArgs : EventArgs
    {
        public int tick;
    }

    public static event EventHandler<OnTickEventArgs> OnTick;          // Whatever is assigned to this tick happens every tick.
    public static event EventHandler<OnTickEventArgs> OnTimeCycleTick; // This tick is when the day cycle is updated. 
    public static event EventHandler<OnTickEventArgs> OnEventTick;     // This tick is when miscellaneous events, such as bug infestations, are updated. 
    public static event EventHandler<OnTickEventArgs> OnPlantCalcTick; // This tick is when plant calculations are updated. 

    // The following stats affect when different calculations are made. Feel free to tinker with these to experiment with the pace of the gameplay.
    [Header("Tick Timers")]
    [SerializeField] public int timeCycleTickTimer; // Set to happen every two ticks by default.
    [SerializeField] public int eventTickTimer;     // Set to happen every four ticks by default.
    [SerializeField] public int plantCalcTickTimer; // Set to happen every six ticks by default.

    [Header("Tick Speed")]
    [SerializeField] private float _TICK_TIMER_MAX = 0.2f; // This decides when a tick happens. Right now it's set to 0.2,or 200ms. A whole number represents a second (i.e. 1 = 1 second).

    private int _tick;
    private float _tickTimer;

    void Awake()
    {
        _tick = 0;
    }

    // This counts the ticks that pass and triggers each tick event on the nth tick
    void Update()
    {
        _tickTimer += Time.deltaTime;

        if (_tickTimer < _TICK_TIMER_MAX)
        {
            return;
        }

        _tickTimer -= _TICK_TIMER_MAX;
        _tick++;

        if (OnTick != null) OnTick(this, new OnTickEventArgs { tick = _tick });

        if (_tick % timeCycleTickTimer == 0)
        {
            if (OnTimeCycleTick != null) OnTimeCycleTick(this, new OnTickEventArgs { tick = _tick });
        }

        if (_tick % eventTickTimer == 0)
        {
            if (OnEventTick != null) OnEventTick(this, new OnTickEventArgs { tick = _tick });
        }

        if (_tick % plantCalcTickTimer == 0)
        {
            if (OnPlantCalcTick != null) OnPlantCalcTick(this, new OnTickEventArgs { tick = _tick });
        }
    }
}
