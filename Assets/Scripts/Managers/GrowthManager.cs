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
    [SerializeField] private bool WaterCleared;
    [SerializeField] private float WaterloggedMeter;
    [SerializeField] private float WaterCooldown;
    [SerializeField] private float WaterloggedMax;
    [SerializeField] private float WaterFillUpRate;
    [SerializeField] private float WaterDuration;
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

    [Header("Safety Net")]
    [SerializeField] float maxLimit = 80f;
    [SerializeField] float recoveryAmountPerTick = 2f;
    [SerializeField] float minSafe = -20f;
    [SerializeField] float maxSafe = 60f;
    [SerializeField] float recoveryRate = 2f; // How many points it recovers per tick
    [SerializeField] float targetCenter = 20f;
    [SerializeField] float reductionFactor = 0.3f;

    // Direct link to the math backend script
    public BasePlant plantSimulationInstance;

     public bool isPlanted = false;

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
        Debug.Log("Here at waterlogged");
        if (!isPlanted)
            IsWaterLogged();
        if (WaterCleared&&!isplantable)
        {
            DecreaseWaterlogged();
        }


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
    // --- LINKED MINIGAME STAT RESOLUTION ROUTINES ---
    public void ResolveMinigameWin(MinigameType type)
    {
        Debug.Log($"Minigame {type} WON. Correcting stats on plot!");
        if (plantSimulationInstance == null) return;

        float centerPoint = 20f; // The middle of your -20 to 60 sweet spot

        // Changing this to 0.3f brings the stat well inside the safe harvest zone
        float reductionFactor = 0.3f;

        switch (type)
        {
            case MinigameType.Watering:
                // Example with 0.3f: 
                // If moisture is at 100 -> (100 - 20) * 0.3f + 20 = 44f (Safely below the 60f threshold!)
                // If moisture is at -60 -> (-60 - 20) * 0.3f + 20 = -4f (Safely above the -20f threshold!)
                plantSimulationInstance.soilMoisture = (plantSimulationInstance.soilMoisture - centerPoint) * reductionFactor + centerPoint;
                plantSimulationInstance.cropMoisture = (plantSimulationInstance.cropMoisture - centerPoint) * reductionFactor + centerPoint;
                WaterClear();
                break;

            case MinigameType.StructuralSupport:
                plantSimulationInstance.soilSoftness = (plantSimulationInstance.soilSoftness - centerPoint) * reductionFactor + centerPoint;
                break;

            case MinigameType.Weeding:
                plantSimulationInstance.soilQuality = Mathf.Min(plantSimulationInstance.soilQuality + 2.0f, 100f);
                break;

            case MinigameType.PestControl:
                bugInfestation = "no bugs:(";
                bugIndex = 0;
                plantSimulationInstance.bugIndex = bugIndex;
                break;

            case MinigameType.SoilEnrichment:
                plantSimulationInstance.soilQuality = 40.0f; // Adjusted downward from 60f so it sits comfortably inside the sweet spot
                break;

            case MinigameType.Netting:
                plantSimulationInstance.cropHP = Mathf.Min(plantSimulationInstance.cropHP + 2.0f, 100f);
                break;
        }

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
        if (!isPlanted || currentSeed == null || plantSimulationInstance == null)
        {
            hasNoBadStats = false;
            return;
        }

        cropMoisture = plantSimulationInstance.cropMoisture;
        soilQuality = plantSimulationInstance.soilQuality;
        soilMoisture = plantSimulationInstance.soilMoisture;
        soilSoftness = plantSimulationInstance.soilSoftness;

        // Your custom range bounds
        float minLimit = -20f;
        float maxLimit = 60f;

        if (cropMoisture < minLimit || cropMoisture > maxLimit ||
            soilQuality < minLimit || soilQuality > maxLimit ||
            soilMoisture < minLimit || soilMoisture > maxLimit ||
            soilSoftness < minLimit || soilSoftness > maxLimit)
        {
            hasNoBadStats = false;
            PlantSadFG.color = new Color(1f, 1f, 1f, 1f);
            PlantSadBG.color = new Color(1f, 1f, 1f, 1f);
        }
        else
        {
            hasNoBadStats = true;
            // Optionally clear the sad face overlay here if they are healthy
            PlantSadFG.color = new Color(1f, 1f, 1f, 0f);
            PlantSadBG.color = new Color(1f, 1f, 1f, 0f);
        }
    }


    /// <summary>
    /// Evaluates structural crop health conditions and handles targeted degradation logic.
    /// </summary>
    /// <summary>
    /// Evaluates structural crop health conditions and handles targeted degradation logic.
    /// </summary>
    public void EvaluatePlotHealth()
    {
        // Safety shield
        if (!isPlanted || plantSimulationInstance == null) return;

        float maxLimit = 80f;
        float recoveryAmountPerTick = 2f; // How fast stats slowly decay back to 0 on their own

        // If stats are outside the -20 to 60 boundary, pull them toward the center point
        if (plantSimulationInstance.soilMoisture < minSafe || plantSimulationInstance.soilMoisture > maxSafe)
        {
            plantSimulationInstance.soilMoisture = Mathf.MoveTowards(plantSimulationInstance.soilMoisture, targetCenter, recoveryRate);
        }

        if (plantSimulationInstance.cropMoisture < minSafe || plantSimulationInstance.cropMoisture > maxSafe)
        {
            plantSimulationInstance.cropMoisture = Mathf.MoveTowards(plantSimulationInstance.cropMoisture, targetCenter, recoveryRate);
        }

        if (plantSimulationInstance.soilSoftness < minSafe || plantSimulationInstance.soilSoftness > maxSafe)
        {
            plantSimulationInstance.soilSoftness = Mathf.MoveTowards(plantSimulationInstance.soilSoftness, targetCenter, recoveryRate);
        }

        IsWaterLogged();
        // Handle waterlogging if moisture stays dangerously high
        
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
        if (WaterDuration > 0) return;
        Debug.Log("Entered is waterlogged");
        if (!TimeOfDayUI.isDrySeason)
        {
            Debug.Log("Adding To Waterlogged");
            WaterloggedMeter += WaterFillUpRate;
        }

        if (WaterloggedMeter >= WaterloggedMax)
        {
            Water.SetActive(true);
            Waterlogged = true;
            WaterCleared = true;
        }
        Debug.Log("Water Logged Meter: " + WaterloggedMeter);
    }
    public void DecreaseWaterlogged()
    {
        if (WaterDuration > 0)
        {
            WaterDuration -= WaterFillUpRate;
        }
    }
    public void WaterClear()
    {
        Waterlogged = false;
        WaterloggedMeter = 0;
        WaterDuration = WaterCooldown;
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