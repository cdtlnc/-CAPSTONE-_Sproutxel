#if UNITY_EDITOR
using Unity.Android.Gradle.Manifest;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEditor.EditorTools;

#endif
using UnityEngine.EventSystems;
using UnityEngine;

public class GrowthManager : MonoBehaviour, IPointerClickHandler
{
    [Header("Visual Components")]
    [SerializeField] public SpriteRenderer plantRenderer;
    [SerializeField] public SpriteRenderer PlantSadBG;
    [SerializeField] public SpriteRenderer PlantSadFG;
    [SerializeField] public Sprite PlantSadBGTexture;
    [SerializeField] public Sprite PlantSadFGTexture;

    [Header("Live Growth Debugger (Visible for Testing)")]
    [SerializeField] private SeedData currentSeed;
    [SerializeField] private int currentStage = 0;

    [Header("Plant Stats")]
    [SerializeField] private string name;
    [SerializeField] private float cropHP;
    [SerializeField] private bool wonMinigame, hasNoBadStats;

    [Header("Crop Moisture")]
    [SerializeField] private float cropMoisture;

    [Header("Soil Quality")]
    [SerializeField] private float soilQuality;

    [Header("Soil Moisture")]
    [SerializeField] private float soilMoisture;

    [Header("Soil Softness")]
    [SerializeField] private float soilSoftness;

    [Header("Crop Mechanics")]
    [SerializeField] private bool Waterlogged;
    [SerializeField] private float WaterloggedMeter;
    [SerializeField] private float WaterCooldown;
    [SerializeField] private float WaterloggedMax;
    [SerializeField] private float WaterFillUpRate;
    [SerializeField] private GameObject Water;

    [Header("Seed Class")]
    [SerializeField] string seasonOutput;
    [SerializeField] string cycleOutput;
    [SerializeField] string weatherOutput;
    [SerializeField] string bugInfestation;

    [SerializeField] int seasonIndex;
    [SerializeField] int cycleIndex;
    [SerializeField] int weatherIndex;
    [SerializeField] int bugIndex;

    [Header("Item Checker")]
    [SerializeField] private bool isplantable;

    // Direct link to the math backend script
    public BasePlant plantSimulationInstance;

    [HideInInspector] public bool isPlanted = false;

    private void Start()
    {
        Waterlogged = false;
        Water.SetActive(false);
        disableSadParts();
    }

    void OnEnable()
    {
        TickManager.OnPlantCalcTick += LinkToOfficialClock;
    }

    void OnDisable()
    {
        TickManager.OnPlantCalcTick -= LinkToOfficialClock;
    }

    private void LinkToOfficialClock(object sender, TickManager.OnTickEventArgs e)
    {
        HandleTick();
    }

    public void HandleTick()
    {
        if (!isPlanted)
            IsWaterLogged();

        // Don't calculate stuff if the plot is completely empty
        if (!isPlanted || currentSeed == null || plantSimulationInstance == null) return;

        // SAFE PASS: If I haven't assigned a data template yet, fake the growth so it doesn't crash!
        if (plantSimulationInstance.stats == null)
        {
            // Just manually step up the growth value by 1.0f every tick so I can test the sprites
            plantSimulationInstance.cropGrowth += 1.0f;
            plantSimulationInstance.cropGrowth = Mathf.Clamp(plantSimulationInstance.cropGrowth, 0f, 10f);
        }
        else
        {
            // Run the formula scripts normally if the template asset exists
            plantSimulationInstance.GetStatsOvertime();

            // Skipping the soil quality calculation for now since it isn't built yet anyway
            plantSimulationInstance.GetSoilQuality();

            plantSimulationInstance.GetHealth();
            plantSimulationInstance.GetGrowth();
            plantSimulationInstance.GetHarvestQuality();
        }

        // Map the 0-10 growth value to my sprite array frames
        if (currentSeed.growthStages == null || currentSeed.growthStages.Length == 0) return;

        int totalSpritesAvailable = currentSeed.growthStages.Length;
        int finalStageIndex = totalSpritesAvailable - 1;

        // If I only put 1 sprite total, index is always 0. Otherwise, do the math.
        if (finalStageIndex > 0)
        {
            // Convert 0.0 - 10.0 scale to a percentage for the current sprite index
            float growthRatio = plantSimulationInstance.cropGrowth / 10f;
            int targetStage = Mathf.FloorToInt(growthRatio * finalStageIndex);

            // Keep it safe so it never breaks or goes out of bounds
            currentStage = Mathf.Clamp(targetStage, 0, finalStageIndex);
        }
        else
        {
            currentStage = 0;
        }

        CheckWeather();
        CheckSeason();
        CheckDay();
        CheckInfestation();
        CheckStats();

        // LINE ADDED HERE: Run real-time condition evaluation checks
        EvaluatePlotHealth();

        GetMogged();
        UpdatePlantSprite();
        Debug.Log("Coming up on Waterlogged");

        Debug.Log("Passed Waterlogged");
        Debug.Log("Pickle  " + plantSimulationInstance.stats.seasonalAffinities[seasonIndex] + " " + plantSimulationInstance.stats.weatherAffinities[weatherIndex] + " " + plantSimulationInstance.stats.cycleAffinities[cycleIndex]);
    }

    // Tapping/clicking the plot to harvest
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isPlanted || currentSeed == null || currentSeed.growthStages == null || currentSeed.growthStages.Length == 0) return;

        int lastConfiguredStageIndex = currentSeed.growthStages.Length - 1;
        int MatureStageIndex = currentSeed.growthStages.Length - 2;

        if (currentStage == MatureStageIndex || currentStage == lastConfiguredStageIndex)
        {
            int yieldAmount = 0;

            if (plantSimulationInstance.stats == null)
            {
                yieldAmount = 3;
                Debug.Log($"[Bypass] Missing stats template! Dropping a fallback default of {yieldAmount} items.");
            }
            else
            {
                yieldAmount = plantSimulationInstance.GetCropYield();
                Debug.Log($"Harvested {yieldAmount} items of {currentSeed.cropName}!");
            }

            GoalManager goalManager = Object.FindFirstObjectByType<GoalManager>();

            if (goalManager != null && yieldAmount > 0)
            {
                // CASE 1: The plant is perfectly healthy (No Bad Stats)
                if (hasNoBadStats)
                {
                    // Directly pass the total yieldAmount ONCE, no loops needed
                    goalManager.AddCrop(currentSeed.cropName, yieldAmount);
                    IsNotPlantable();
                    ResetPlot(); // Cleanly clear the plot out instantly
                }
                // CASE 2: The plant has issues and needs maintenance pop-up window
                else
                {
                    MaintenencePopUp ui = Object.FindFirstObjectByType<MaintenencePopUp>();
                    if (ui != null)
                    {
                        ui.OpenWindow(this); // Opens the window exactly ONCE
                    }
                }
            }
        }
    }

    // Called by my drag and drop system to start planting
    public void PlantSeed(SeedData data)
    {
        if (isPlanted || data == null) return;
        if (Waterlogged || !isplantable) return;
        if (data.remainingSeedBags <= 0)
        {
            Debug.LogWarning($"[Out of Seeds] Can't plant anymore {data.cropName}! 0 bags remaining.");
            return;
        }

        // Safety check in case I forgot to add a sprite to the asset file
        if (data.growthStages == null || data.growthStages.Length == 0)
        {
            Debug.LogError($"[GrowthManager] Can't plant {data.cropName}! The SeedData needs at least 1 sprite in the array.");
            return;
        }

        currentSeed = data;
        isPlanted = true;
        currentStage = 0;
        FindFirstObjectByType<AudioManager>().Play("Planting");

        // Fire up a brand new simulation instance
        plantSimulationInstance = new BasePlant();
        plantSimulationInstance.stats = data.plantStatsTemplate;
        plantSimulationInstance.growthStages = data.growthStages;

        // Set up my starting values for the parameters
        if (data.plantStatsTemplate != null)
        {
            plantSimulationInstance.cropHP = data.plantStatsTemplate.maxHP;
        }
        plantSimulationInstance.cropGrowth = 0f;
        plantSimulationInstance.cropMoisture = 20f;
        plantSimulationInstance.soilMoisture = 20f;
        plantSimulationInstance.soilSoftness = 20f;
        plantSimulationInstance.soilQuality = 20f; 

        data.remainingSeedBags--;

        UpdatePlantSprite();
        Debug.Log($"Successfully planted {data.cropName}! Calculations started.");
    }

    private void UpdatePlantSprite()
    {
        Debug.Log("Entered Plant Sprite Update");
        if (plantRenderer != null && currentSeed != null && currentSeed.growthStages != null && currentStage < currentSeed.growthStages.Length)
        {
            Debug.Log("Entered Plant Sprite Update 1st PHASE");
            // Only swap the sprite if the slot isn't empty, otherwise keep whatever it's showing
            if (currentSeed.growthStages[currentStage] != null)
            {
                Debug.Log("Entered Plant Sprite Update 2nd PHASE");
                plantRenderer.sprite = currentSeed.growthStages[currentStage];
            }
        }
    }

    //User Shovel
    void ResetPlot()
    {
        isPlanted = false;
        currentSeed = null;
        plantSimulationInstance = null;
        currentStage = 0;
        if (plantRenderer != null) plantRenderer.sprite = null;
        disableSadParts();
        IsNotPlantable();
    }

    private void FixedUpdate()
    {
        // SAFETY SHIELD: Stops the code from running and crashing if the plot is empty!
        if (!isPlanted || currentSeed == null || plantSimulationInstance == null)
        {
            name = "Empty Plot";
            cropHP = 0f;
            cropMoisture = 0f;
            soilQuality = 0f;
            soilMoisture = 0f;
            soilSoftness = 0f;
            return; // Exits the function early
        }

        // This updates the inspector display LIVE when values change from watering
        name = currentSeed.cropName;
        cropHP = plantSimulationInstance.cropHP;
        cropMoisture = plantSimulationInstance.cropMoisture;
        soilQuality = plantSimulationInstance.soilQuality;
        soilMoisture = plantSimulationInstance.soilMoisture;
        soilSoftness = plantSimulationInstance.soilSoftness;
    }

    public void winMinigame()
    {
        FindFirstObjectByType<AudioManager>().Play("WinMinigame");
        SuperCharge();
        IsNotPlantable(); //Used to make sure the soil tiller is used
        ResetPlot();
    }

    public void LoseMinigame()
    {
        FindFirstObjectByType<AudioManager>().Play("LoseMinigame");
        GoalManager goalManager = Object.FindFirstObjectByType<GoalManager>();
        int yield = plantSimulationInstance.GetCropYield();
        goalManager.AddCrop(currentSeed.cropName, yield / 2);
        IsNotPlantable(); //Used to make sure the soil tiller is used
        ResetPlot();
    }

    // --- LINKED MINIGAME STAT RESOLUTION ROUTINES ---
    public void ResolveMinigameWin(MinigameType type)
    {
        Debug.Log($"Minigame {type} WON. Correcting stats on plot!");

        if (plantSimulationInstance == null) return;

        switch (type)
        {
            case MinigameType.Watering:
                plantSimulationInstance.soilMoisture = 5.0f;
                plantSimulationInstance.cropMoisture = 5.0f;
                WaterClear(); // Reuses your existing function to clear graphics and meters
                break;

            case MinigameType.Weeding:
                plantSimulationInstance.soilQuality = Mathf.Min(plantSimulationInstance.soilQuality + 2.0f, 10f);
                break;

            case MinigameType.PestControl:
                bugInfestation = "no bugs:(";
                bugIndex = 0;
                plantSimulationInstance.bugIndex = bugIndex;
                break;

            case MinigameType.SoilEnrichment:
                plantSimulationInstance.soilQuality = 10.0f;
                break;

            case MinigameType.StructuralSupport:
                plantSimulationInstance.soilSoftness = 5.0f;
                break;

            case MinigameType.Netting:
                // SAFE PASS: When the player nails the anchor placements, award +2.0f back to the crop's health!
                // Using Mathf.Min with 10f ensures I don't break the game logic by accidentally overflowing past max HP.
                plantSimulationInstance.cropHP = Mathf.Min(plantSimulationInstance.cropHP + 2.0f, 10f);
                break;
        }
        SuperCharge();
        CheckStats();
        UpdatePlantSprite();
    }

    public void ResolveMinigameLose(MinigameType type)
    {
        Debug.Log($"Minigame {type} FAILED. Penalizing crop stats.");

        if (plantSimulationInstance == null) return;

        switch (type)
        {
            case MinigameType.Watering:
                plantSimulationInstance.cropHP -= 2.0f;
                break;
            case MinigameType.PestControl:
                plantSimulationInstance.cropHP -= 3.0f;
                break;
            case MinigameType.Netting:
                plantSimulationInstance.cropHP -= 1.0f; // Custom damage deduction if they mess up netting loops
                break;
            default:
                plantSimulationInstance.cropHP -= 1.0f;
                break;
        }

        CheckStats();
    }

    private void CheckStats()
    {
        // Safety check to prevent crashes if nothing is planted yet
        if (!isPlanted || currentSeed == null || plantSimulationInstance == null)
        {
            hasNoBadStats = false;
            return;
        }

        // 1. Sync the display variables from the simulation backend
        cropMoisture = plantSimulationInstance.cropMoisture;
        soilQuality = plantSimulationInstance.soilQuality;
        soilMoisture = plantSimulationInstance.soilMoisture;
        soilSoftness = plantSimulationInstance.soilSoftness;

        // 2. Set the threshold limits (80% of 100 is 80)
        float maxLimit = 80f;
        float minLimit = -80f;

        // 3. Check if ANY of the tracked stats have gone outside the safe -80 to 80 range
        if (
            cropMoisture < minLimit || cropMoisture > maxLimit ||
            soilQuality < minLimit || soilQuality > maxLimit ||
            soilMoisture < minLimit || soilMoisture > maxLimit ||
            soilSoftness < minLimit || soilSoftness > maxLimit)
        {
            // At least one stat is bad (outside the 80% range)
            hasNoBadStats = false;
            PlantSadFG.color = new Color(1f, 1f, 1f, 1f);
            PlantSadBG.color = new Color(1f, 1f, 1f, 1f);
        }
        else
        {
            // All stats are safely within the -80 to 80 range!
            hasNoBadStats = true;
        }
    }

    /// <summary>
    /// Evaluates structural crop health conditions and handles targeted degradation logic.
    /// </summary>
    public void EvaluatePlotHealth()
    {
        if (plantSimulationInstance == null) return;

        // Check discrete plot parameters
        bool isThirsty = plantSimulationInstance.soilMoisture < 2.0f;
        bool isDrowning = Waterlogged;
        bool isInfested = bugInfestation == "INFESTEDDD";

        // Apply health breakdown modifications if environments are compromised
        if (isThirsty || isDrowning || isInfested)
        {
            hasNoBadStats = false;
            plantSimulationInstance.cropHP -= 0.5f; // Apply active tick damage penalty
        }
    }

    private void disableSadParts()
    {
        PlantSadBG.color = new Color(1f, 1f, 1f, 0f);
        PlantSadFG.color = new Color(1f, 1f, 1f, 0f);
    }

    public void MoistureCleanse()
    {

    }

    public void CheckInfestation()
    {
        if (EventManager.isInfested)
        {
            bugInfestation = "INFESTEDDD";
            bugIndex = 1;
            plantSimulationInstance.bugIndex = bugIndex;
        }
        else
        {
            bugInfestation = "no bugs:(";
            bugIndex = 0;
            plantSimulationInstance.bugIndex = bugIndex;
        }
    }

    public void CheckWeather()
    {
        switch (EventManager._weatherEvent)
        {
            case 0:
                weatherOutput = "CLEAR";
                weatherIndex = 0;
                plantSimulationInstance.weatherIndex = weatherIndex;
                break;
            case 1:
                weatherOutput = "HEAT HAZE";
                weatherIndex = 1;
                plantSimulationInstance.weatherIndex = weatherIndex;
                break;
            case 2:
                weatherOutput = "TYPHOON";
                weatherIndex = 2;
                plantSimulationInstance.weatherIndex = weatherIndex;
                break;
        }
    }

    public void CheckSeason()
    {
        if (TimeOfDayUI.isDrySeason)
        {
            seasonOutput = "DRY SEASON";
            seasonIndex = 0;
            plantSimulationInstance.seasonIndex = seasonIndex;
        }
        else
        {
            seasonOutput = "Wet SEASON";
            seasonIndex = 1;
            plantSimulationInstance.seasonIndex = seasonIndex;
        }
    }

    public void CheckDay()
    {
        if (TimeOfDayUI.isDay)
        {
            cycleOutput = "Day";
            cycleIndex = 0;
            plantSimulationInstance.dayIndex = cycleIndex;
        }
        else
        {
            cycleOutput = "Night";
            cycleIndex = 1;
            plantSimulationInstance.dayIndex = cycleIndex;
        }
    }

    public void IsWaterLogged()
    {
        Debug.Log("Entered is waterlogged");
        if (!TimeOfDayUI.isDrySeason&&EventManager._weatherEvent==2)
        {
            Debug.Log("Adding To Waterlogged");
            WaterloggedMeter += WaterFillUpRate;
        }

        if (WaterloggedMeter >= WaterloggedMax)
        {
            Water.SetActive(true);
            Waterlogged = true;
        }
        Debug.Log("Water Logged Meter: " + WaterloggedMeter);
    }

    public void WaterClear()
    {
        Waterlogged = false;
        WaterloggedMeter = 0;
        WaterCooldown = 50;
        Water.SetActive(false);
    }

    // Remove Bug Stats// Pesticide
    public void unBug()
    {
        bugIndex = 0;
        plantSimulationInstance.bugIndex = bugIndex;
    }

    public void RefreshPlot()
    {
        isplantable = true;
    }

    public void IsNotPlantable()
    {
        isplantable = false;
    }

    public void RemovePlant()
    {
        ResetPlot();
        IsNotPlantable();
    }

    //Fertilizer, Super Yield
    public void SuperCharge()
    {
        GoalManager goalManager = Object.FindFirstObjectByType<GoalManager>();
        int super_yield = plantSimulationInstance.GetMaxYield();
        goalManager.AddCrop(currentSeed.cropName, super_yield);
        RemovePlant();
        IsNotPlantable();
    }

    public void GetMogged()
    {
        Debug.Log(
            "LOOK!!!!!\n" +
            "Season: " + seasonOutput + " | Resistance: " + plantSimulationInstance.stats.seasonalAffinities[seasonIndex] + "\n"
            + "Day: " + cycleOutput + " | Resistance: " + plantSimulationInstance.stats.cycleAffinities[cycleIndex] + "\n"
            + "Weather Event: " + weatherOutput + " | Resistance: " + plantSimulationInstance.stats.weatherAffinities[weatherIndex] + "\n"
            + "Infested: " + bugInfestation + " | Resistance: " + plantSimulationInstance.stats.bugResistances[bugIndex] + "\n"

            + "Crop HP: " + plantSimulationInstance.cropHP + "\n"
            + "Crop Moisture: " + plantSimulationInstance.cropMoisture + "\n"
            + "Crop Growth: " + plantSimulationInstance.cropGrowth + "\n"
            + "Soil Quality: " + plantSimulationInstance.soilQuality + "\n"
            + "Soil Moisture: " + plantSimulationInstance.soilMoisture + "\n"
            + "Soil Softness: " + plantSimulationInstance.soilSoftness + "\n"
            + "Harvest Quality: " + plantSimulationInstance.harvestQuality + "\n"
            + "Crop Yield: " + plantSimulationInstance.GetCropYield() + "\n"
            + "Is Water logged?" + Waterlogged
        );
    }
}