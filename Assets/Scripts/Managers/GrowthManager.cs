using UnityEngine;

public class GrowthManager : MonoBehaviour
{
    [Header("Visual Components")]
    public SpriteRenderer plantRenderer;

    [Header("Live Growth Debugger (Visible for Testing)")]
    [SerializeField] private SeedData currentSeed;
    [SerializeField] private int currentStage = 0;

    // The core connection to Lance's statistics backend simulation
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
        if (!isPlanted || currentSeed == null || plantSimulationInstance == null) return;

        // 1. Run the calculations provided by Lance
        plantSimulationInstance.GetStatsOvertime();
        plantSimulationInstance.GetSoilQuality();
        plantSimulationInstance.GetHealth();
        plantSimulationInstance.GetGrowth();
        plantSimulationInstance.GetHarvestQuality();

        // 2. Map the simulation growth value (0 to 10) to the sprite array frames
        int totalSpritesAvailable = currentSeed.growthStages.Length;
        int finalStageIndex = totalSpritesAvailable - 1;

        // Calculate stage dynamically based on crop growth percentage (0.0 to 10.0 scale)
        float growthRatio = plantSimulationInstance.cropGrowth / 10f;
        int targetStage = Mathf.FloorToInt(growthRatio * finalStageIndex);

        // Ensure we never go out of bounds of our sprite array
        currentStage = Mathf.Clamp(targetStage, 0, finalStageIndex);

        UpdatePlantSprite();
    }

    void OnMouseDown()
    {
        if (!isPlanted || currentSeed == null) return;

        int finalStageIndex = currentSeed.growthStages.Length - 1;

        // Process actual crop yield when fully matured based on simulation results
        if (currentStage == finalStageIndex)
        {
            int yieldAmount = plantSimulationInstance.GetCropYield();
            Debug.Log($"Harvested {yieldAmount} items of {currentSeed.cropName}!");

            // Using the latest modern Unity 6 lookup command directly!
            GoalManager goalManager = Object.FindFirstObjectByType<GoalManager>();

            if (goalManager != null && yieldAmount > 0)
            {
                // Adds crop quantities into the collection parameters
                for (int i = 0; i < yieldAmount; i++)
                {
                    goalManager.AddCrop(currentSeed.cropName);
                }
            }
            ResetPlot();
        }
    }

    public void PlantSeed(SeedData data)
    {
        if (isPlanted || data == null) return;

        currentSeed = data;
        isPlanted = true;
        currentStage = 0;

        // Instantiate structural class memory allocation container
        plantSimulationInstance = new BasePlant();
        plantSimulationInstance.stats = data.plantStatsTemplate;
        plantSimulationInstance.growthStages = data.growthStages;

        // Initialize basic start parameters inside structural logic containers
        if (data.plantStatsTemplate != null)
        {
            plantSimulationInstance.cropHP = data.plantStatsTemplate.maxHP;
        }
        plantSimulationInstance.cropGrowth = 0f;
        plantSimulationInstance.cropMoisture = 20f; // Safe default start setting zone
        plantSimulationInstance.soilMoisture = 20f;
        plantSimulationInstance.soilSoftness = 20f;
        plantSimulationInstance.soilQuality = 50f;

        UpdatePlantSprite();
    }

    private void UpdatePlantSprite()
    {
        if (plantRenderer != null && currentSeed != null && currentSeed.growthStages[currentStage] != null)
        {
<<<<<<< HEAD
            // Clean fallback support for both old and new Unity engine versions
#if UNITY_6_0_OR_NEWER
            GoalManager goalManager = Object.FindFirstObjectByType<GoalManager>();
#else
            GoalManager goalManager = FindFirstObjectByType<GoalManager>();
#endif

            if (goalManager != null)
            {
                goalManager.AddCrop(currentSeed.cropName);
            }
            ResetPlot();
=======
            plantRenderer.sprite = currentSeed.growthStages[currentStage];
>>>>>>> 28309f45ad5492284838237408886ca238e7dc2e
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