using UnityEngine;
using System;

public class TimeManager : MonoBehaviour
{
    public static event Action OnTick;

    private float tickTimer = 0f;
    private const float TICK_DURATION = 1f; // Fast testing: 1 second per tick

    // We will keep a local cache of all plots in the scene
    private GrowthManager[] allPlots;

    void Start()
    {
        // Automatically find every single farming plot layout in the level when the game starts
        allPlots = UnityEngine.Object.FindObjectsByType<GrowthManager>(FindObjectsSortMode.None);
        Debug.Log($"TimeManager linked up with {allPlots.Length} farming plots successfully.");
    }

    void Update()
    {
        tickTimer += Time.deltaTime;

        if (tickTimer >= TICK_DURATION)
        {
            tickTimer = 0f;

            // 1. Fire the standard event (for environment, UI, weather, etc.)
            OnTick?.Invoke();

            // 2. Direct Force-Growth fallback: Tell every plot to process the tick directly
            ForceUpdateAllCrops();

            Debug.Log("Master Tick Processed: 1 second passed.");
        }
    }

    private void ForceUpdateAllCrops()
    {
        // If we don't have our list yet, quickly search the scene layout
        if (allPlots == null || allPlots.Length == 0)
        {
            allPlots = UnityEngine.Object.FindObjectsByType<GrowthManager>(FindObjectsSortMode.None);
        }

        // Loop through every plot and run its growth mechanics manually
        foreach (GrowthManager plot in allPlots)
        {
            if (plot != null && plot.isPlanted)
            {
                // We bypass the delegate and trigger the function directly
                plot.HandleTick();
            }
        }
    }
}