#if UNITY_EDITOR
using Unity.Android.Gradle.Manifest;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEditor;
using UnityEditor.EditorTools;
#endif
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using Photon.Pun;

public class GrowthManager_Multiplayer : MonoBehaviourPun, IPointerClickHandler
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
    [SerializeField] private string cropNameDisplay; // renamed 'name' to avoid hiding GameObject.name
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
    [SerializeField] float centerPoint = 20f;

    [Header("Stat Parameters")]
    [SerializeField] float minSafe = -20f;
    [SerializeField] float maxSafe = 60f;
    [SerializeField] float recoveryRate = 2f;
    [SerializeField] float targetCenter = 20f;

    [SerializeField] float BugCooldownMeter;
    [SerializeField] float BugCooldownMeterMax = 100f;
    [SerializeField] float BugCooldownRate = 10;
    [SerializeField] bool unBugged;
    [SerializeField] bool _IsInfested;

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
        // Only run tick calculations if we are the MasterClient to prevent data drift
        if (PhotonNetwork.IsMasterClient)
        {
            HandleTick();
        }
    }

    public void HandleTick()
    {
        if (!isPlanted)
            IsWaterLogged();
        if (WaterCleared && !isplantable)
        {
            DecreaseWaterlogged();
        }
        if (unBugged)
        {
            UnBugCountdown();
        }

        if (!isPlanted || currentSeed == null || plantSimulationInstance == null) return;

        if (plantSimulationInstance.stats == null)
        {
            plantSimulationInstance.cropGrowth += 1.0f;
            plantSimulationInstance.cropGrowth = Mathf.Clamp(plantSimulationInstance.cropGrowth, 0f, 10f);
        }
        else
        {
            plantSimulationInstance.GetStatsOvertime();
            plantSimulationInstance.GetSoilQuality();
            plantSimulationInstance.GetHealth();
            plantSimulationInstance.GetGrowth();
            plantSimulationInstance.GetHarvestQuality();
        }

        if (currentSeed.growthStages == null || currentSeed.growthStages.Length == 0) return;

        int totalSpritesAvailable = currentSeed.growthStages.Length;
        int finalStageIndex = totalSpritesAvailable - 1;

        if (finalStageIndex > 0)
        {
            float growthRatio = plantSimulationInstance.cropGrowth / 10f;
            int targetStage = Mathf.FloorToInt(growthRatio * finalStageIndex);
            currentStage = Mathf.Clamp(targetStage, 0, finalStageIndex);
        }
        else
        {
            currentStage = 0;
        }

        CheckWeather();
        CheckSeason();
        CheckDay();

        // Broadcast visual growth changes across the network
        photonView.RPC("RPC_UpdateGrowthVisuals", RpcTarget.All, currentStage, plantSimulationInstance.cropGrowth);
    }

    [PunRPC]
    private void RPC_UpdateGrowthVisuals(int synchronizedStage, float synchronizedGrowth)
    {
        currentStage = synchronizedStage;
        if (plantSimulationInstance != null)
        {
            plantSimulationInstance.cropGrowth = synchronizedGrowth;
        }
        UpdatePlantSprite();
    }

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
            }
            else
            {
                yieldAmount = plantSimulationInstance.GetCropYield();
            }

            if (yieldAmount > 0)
            {
                // Harvest action is synced over network
                photonView.RPC("RPC_HarvestPlot", RpcTarget.All, yieldAmount);
            }
        }
    }

    [PunRPC]
    private void RPC_HarvestPlot(int yieldAmount)
    {
        if (assignedCanon != null)
        {
            assignedCanon.AddLoad(yieldAmount);
        }
        IsNotPlantable();
        ResetPlot();
    }

    public void PlantSeed(SeedData data)
    {
        if (data == null) return;
        // Redirect standard local calls to networked version passing its name asset string
        photonView.RPC("RPC_PlantSeedByName", RpcTarget.All, data.name);
    }

    [PunRPC]
    public void RPC_PlantSeedByName(string seedAssetName)
    {
        if (isPlanted || Waterlogged || !isplantable) return;

        // CRITICAL WORKFLOW REQUIREMENT: 
        // Put your SeedData assets inside a folder named 'Resources' so Photon can look them up by name string!
        SeedData data = Resources.Load<SeedData>(seedAssetName);
        if (data == null)
        {
            Debug.LogError($"[Photon Growth] Could not find asset '{seedAssetName}' inside any Resources folder!");
            return;
        }

        currentSeed = data;
        isPlanted = true;
        currentStage = 0;

        if (AudioManager.instance != null) AudioManager.instance.Play("Planting");

        plantSimulationInstance = new BasePlant();
        plantSimulationInstance.stats = data.plantStatsTemplate;
        plantSimulationInstance.growthStages = data.growthStages;

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
    }

    private void UpdatePlantSprite()
    {
        if (plantRenderer != null && currentSeed != null && currentSeed.growthStages != null && currentStage < currentSeed.growthStages.Length)
        {
            if (currentSeed.growthStages[currentStage] != null)
            {
                plantRenderer.sprite = currentSeed.growthStages[currentStage];
            }
        }
    }

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
        if (!isPlanted || currentSeed == null || plantSimulationInstance == null)
        {
            cropNameDisplay = "Empty Plot";
            cropHP = 0f;
            cropMoisture = 0f;
            soilQuality = 0f;
            soilMoisture = 0f;
            soilSoftness = 0f;
            return;
        }

        cropNameDisplay = currentSeed.cropName;
        cropHP = plantSimulationInstance.cropHP;
        cropMoisture = plantSimulationInstance.cropMoisture;
        soilQuality = plantSimulationInstance.soilQuality;
        soilMoisture = plantSimulationInstance.soilMoisture;
        soilSoftness = plantSimulationInstance.soilSoftness;
    }

    private void disableSadParts()
    {
        if (PlantSadBG != null) PlantSadBG.color = new Color(1f, 1f, 1f, 0f);
        if (PlantSadFG != null) PlantSadFG.color = new Color(1f, 1f, 1f, 0f);
    }

    public void plantableornot()
    {
        if (isplantable)
        {
            if (Untilled != null) Untilled.SetActive(false);
            if (Tilled != null) Tilled.SetActive(true);
        }
        else
        {
            if (Tilled != null) Tilled.SetActive(false);
            if (Untilled != null) Untilled.SetActive(true);
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
                if (plantSimulationInstance != null) plantSimulationInstance.bugIndex = bugIndex;
                unBugged = false;
                if (Bugging != null) Bugging.SetActive(true);
                BugCooldownMeter = BugCooldownMeterMax;
                _IsInfested = true;
            }
        }
        else
        {
            bugInfestation = "no bugs:(";
            bugIndex = 0;
            if (plantSimulationInstance != null) plantSimulationInstance.bugIndex = bugIndex;
            if (Bugging != null) Bugging.SetActive(false);
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
                break;
            case 1:
                weatherOutput = "HEAT HAZE";
                weatherIndex = 1;
                break;
            case 2:
                weatherOutput = "TYPHOON";
                weatherIndex = 2;
                break;
        }
        if (plantSimulationInstance != null) plantSimulationInstance.weatherIndex = weatherIndex;
    }

    public void CheckSeason()
    {
        if (TimeOfDayUI.isDrySeason)
        {
            seasonOutput = "DRY SEASON";
            seasonIndex = 0;
        }
        else
        {
            seasonOutput = "Wet SEASON";
            seasonIndex = 1;
        }
        if (plantSimulationInstance != null) plantSimulationInstance.seasonIndex = seasonIndex;
    }

    public void CheckDay()
    {
        if (TimeOfDayUI.isDay)
        {
            cycleOutput = "Day";
            cycleIndex = 0;
        }
        else
        {
            cycleOutput = "Night";
            cycleIndex = 1;
        }
        if (plantSimulationInstance != null) plantSimulationInstance.dayIndex = cycleIndex;
    }

    public void IsWaterLogged()
    {
        if (WaterDuration > 0) return;
        if (!TimeOfDayUI.isDrySeason)
        {
            WaterloggedMeter += WaterFillUpRate;
        }

        if (WaterloggedMeter >= WaterloggedMax)
        {
            if (Water != null) Water.SetActive(true);
            Waterlogged = true;
            WaterCleared = true;
        }
    }

    public void DecreaseWaterlogged()
    {
        if (WaterDuration > 0)
        {
            WaterDuration -= WaterFillUpRate;
        }
    }

    [PunRPC]
    public void RPC_WaterClear()
    {
        Waterlogged = false;
        WaterloggedMeter = 0;
        WaterDuration = WaterCooldown;
        if (Water != null) Water.SetActive(false);
    }

    public void WaterClear() { photonView.RPC("RPC_WaterClear", RpcTarget.All); }

    [PunRPC]
    public void RPC_unBug()
    {
        if (!isPlanted || !_IsInfested) return;
        bugIndex = 0;
        if (plantSimulationInstance != null) plantSimulationInstance.bugIndex = bugIndex;
        if (Bugging != null) Bugging.SetActive(false);
        unBugged = true;
        _IsInfested = false;
    }

    public void unBug() { photonView.RPC("RPC_unBug", RpcTarget.All); }

    [PunRPC]
    public void RPC_RefreshPlot()
    {
        isplantable = true;
        plantableornot();
    }

    public void RefreshPlot() { photonView.RPC("RPC_RefreshPlot", RpcTarget.All); }

    [PunRPC]
    public void RPC_IsNotPlantable()
    {
        isplantable = false;
        plantableornot();
    }

    public void IsNotPlantable() { photonView.RPC("RPC_IsNotPlantable", RpcTarget.All); }

    [PunRPC]
    public void RPC_RemovePlant()
    {
        RPC_unBug();
        ResetPlot();
        plantableornot();
        IsNotPlantable();
    }

    public void RemovePlant() { photonView.RPC("RPC_RemovePlant", RpcTarget.All); }

    [PunRPC]
    public void RPC_SuperCharge()
    {
        if (!isPlanted) return;

        int super_yield = plantSimulationInstance.GetMaxYield();
        if (assignedCanon != null)
        {
            assignedCanon.AddLoad(super_yield);
        }
        ResetPlot();
        plantableornot();
        IsNotPlantable();
    }

    public void SuperCharge() { photonView.RPC("RPC_SuperCharge", RpcTarget.All); }

    [PunRPC]
    public void RPC_CommitAction(string action)
    {
        if (!isPlanted) return;
        switch (action)
        {
            case "GetWaterLogged":
                if (Water != null) Water.SetActive(true);
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
                if (plantSimulationInstance != null) plantSimulationInstance.bugIndex = bugIndex;
                unBugged = false;
                if (Bugging != null) Bugging.SetActive(true);
                _IsInfested = true;
                break;
            case "FERTILIZING":
                if (plantSimulationInstance != null) plantSimulationInstance.cropGrowth = 10;
                break;
        }
    }

    public void CommitAction(string action) { photonView.RPC("RPC_CommitAction", RpcTarget.All, action); }
}