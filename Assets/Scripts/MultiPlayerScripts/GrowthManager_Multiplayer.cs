#if UNITY_EDITOR
using Unity.Android.Gradle.Manifest;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEditor.EditorTools;

#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

public class GrowthManager_Multiplayer : MonoBehaviour, IPointerClickHandler
{

    [Header("Player-Specific UI Router Assignments")]
    [SerializeField] private CanonFire assignedCanon;
    [SerializeField] public GameObject Untilled;
    [SerializeField] public GameObject Tilled;



    [Header("Visual Components")]
    [SerializeField] public SpriteRenderer plantRenderer;
    [SerializeField] public SpriteRenderer PlantSadBG;
    [SerializeField] public SpriteRenderer PlantSadFG;
    [SerializeField] public Sprite PlantSadBGTexture;
    [SerializeField] public Sprite PlantSadFGTexture;

    [Header("Live Growth Debugger (Visible for Testing)")]
    [SerializeField] public SeedData currentSeed;
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
    [SerializeField] private GameObject Bugging;

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
    [SerializeField] float minLimit = -20f;
    [SerializeField] float centerPoint = 20f; // The middle of your -20 to 60 sweet spot

    // Changing this to 0.3f brings the stat well inside the safe harvest zone
    [SerializeField] float reductionFactor = 0.3f;
    [SerializeField] float recoveryAmountPerTick = 2f;
    [SerializeField] float closetothecenter = 40f;
    [Header("Stat Paremeters")]
    [SerializeField] float minSafe = -20f;
    [SerializeField] float maxSafe = 60f;
    [SerializeField] float recoveryRate = 2f; // How many points it recovers per tick
    [SerializeField] float targetCenter = 20f;
    
    [SerializeField] float BugCooldownMeter;
    [SerializeField] float BugCooldownMeterMax = 100f;
    [SerializeField] float BugCooldownRate = 10;
    [SerializeField] bool unBugged;
    [SerializeField] bool _IsInfested;

    // Direct link to the math backend script
    public BasePlant plantSimulationInstance;

     public bool isPlanted = false;

    private void Start()
    {
        Waterlogged = false;
        Water.SetActive(false);
        BugCooldownMeter = 0;
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
        if (unBugged)
        {
            UnBugCountdown();
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
        
      

        // LINE ADDED HERE: Run real-time condition evaluation checks

       
       
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

            // FIXED: Using localized assignments instead of scene-wide searching
            if ( yieldAmount > 0)
            {
                
     
                    Debug.Log("[STEP 2] HARVESTING SEED");
                    assignedCanon.AddLoad(yieldAmount);
                    IsNotPlantable();
                    ResetPlot();
                
               
               
            }
        }
    }

    // Called by my drag and drop system to start planting
    public void PlantSeed(SeedData data)
    {
        if (isPlanted || data == null) return;
        Debug.LogWarning("[PASSED 1] Passed by Is Planted ");
        if (Waterlogged || !isplantable) return;
        Debug.LogWarning("[PASSED 2] Passed by Is Plantable ");
        if (data.remainingSeedBags <= 0)
        {
            Debug.LogWarning($"[Out of Seeds] Can't plant anymore {data.cropName}! 0 bags remaining.");
            return;
        }
        Debug.LogWarning("[PASSED 3] Passed by seedbads ");

        // Safety check in case I forgot to add a sprite to the asset file
        if (data.growthStages == null || data.growthStages.Length == 0)
        {
            Debug.LogError($"[GrowthManager] Can't plant {data.cropName}! The SeedData needs at least 1 sprite in the array.");
            return;
        }
        Debug.LogWarning("[PASSED 4] PassedTS ");

        currentSeed = data;
        isPlanted = true;
        currentStage = 0;
        AudioManager.instance.Play("Planting");

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
        unBug();
        isPlanted = false;
        currentSeed = null;
        plantSimulationInstance = null;
        currentStage = 0;
        if (plantRenderer != null) plantRenderer.sprite = null;
        plantableornot();
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



    // --- LINKED MINIGAME STAT RESOLUTION ROUTINES ---
    // --- LINKED MINIGAME STAT RESOLUTION ROUTINES ---
 

    /// <summary>
    /// Evaluates structural crop health conditions and handles targeted degradation logic.
    /// </summary>
    /// <summary>
    /// Evaluates structural crop health conditions and handles targeted degradation logic.
    /// </summary>


    private void disableSadParts()
    {
        PlantSadBG.color = new Color(1f, 1f, 1f, 0f);
        PlantSadFG.color = new Color(1f, 1f, 1f, 0f);
    }


    public void plantableornot()
    {
        if (isplantable)
        {

            Untilled.SetActive(false);
            Tilled.SetActive(true);
        }
        else
        {
            Tilled.SetActive(false);
            Untilled.SetActive(true);
        }

    }
    public void CheckInfestation()
    {
        if (EventManager.isInfested)
        {
            if (BugCooldownMeter <= 0)
            {
                bugInfestation = "INFESTEDDD";
                bugIndex = 1;
                plantSimulationInstance.bugIndex = bugIndex;
                unBugged = false;
                Bugging.SetActive(true);
                BugCooldownMeter = BugCooldownMeterMax;
                _IsInfested = true;
            }
          
        }
        else
        {
            bugInfestation = "no bugs:(";
            bugIndex = 0;
            plantSimulationInstance.bugIndex = bugIndex;
            Bugging.SetActive(false);
            _IsInfested = false;
        }
    }
    public void UnBugCountdown()
    {
        BugCooldownMeter -= BugCooldownRate;
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
        Debug.Log("YOU HAVE BEEN UNBUGGED");
        if (!isPlanted||_IsInfested!=true) return;
        bugIndex = 0;
        plantSimulationInstance.bugIndex = bugIndex;
        Bugging.SetActive(false);
        unBugged = true;
        _IsInfested=false;
    }

    public void RefreshPlot()
    {
        isplantable = true; plantableornot();
    }

    public void IsNotPlantable()
    {
        isplantable = false; plantableornot();
    }

    public void RemovePlant()
    {
        unBug();
        ResetPlot();
        plantableornot();
        IsNotPlantable();
    }

    //Fertilizer, Super Yield
    public void SuperCharge()
    {
        if (!isPlanted) return;

        int super_yield = plantSimulationInstance.GetMaxYield();
        Debug.Log("[STEP 2.5] SUPER");
        assignedCanon.AddLoad(super_yield);
        RemovePlant();
        plantableornot();
        IsNotPlantable();
    }


    public void CommitAction(string action)
    {
        if (!isPlanted) return;
        {
            switch (action)
            {
                case "GetWaterLogged":
                    Water.SetActive(true);
                    Waterlogged = true;
                    WaterloggedMeter = WaterloggedMax;

                    break;

                case "RemovePlants":
                    ResetPlot();
                    break;

                case "UnTillable":
                    isplantable = false;

                    
                    break;

                case "GETBUGGED":
                    BugCooldownMeter = 0f;
                    bugInfestation = "INFESTEDDD";
                    bugIndex = 1;
                    plantSimulationInstance.bugIndex = bugIndex;
                    unBugged = false;
                    Bugging.SetActive(true);
                    _IsInfested = true;

                    break;

                case "FERTILIZING":

                    plantSimulationInstance.cropGrowth = 10;
                    break;



            }



        }
      
    }

}