using UnityEngine;

public class GrowthManager : MonoBehaviour
{
    [Header("Visual Components")]
    public SpriteRenderer plantRenderer;

    [Header("Live Growth Debugger (Visible for Testing)")]
    [SerializeField] private SeedData currentSeed;
    [SerializeField] private int currentStage = 0;

    // Connects directly to Lance's math backend script
    public BasePlant plantSimulationInstance;

    [HideInInspector] public bool isPlanted = false;

    void OnEnable() { TickManager.OnPlantCalcTick += LinkToOfficialClock; }
    void OnDisable() { TickManager.OnPlantCalcTick -= LinkToOfficialClock; }

    private void LinkToOfficialClock(object sender, TickManager.OnTickEventArgs e)
    {
        HandleTick();
    }

    public void HandleTick()
    {
        // Don't do calculations if nothing is planted yet
        if (!isPlanted || currentSeed == null || plantSimulationInstance == null) return;

        // SAFE PASS: If your team hasn't assigned a Plant Stats Template yet, fake the growth so it doesn't crash!
        if (plantSimulationInstance.stats == null)
        {
            // Just manually tick up the growth number by 1.0f every simulation tick so we can test sprites
            plantSimulationInstance.cropGrowth += 1.0f;
            plantSimulationInstance.cropGrowth = Mathf.Clamp(plantSimulationInstance.cropGrowth, 0f, 10f);
        }
        else
        {
            // 1. Run Lance's formulas normally if data template actually exists!
            plantSimulationInstance.GetStatsOvertime();

            // (Soil quality line skipped since it's not implemented yet anyway)
            plantSimulationInstance.soilQuality = 50f;

            plantSimulationInstance.GetHealth();
            plantSimulationInstance.GetGrowth();
            plantSimulationInstance.GetHarvestQuality();
        }

        // 2. Math to map the 0-10 growth value to our sprite array frames
        if (currentSeed.growthStages == null || currentSeed.growthStages.Length == 0) return;

        int totalSpritesAvailable = currentSeed.growthStages.Length;
        int finalStageIndex = totalSpritesAvailable - 1;

        // If there's only 1 sprite total, index is always 0. Otherwise, do the math.
        if (finalStageIndex > 0)
        {
            // Convert 0.0 - 10.0 scale to a percentage for the current sprite index
            float growthRatio = plantSimulationInstance.cropGrowth / 10f;
            int targetStage = Mathf.FloorToInt(growthRatio * finalStageIndex);

            // Keep it safe so it never goes out of bounds of the array
            currentStage = Mathf.Clamp(targetStage, 0, finalStageIndex);
        }
        else
        {
            currentStage = 0;
        }

        UpdatePlantSprite();
    }

    // Tapping/clicking the plot to harvest
    void OnMouseDown()
    {
        // SAFETY: Ignore clicks if the plot is empty or data isn't set up yet
        if (!isPlanted || currentSeed == null || currentSeed.growthStages == null || currentSeed.growthStages.Length == 0) return;

        int finalStageIndex = currentSeed.growthStages.Length - 1;

        // Only harvest if it's completely fully grown (at the last sprite stage)
        if (currentStage == finalStageIndex)
        {
            int yieldAmount = plantSimulationInstance.GetCropYield();
            Debug.Log($"Harvested {yieldAmount} items of {currentSeed.cropName}!");

            // Grab the GoalManager directly using the modern Unity 6 command
            GoalManager goalManager = Object.FindFirstObjectByType<GoalManager>();

            if (goalManager != null && yieldAmount > 0)
            {
                // Loop to add the correct amount of items to the level objectives
                for (int i = 0; i < yieldAmount; i++)
                {
                    goalManager.AddCrop(currentSeed.cropName);
                }
            }
            ResetPlot();
        }
    }

    // Called explicitly by our seed item/drag system to start planting
    public void PlantSeed(SeedData data)
    {
        if (isPlanted || data == null) return;

        // Catch-all if someone forgot to put even a single sprite in the asset file
        if (data.growthStages == null || data.growthStages.Length == 0)
        {
            Debug.LogError($"[GrowthManager] Can't plant {data.cropName}! Your SeedData needs at least 1 sprite in the array.");
            return;
        }

        currentSeed = data;
        isPlanted = true;
        currentStage = 0;

        // Initialize a new data instance using Lance's script setup
        plantSimulationInstance = new BasePlant();
        plantSimulationInstance.stats = data.plantStatsTemplate;
        plantSimulationInstance.growthStages = data.growthStages;

        // Set up the starting values for the soil and plant parameters
        if (data.plantStatsTemplate != null)
        {
            plantSimulationInstance.cropHP = data.plantStatsTemplate.maxHP;
        }
        plantSimulationInstance.cropGrowth = 0f;
        plantSimulationInstance.cropMoisture = 20f;
        plantSimulationInstance.soilMoisture = 20f;
        plantSimulationInstance.soilSoftness = 20f;
        plantSimulationInstance.soilQuality = 50f;

        UpdatePlantSprite();
        Debug.Log($"Successfully planted {data.cropName}! Calculations started.");
    }

    private void UpdatePlantSprite()
    {
        if (plantRenderer != null && currentSeed != null && currentSeed.growthStages != null && currentStage < currentSeed.growthStages.Length)
        {
            // Only swap the sprite if the slot isn't empty, otherwise keep whatever it's currently showing
            if (currentSeed.growthStages[currentStage] != null)
            {
                plantRenderer.sprite = currentSeed.growthStages[currentStage];
            }
        }
    }

    void ResetPlot()
    {
        isPlanted = false;
        currentSeed = null;
        plantSimulationInstance = null;
        currentStage = 0;
        if (plantRenderer != null) plantRenderer.sprite = null;
    }
}