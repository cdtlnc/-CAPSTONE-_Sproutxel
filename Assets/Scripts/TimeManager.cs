using UnityEngine;
using System;

public class TimeManager : MonoBehaviour
{
    public static event Action OnTick; // This is the pulse plants listen to

    private float tickTimer = 0f;
    private const float TICK_DURATION = 20f; // 1 tick = 20 seconds

    void Update()
    {
        tickTimer += Time.deltaTime;

        if (tickTimer >= TICK_DURATION)
        {
            tickTimer = 0f;
            // Logical flow: Process environment/weather, then growth
            OnTick?.Invoke();
            Debug.Log("Tick Processed: 20 seconds passed.");
        }
    }
}