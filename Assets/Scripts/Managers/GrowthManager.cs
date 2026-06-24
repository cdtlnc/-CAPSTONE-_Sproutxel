#if UNITY_EDITOR
using Unity.Android.Gradle.Manifest;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEditor.EditorTools;

#endif
using UnityEngine.EventSystems;
using UnityEngine;
using Unity.VisualScripting;
using System.Collections;

public class GrowthManager : MonoBehaviour, IPointerClickHandler
{
    [Header("Visual Components")]
    [SerializeField] public SpriteRenderer plantRenderer;
    [SerializeField] public SpriteRenderer PlantSadBG;
    [SerializeField] public SpriteRenderer PlantSadFG;
    [SerializeField] public Sprite PlantSadBGTexture;
    [SerializeField] public GameObject Untilled;
    [SerializeField] public GameObject Tilled;

    [Header("Weather Death Animations")]
    [SerializeField] public Sprite[] HeatDeath, TyphoonDeath, Harvestable;
    [SerializeField] public Sprite TyphoonBG;
    [SerializeField] public Animator ForeAnim,BackAnim;


    [Header("Live Growth Debugger (Visible for Testing)")]
    [SerializeField] public SeedData currentSeed;
    [SerializeField] private int currentStage = 0;
    [SerializeField] private float cropGrowth;
    private float[] growthThresholds = new float[] { 0f, 2f, 4f, 6f, 14f };

    [Header("Plant Stats")]
    [SerializeField] public string name;
    [SerializeField] private float cropHP;
    [SerializeField] private bool wonMinigame, hasNoBadStats;
    [SerializeField] bool unBugged;
    [SerializeField] bool _IsInfested;
    [SerializeField] public string le_realname;

    [Header("Crop Moisture")]
    [SerializeField] private float cropMoisture;

    [Header("Soil Quality")]
    [SerializeField] private float soilQuality;

    [Header("Soil Moisture")]
    [SerializeField] private float soilMoisture;

    [Header("Soil Softness")]
    [SerializeField] private float soilSoftness;

    [Header("Seed Class")]
    [SerializeField] string seasonOutput;
    [SerializeField] string cycleOutput;
    [SerializeField] string weatherOutput;
    [SerializeField] string bugInfestation;

    [SerializeField] int seasonIndex;
    [SerializeField] int cycleIndex;
    [SerializeField] int weatherIndex;
    [SerializeField] int bugIndex;

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

    

    [Header("Item Checker")]
    [SerializeField] private bool isplantable;

    [Header("Safety Net")]
         float maxLimit = 50f;
         float minLimit = -50f;

    float maxRange = 100f;
    float minRange = -100f;
    [SerializeField] float centerPoint = 20f; // The middle of your -20 to 60 sweet spot

    // Changing this to 0.3f brings the stat well inside the safe harvest zone
    [SerializeField] float reductionFactor = 0.3f;
    [SerializeField] float recoveryAmountPerTick = 2f;
    [SerializeField] float closetothecenter = 40f;
    [Header("Stat Paremeters")]
     float minSafe = -50f;
     float maxSafe = 50f;
     float recoveryRate = 1f; // How many points it recovers per tick
    [SerializeField] float targetCenter = 20f;
    
    [SerializeField] float BugCooldownMeter;
    [SerializeField] float BugCooldownMeterMax = 100f;
    [SerializeField] float BugCooldownRate = 10;


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
        plantableornot();
        checkDead();
        CheckWeather();
        CheckSeason();
        CheckDay();
        CheckInfestation();
        CheckStats();

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
            plantSimulationInstance.GetGrowth();// THis is whats causing the growth
            plantSimulationInstance.GetHarvestQuality();
            cropGrowth=plantSimulationInstance.cropGrowth;
        }
        if (currentSeed.growthStages == null || currentSeed.growthStages.Length == 0) return;
        UpdateGrowth();


        // LINE ADDED HERE: Run real-time condition evaluation checks
        //EvaluatePlotHealth();
       
      
        UpdatePlantSprite();
       
    }

    public void UpdateGrowth()
    {

        if (currentSeed.growthStages == null || currentSeed.growthStages.Length == 0) return;

        float currentGrowth = plantSimulationInstance.cropGrowth;
        int targetStage = 0;

        // Loop through thresholds and find the highest one the plant has passed
        for (int i = 0; i < growthThresholds.Length; i++)
        {
            // Make sure we don't look for a threshold past our available sprites
            if (i >= currentSeed.growthStages.Length) break;

            if (currentGrowth >= growthThresholds[i])
            {
                targetStage = i;
                if (currentGrowth >= growthThresholds[3])
                {
                    PlantSadBG.color = new Color(1f, 1f, 1f, 1f);
                    BackAnim.SetInteger("HarvestingTrigger", 1);
                }
               
            }
        }

        currentStage = targetStage;
        if (currentGrowth >= 12f)
        {
            AnimationBG();
        }
    }

    // Tapping/clicking the plot to harvest
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("[STEP 1] Entering Harvesting");
        if (!isPlanted || currentSeed == null || currentSeed.growthStages == null) return;

        int lastConfiguredStageIndex = currentSeed.growthStages.Length - 1;
        int MatureStageIndex = currentSeed.growthStages.Length - 2;
        Debug.Log("[STEP 2] Entering Mature Stage");
       
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

            Debug.Log("[STEP 3] Looking for Goal Manager");
            GoalManager goalManager = Object.FindFirstObjectByType<GoalManager>();
            Debug.Log("[STEP 4] Found Goal Manager");

            if (goalManager != null)
            if (goalManager != null)
            {
                
                    Debug.Log("[STEP 5.B] Has Bad Stats");
                    MaintenencePopUp ui = Object.FindFirstObjectByType<MaintenencePopUp>();
                    if (ui != null)
                    {
                        Debug.Log("[STEP 6] Opening UI");
                        ui.OpenWindow(this); // Opens the window exactly ONCE
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
        plantSimulationInstance.cropMoisture = 0f;
        plantSimulationInstance.soilMoisture = 0f;
        plantSimulationInstance.soilSoftness = 0f;
        plantSimulationInstance.soilQuality = 0f; 

        data.remainingSeedBags--;
        
        EndGameManager end=GameObject.FindFirstObjectByType<EndGameManager>();
        end.LoseSeed();
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

   

 


    public void ResolveMinigameWin(MinigameType type)
    {
        Debug.Log($"Minigame {type} WON. Correcting stats on plot!");
        if (plantSimulationInstance == null) return;

        bool MinigameCorrect;

        switch (type)
        {
            case MinigameType.Watering:
                if (plantSimulationInstance.cropMoisture < minSafe|| plantSimulationInstance.cropMoisture < minSafe)
                {
                    plantSimulationInstance.cropMoisture = (plantSimulationInstance.cropMoisture - centerPoint) * reductionFactor + centerPoint;
                    
                    WaterClear();
                    statsTowardsTheCenter();
                }
                else
                {
                    plantSimulationInstance.cropMoisture = (plantSimulationInstance.cropMoisture - centerPoint) * reductionFactor + centerPoint;
                   
                    Debug.Log("Wrong Minigame");
                }



                    break;

            case MinigameType.StructuralSupport:
                if (plantSimulationInstance.soilSoftness > maxSafe)
                {
                    Debug.Log("Entered Structurual Support Minigame");
                    plantSimulationInstance.soilSoftness = (plantSimulationInstance.soilSoftness - centerPoint) * reductionFactor + centerPoint;
                    statsTowardsTheCenter();
                }
                else 
                {
                    plantSimulationInstance.soilSoftness = (plantSimulationInstance.soilSoftness - centerPoint) * reductionFactor + centerPoint;
                }

                    break;

            case MinigameType.Weeding:
                if ( plantSimulationInstance.soilQuality > maxSafe)
                {
                    plantSimulationInstance.soilQuality = Mathf.Min(plantSimulationInstance.soilQuality + 2.0f, 100f);
                    plantSimulationInstance.soilSoftness = (plantSimulationInstance.soilSoftness - centerPoint) * reductionFactor + centerPoint;
                    statsTowardsTheCenter();
                }
                else
                {
                    plantSimulationInstance.soilQuality = Mathf.Min(plantSimulationInstance.soilQuality + 2.0f, 100f);
                    plantSimulationInstance.soilSoftness = (plantSimulationInstance.soilSoftness - centerPoint) * reductionFactor + centerPoint;
                }

                    break;

            case MinigameType.PestControl:
                if (_IsInfested|| plantSimulationInstance.soilSoftness < minSafe)
                {
                    unBug();
                    statsTowardsTheCenter();
                }
                else
                {
                    plantSimulationInstance.soilSoftness = 0f;
                }

                    break;

            case MinigameType.SoilEnrichment:
                if (plantSimulationInstance.soilQuality < minSafe || plantSimulationInstance.soilMoisture > maxLimit)
                {
                    plantSimulationInstance.soilQuality = 40.0f; // Adjusted downward from 60f so it sits comfortably inside the sweet spot
                    statsTowardsTheCenter();
                    WaterClear();
                }
                else
                {
                    plantSimulationInstance.soilQuality = 0f;
                }

                    break;

            case MinigameType.Netting:
                if (plantSimulationInstance.cropMoisture > maxSafe)
                {
                    plantSimulationInstance.cropHP = Mathf.Min(plantSimulationInstance.cropHP + 2.0f, 100f);
                    statsTowardsTheCenter();
                }
                else
                {
                    plantSimulationInstance.cropMoisture  = 0f;
                }
                
                break;
        }

        CheckStats();
        UpdatePlantSprite();
    }

    public void statsTowardsTheCenter()
    {
        Debug.Log("STATED TOWARDED THE CENTEREDED");
        plantSimulationInstance.soilMoisture = Mathf.MoveTowards(plantSimulationInstance.soilMoisture, targetCenter, closetothecenter);
        plantSimulationInstance.cropMoisture = Mathf.MoveTowards(plantSimulationInstance.cropMoisture, targetCenter, closetothecenter);
        plantSimulationInstance.soilSoftness = Mathf.MoveTowards(plantSimulationInstance.soilSoftness, targetCenter, closetothecenter);
        plantSimulationInstance.soilQuality = Mathf.MoveTowards(plantSimulationInstance.soilQuality, targetCenter, closetothecenter);

    }

   

    public void ResolveMinigameLose(MinigameType type)
    {
        Debug.Log($"Minigame {type} FAILED. Penalizing crop stats.");

        if (plantSimulationInstance == null) return;

        switch (type)
        {
            case MinigameType.Watering:
                plantSimulationInstance.cropHP -= 10.0f;
                break;
            case MinigameType.PestControl:
                plantSimulationInstance.cropHP -= 10.0f;
                break;
            case MinigameType.Netting:
                plantSimulationInstance.cropHP -= 10.0f; // Custom damage deduction if they mess up netting loops
                break;
            default:
                plantSimulationInstance.cropHP -= 10.0f;
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
        

    
        if (cropMoisture < minLimit || cropMoisture > maxLimit ||
            soilQuality < minLimit || soilQuality > maxLimit ||
            soilMoisture < minLimit || soilMoisture > maxLimit ||
            soilSoftness < minLimit || soilSoftness > maxLimit)
        {
            hasNoBadStats = false;
           
        }
        else
        {
            hasNoBadStats = true;
        
        
        }


        //Check if 100 of anything
        if (cropMoisture >= maxRange || cropMoisture <= minRange || soilQuality >= maxRange || soilQuality <= minRange || soilMoisture >= maxRange || soilMoisture <= minRange || soilSoftness >= maxRange || soilSoftness <= minRange)
        {
            ResetPlot();// Kill plant
        }
        else if (cropMoisture >= maxLimit || cropMoisture <= minLimit || 
            soilQuality >= maxLimit || soilQuality <= minLimit || 
            soilMoisture >= maxLimit || soilMoisture <= minLimit || 
            soilSoftness >= maxLimit || soilSoftness <= minLimit)
        {
            AnimationBG();
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

        // How fast stats slowly decay back to 0 on their own

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
        BackAnim.SetInteger("HarvestingTrigger", 0);
        ForeAnim.SetInteger("WeatherTrigger", 0);
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
                plantSimulationInstance.cropHP -= 5f;
                AudioManager.instance.Play("BugInfestation");
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
        if (!isPlanted) return;
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
        WeatherAnimationPlayFG(weatherIndex);
    }
    public void WeatherAnimationPlayFG(int play)
    {
        
        if (!isPlanted) return;
        switch (play)
        {
            case 0:
                PlantSadFG.color = new Color(1f, 1f, 1f, 0f);
                break;
            case 1:// HeatDaze
                PlantSadFG.color = new Color(1f, 1f, 1f, 1f);
                PlantSadFG.transform.localScale = new Vector3(0.02685377f, 0.01804939f, 0.03480541f);
                break;
            case 2://Typhoon
                PlantSadFG.color = new Color(1f, 1f, 1f, 1f);
                PlantSadFG.transform.localScale = new Vector3(0.01527228f, 0.01026505f, 0.01979453f);
                break;
           
        }
        ForeAnim.SetInteger("WeatherTrigger", play);

    }
    public void AnimationBG()
    {
        Debug.Log("Entered Animation BG");
        float animationTimer = 0f;
        if (!isPlanted) return;

       
        Color end1 = Color.white;
        Color end2 = new Color(255f / 255f, 77f / 255f, 77f / 255f);

   
        animationTimer += Time.deltaTime;

      
        float lerpPercentage = Mathf.PingPong(animationTimer * 2.0f, 1.0f);

   
        plantRenderer.color = Color.Lerp(end1, end2, lerpPercentage);

        PlantSadFG.color = new Color(1f, 1f, 1f, 1f);
        PlantSadFG.transform.localScale = new Vector3(0.01527228f, 0.01026505f, 0.01979453f);
        ForeAnim.SetBool("Danger", true);
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
        //Debug.Log("Entered is waterlogged");
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
        //Debug.Log("Water Logged Meter: " + WaterloggedMeter);
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

    public void checkDead() // For Health and Growth
    {
        if (cropHP <= 0)
        {
            ResetPlot();

        }
        if (plantSimulationInstance.cropGrowth == 15f)
        {
            ResetPlot();
        }
    }



    public void IsNotPlantable()
    {
        isplantable = false;
        plantableornot();
    }
    void ResetPlot()
    {
        unBug();
        BackAnim.SetInteger("HarvestingTrigger", 0);
        ForeAnim.SetInteger("WeatherTrigger", 0);

        ForeAnim.SetBool("Danger", true);
        isPlanted = false;
        currentSeed = null;
        plantSimulationInstance = null;
        currentStage = 0;
        UpdatePlantSprite();
        if (plantRenderer != null) plantRenderer.sprite = null;
        disableSadParts();
        IsNotPlantable();
    }
    public void HarvestPlant() // New Shovel
    {
        if (!isPlanted) return;
        int yieldAmount = plantSimulationInstance.GetCropYield();
        GoalManager goalManager = Object.FindFirstObjectByType<GoalManager>();
        goalManager.AddCrop(currentSeed.cropName, yieldAmount);
        Debug.Log("Plant Harvested");
        IsNotPlantable();
        ResetPlot(); // Cleanly clear the plot out instantly
    }

    public void RefreshPlot()
    {
        Debug.Log("YOU HAVE BEEN SOIL TILLED");
        isplantable = true;
        plantableornot();
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

    //Fertilizer, Super Yield
    public void SuperCharge()
    {if (!isPlanted) return;
        GoalManager goalManager = Object.FindFirstObjectByType<GoalManager>();
        int super_yield = plantSimulationInstance.GetMaxYield();
        goalManager.AddCrop(currentSeed.cropName, super_yield);
        ResetPlot();
        IsNotPlantable();
    }

}