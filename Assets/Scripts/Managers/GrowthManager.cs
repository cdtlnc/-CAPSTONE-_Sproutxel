using UnityEngine;
using System;

public class GrowthManager : MonoBehaviour
{
    [Header("Visual Components")]
    public SpriteRenderer plantRenderer;

    [Header("Live Growth Debugger (Visible for Testing)")]
    [SerializeField] private SeedData currentSeed;
    [SerializeField] private int currentStage = 0;
    [SerializeField] private int ticksElapsed = 0;

    [HideInInspector] public bool isPlanted = false;

    void OnEnable()
    {
        // Leaves TickManager untouched, but safely hooks into its clock channel
        TickManager.OnPlantCalcTick += LinkToOfficialClock;
    }

    void OnDisable()
    {
        TickManager.OnPlantCalcTick -= LinkToOfficialClock;
    }

    // This safely catches the TickManager event and redirects it into your growth calculations
    private void LinkToOfficialClock(object sender, TickManager.OnTickEventArgs e)
    {
        HandleTick();
    }

    // Kept public and unmodified so TimeManager.cs compiles perfectly without errors!
    public void HandleTick()
    {
        if (!isPlanted || currentSeed == null) return;

        if (currentStage < currentSeed.growthStages.Length - 1)
        {
            ticksElapsed++;
            if (ticksElapsed >= currentSeed.ticksPerStage)
            {
                ticksElapsed = 0;
                currentStage++;

                if (plantRenderer != null && currentSeed.growthStages[currentStage] != null)
                {
                    plantRenderer.sprite = currentSeed.growthStages[currentStage];
                }
            }
        }
    }

    public void PlantSeed(SeedData data)
    {
        if (isPlanted) return;
        currentSeed = data;
        isPlanted = true;
        currentStage = 0;
        ticksElapsed = 0;

        if (plantRenderer != null && currentSeed != null && currentSeed.growthStages.Length > 0)
        {
            plantRenderer.sprite = currentSeed.growthStages[0];
        }
    }

    void OnMouseDown()
    {
        if (isPlanted && currentSeed != null && currentStage == currentSeed.growthStages.Length - 1)
        {
            // Clean fallback support for both old and new Unity engine versions
#if UNITY_6_0_OR_NEWER
            GoalManager goalManager = Object.FindFirstObjectByType<GoalManager>();
#else
            GoalManager goalManager = FindObjectOfType<GoalManager>();
#endif

            if (goalManager != null)
            {
                goalManager.AddCrop(currentSeed.cropName);
            }
            ResetPlot();
        }
    }

    void ResetPlot()
    {
        isPlanted = false;
        currentSeed = null;
        currentStage = 0;
        ticksElapsed = 0;
        if (plantRenderer != null)
        {
            plantRenderer.sprite = null;
        }
    }
}