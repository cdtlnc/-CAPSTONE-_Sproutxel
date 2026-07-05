#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.EditorTools;
#endif

using UnityEngine;
using UnityEngine.EventSystems;
using Photon.Pun;

public class GrowthManager_Multiplayer : MonoBehaviourPun, IPointerClickHandler
{
    [Header("Network")]
    [SerializeField] public bool isLocalPlayerPlot = true;
    [SerializeField] private int plotIndex = 0;

    [Header("UI")]
    [SerializeField] private CanonFire assignedCanon;
    [SerializeField] public GameObject Untilled;
    [SerializeField] public GameObject Tilled;

    [Header("Visual Components")]
    [SerializeField] public SpriteRenderer plantRenderer;
    [SerializeField] public SpriteRenderer PlantSadBG;
    [SerializeField] public SpriteRenderer PlantSadFG;
    [SerializeField] public Sprite PlantSadBGTexture;
    [SerializeField] public Sprite PlantSadFGTexture;

    [Header("Live Growth Debugger")]
    [SerializeField] public SeedData currentSeed;
    [SerializeField] private int currentStage = 0;

    [Header("Plant Stats")]
    [SerializeField] private string name;
    [SerializeField] private float cropHP;
    [SerializeField] private bool wonMinigame, hasNoBadStats;

    [Header("Crop Moisture")][SerializeField] private float cropMoisture;
    [Header("Soil Quality")][SerializeField] private float soilQuality;
    [Header("Soil Moisture")][SerializeField] private float soilMoisture;
    [Header("Soil Softness")][SerializeField] private float soilSoftness;

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
    [SerializeField] float reductionFactor = 0.3f;
    [SerializeField] float recoveryAmountPerTick = 2f;
    [SerializeField] float closetothecenter = 40f;

    [Header("Stat Parameters")]
    [SerializeField] float minSafe = -20f;
    [SerializeField] float maxSafe = 60f;
    [SerializeField] float recoveryRate = 2f;
    [SerializeField] float targetCenter = 20f;
    [SerializeField] float BugCooldownMeter;
    [SerializeField] float BugCooldownMeterMax = 100f;
    [SerializeField] float BugCooldownRate = 10f;
    [SerializeField] bool unBugged;
    [SerializeField] bool _IsInfested;

    public BasePlant plantSimulationInstance;
    public bool isPlanted = false;

    private string _lastSentSpriteName = "";

    private void Start()
    {
        Waterlogged = false;
        if (Water != null) Water.SetActive(false);
        if (Bugging != null) Bugging.SetActive(false);
        BugCooldownMeter = 0;
        disableSadParts();
    }

    void OnEnable() { TickManager.OnPlantCalcTick += LinkToOfficialClock; }
    void OnDisable() { TickManager.OnPlantCalcTick -= LinkToOfficialClock; }

    private void LinkToOfficialClock(object sender, TickManager.OnTickEventArgs e)
    {
        HandleTick();
    }

    // GHOST HELPER

    // sends any farm state RPC to opponent GhostFarmView
    private void SendGhostState(string rpcName, params object[] args)
    {
        if (!isLocalPlayerPlot) return;
        GhostFarmView ghost = FindFirstObjectByType<GhostFarmView>();
        if (ghost == null || ghost.photonView == null) return;
        ghost.photonView.RPC(rpcName, RpcTarget.Others, args);
    }

    // TICK

    public void HandleTick()
    {
        if (!isLocalPlayerPlot) return;

        if (!isPlanted) IsWaterLogged();
        if (WaterCleared && !isplantable) DecreaseWaterlogged();
        if (unBugged) UnBugCountdown();

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

        int finalStageIndex = currentSeed.growthStages.Length - 1;
        if (finalStageIndex > 0)
        {
            float growthRatio = plantSimulationInstance.cropGrowth / 10f;
            int targetStage = Mathf.FloorToInt(growthRatio * finalStageIndex);
            currentStage = Mathf.Clamp(targetStage, 0, finalStageIndex);
        }
        else { currentStage = 0; }

        CheckWeather();
        CheckSeason();
        CheckDay();
        UpdatePlantSprite();
    }

    // HARVESTING

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isLocalPlayerPlot) return;
        if (!isPlanted || currentSeed == null || currentSeed.growthStages == null ||
            currentSeed.growthStages.Length == 0) return;

        int lastIndex = currentSeed.growthStages.Length - 1;
        int matureIndex = currentSeed.growthStages.Length - 2;

        if (currentStage == matureIndex || currentStage == lastIndex)
        {
            int yieldAmount = plantSimulationInstance.stats == null
                ? 3
                : plantSimulationInstance.GetCropYield();

            if (yieldAmount > 0)
            {
                assignedCanon.AddLoad(yieldAmount);
                IsNotPlantable();
                ResetPlot();
            }
        }
    }

    // PLANTING

    public void PlantSeed(SeedData data)
    {
        if (!isLocalPlayerPlot) return;
        if (isPlanted || data == null) return;
        if (Waterlogged || !isplantable) return;
        if (data.remainingSeedBags <= 0) return;
        if (data.growthStages == null || data.growthStages.Length == 0) return;

        currentSeed = data;
        isPlanted = true;
        currentStage = 0;
        AudioManager.instance.Play("Planting");

        plantSimulationInstance = new BasePlant();
        plantSimulationInstance.stats = data.plantStatsTemplate;
        plantSimulationInstance.growthStages = data.growthStages;

        if (data.plantStatsTemplate != null)
            plantSimulationInstance.cropHP = data.plantStatsTemplate.maxHP;

        plantSimulationInstance.cropGrowth = 0f;
        plantSimulationInstance.cropMoisture = 20f;
        plantSimulationInstance.soilMoisture = 20f;
        plantSimulationInstance.soilSoftness = 20f;
        plantSimulationInstance.soilQuality = 20f;

        data.remainingSeedBags--;
        UpdatePlantSprite();
        Debug.Log($"[GrowthManager] Planted {data.cropName}.");
    }

    // SPRITES

    private void UpdatePlantSprite()
    {
        if (plantRenderer == null || currentSeed == null ||
            currentSeed.growthStages == null ||
            currentStage >= currentSeed.growthStages.Length) return;

        Sprite sprite = currentSeed.growthStages[currentStage];
        if (sprite == null) return;

        plantRenderer.sprite = sprite;

        string spriteName = sprite.name;
        if (spriteName == _lastSentSpriteName) return;
        _lastSentSpriteName = spriteName;

        SendGhostState("RPC_UpdateGhostPlot", plotIndex, spriteName);
    }

    // PLOT RESET

    void ResetPlot()
    {
        unBug();
        isPlanted = false;
        currentSeed = null;
        plantSimulationInstance = null;
        currentStage = 0;
        _lastSentSpriteName = "";

        if (plantRenderer != null) plantRenderer.sprite = null;

        // clear ghost sprite
        SendGhostState("RPC_UpdateGhostPlot", plotIndex, "");
        // clear ghost waterlog
        SendGhostState("RPC_SetGhostWaterlogged", plotIndex, false);
        // clear ghost infestation
        SendGhostState("RPC_SetGhostBugged", plotIndex, false);

        plantableornot();
        disableSadParts();
        IsNotPlantable();
    }

    private void FixedUpdate()
    {
        if (!isPlanted || currentSeed == null || plantSimulationInstance == null)
        {
            name = "Empty Plot"; cropHP = 0f; cropMoisture = 0f;
            soilQuality = 0f; soilMoisture = 0f; soilSoftness = 0f;
            return;
        }
        name = currentSeed.cropName;
        cropHP = plantSimulationInstance.cropHP;
        cropMoisture = plantSimulationInstance.cropMoisture;
        soilQuality = plantSimulationInstance.soilQuality;
        soilMoisture = plantSimulationInstance.soilMoisture;
        soilSoftness = plantSimulationInstance.soilSoftness;
    }

    // HELPERS

    private void disableSadParts()
    {
        if (PlantSadBG != null) PlantSadBG.color = new Color(1f, 1f, 1f, 0f);
        if (PlantSadFG != null) PlantSadFG.color = new Color(1f, 1f, 1f, 0f);
    }

    public void plantableornot()
    {
        if (isplantable) { Untilled.SetActive(false); Tilled.SetActive(true); }
        else { Tilled.SetActive(false); Untilled.SetActive(true); }
    }

    // WEATHER EVENTS (NOT ASSIGNED)

    public void CheckWeather()
    {
        int idx = PhotonNetwork.IsMasterClient
            ? EventManager._weatherEvent
            : NetworkTimeState.weatherEvent;

        switch (idx)
        {
            case 0: weatherOutput = "CLEAR"; weatherIndex = 0; break;
            case 1: weatherOutput = "HEAT WAVE"; weatherIndex = 1; break;
            case 2: weatherOutput = "TYPHOON"; weatherIndex = 2; break;
        }
        if (plantSimulationInstance != null)
            plantSimulationInstance.weatherIndex = weatherIndex;
    }

    public void CheckSeason()
    {
        bool dry = PhotonNetwork.IsMasterClient
            ? TimeOfDayUI.isDrySeason
            : NetworkTimeState.isDrySeason;

        seasonIndex = dry ? 0 : 1;
        seasonOutput = dry ? "DRY SEASON" : "WET SEASON";
        if (plantSimulationInstance != null)
            plantSimulationInstance.seasonIndex = seasonIndex;
    }

    public void CheckDay()
    {
        bool day = PhotonNetwork.IsMasterClient
            ? TimeOfDayUI.isDay
            : NetworkTimeState.isDay;

        cycleIndex = day ? 0 : 1;
        cycleOutput = day ? "Day" : "Night";
        if (plantSimulationInstance != null)
            plantSimulationInstance.dayIndex = cycleIndex;
    }

    public void CheckInfestation()
    {
        bool infested = PhotonNetwork.IsMasterClient
            ? EventManager.isInfested
            : NetworkTimeState.isInfested;

        if (infested && BugCooldownMeter <= 0)
        {
            bugIndex = 1; unBugged = false;
            if (Bugging != null) Bugging.SetActive(true);
            BugCooldownMeter = BugCooldownMeterMax;
            _IsInfested = true;
            if (plantSimulationInstance != null) plantSimulationInstance.bugIndex = 1;

            // tell opponent plot is infested
            SendGhostState("RPC_SetGhostBugged", plotIndex, true);
        }
        else if (!infested)
        {
            bugIndex = 0;
            _IsInfested = false;
            if (Bugging != null) Bugging.SetActive(false);
            if (plantSimulationInstance != null) plantSimulationInstance.bugIndex = 0;
        }
    }

    // WATERLOG

    public void IsWaterLogged()
    {
        if (WaterDuration > 0) return;
        bool wet = PhotonNetwork.IsMasterClient
            ? !TimeOfDayUI.isDrySeason
            : !NetworkTimeState.isDrySeason;

        if (wet) WaterloggedMeter += WaterFillUpRate;

        if (WaterloggedMeter >= WaterloggedMax)
        {
            if (Water != null) Water.SetActive(true);
            Waterlogged = true;
            WaterCleared = true;

            // tell opponent plot is waterlogged
            SendGhostState("RPC_SetGhostWaterlogged", plotIndex, true);
        }
    }

    public void DecreaseWaterlogged()
    {
        if (WaterDuration > 0) WaterDuration -= WaterFillUpRate;
    }

    public void WaterClear()
    {
        Waterlogged = false;
        WaterloggedMeter = 0;
        WaterDuration = WaterCooldown;
        if (Water != null) Water.SetActive(false);

        // tell opponent waterlog is gone
        SendGhostState("RPC_SetGhostWaterlogged", plotIndex, false);
    }

    // BUG INFESTATION CLEAR

    public void UnBugCountdown() { BugCooldownMeter -= BugCooldownRate; }

    public void unBug()
    {
        if (!isPlanted || !_IsInfested) return;
        bugIndex = 0;
        if (plantSimulationInstance != null) plantSimulationInstance.bugIndex = 0;
        if (Bugging != null) Bugging.SetActive(false);
        unBugged = true;
        _IsInfested = false;

        // tell opponent bugs are cleared
        SendGhostState("RPC_SetGhostBugged", plotIndex, false);
    }

    // TOOLS

    public void RefreshPlot()
    {
        isplantable = true;
        plantableornot();
        // tell opponent plot is tilled
        SendGhostState("RPC_SetGhostTilled", plotIndex, true);
    }

    public void IsNotPlantable()
    {
        isplantable = false;
        plantableornot();
        // tell opponent plot is not tilled
        SendGhostState("RPC_SetGhostTilled", plotIndex, false);
    }

    public void RemovePlant() { unBug(); ResetPlot(); plantableornot(); IsNotPlantable(); }

    public void SuperCharge()
    {
        if (!isPlanted) return;
        int super_yield = plantSimulationInstance.GetMaxYield();
        assignedCanon.AddLoad(super_yield);
        RemovePlant(); plantableornot(); IsNotPlantable();
    }

    public void CommitAction(string action)
    {
        if (!isPlanted) return;
        switch (action)
        {
            case "GetWaterLogged":
                if (Water != null) Water.SetActive(true);
                Waterlogged = true;
                WaterloggedMeter = WaterloggedMax;
                SendGhostState("RPC_SetGhostWaterlogged", plotIndex, true);
                break;
            case "RemovePlants":
                ResetPlot(); break;
            case "UnTillable":
                isplantable = false;
                plantableornot();
                SendGhostState("RPC_SetGhostTilled", plotIndex, false);
                break;
            case "GETBUGGED":
                BugCooldownMeter = 0f; bugIndex = 1;
                if (plantSimulationInstance != null) plantSimulationInstance.bugIndex = 1;
                unBugged = false;
                if (Bugging != null) Bugging.SetActive(true);
                _IsInfested = true;
                SendGhostState("RPC_SetGhostBugged", plotIndex, true);
                break;
            case "FERTILIZING":
                if (plantSimulationInstance != null) plantSimulationInstance.cropGrowth = 10;
                break;
        }
    }
}