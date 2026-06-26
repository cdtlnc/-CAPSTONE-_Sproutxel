using UnityEngine;
using UnityEngine.UI;

public class EventManager : MonoBehaviour
{
    [Header("Base Event Odds")]
    [SerializeField] private int[] _weatherEventOdds = new int[3];
    [Header("Infestation Chances")]
    [SerializeField] private int _infestationOdds;
    [SerializeField] private int _infestationChances;
    [SerializeField] private bool _infestationStart;

    [SerializeField] private bool dontChangeWeather;
    [SerializeField] private bool dontGetInfested;
    [SerializeField] private bool StartOnTyphoon;
    [SerializeField] private bool StartOnHeatDaze;


    [Header("Weather Parameters")]
    [SerializeField] private int _weatherDuration = 20;
    [SerializeField] private int _weatherCooldown = 10;
    [SerializeField] private int _infestationCooldown = 10;
    [SerializeField] private int _weatherDurationTimer = 0;
    [SerializeField] private int _weatherCooldownTimer = 0;
    [SerializeField] private int _infestationTimer = 0;

    [Header("Weather Meters")]
    [SerializeField] private int _HeatWaveTimer = 0;
    [SerializeField] private int _HeatWaveDuration = 0;
    [SerializeField] private int _TyphoonDuration = 0;
    [SerializeField] private int _TyphoonTimer = 0;

    [Header("Weather Chances")]
    [SerializeField] private int _HeatWaveChance = 0;
    [SerializeField] private int _TyphoonChance = 0;

    [Header("UI Weather Changes")]
    [SerializeField] public Sprite[] weatherIcons;
    [SerializeField] public Image weatherIcon;

    [Header("UI Weather Panels")]
    [SerializeField] private GameObject HeatDazeContainer;
    [SerializeField] private GameObject TyphoonContinaer;
    [SerializeField] private GameObject RainyContinaer;
    [SerializeField] private Image WeatherText;
    [SerializeField] private Sprite[] WeatherTextSprites;

    public static bool isInfested { get; private set; } = false;
    public static int _weatherEvent { get; private set; } = 0;



    void Start()
    {
        isInfested = _infestationStart;
        TickManager.OnEventTick += delegate (object sender, TickManager.OnTickEventArgs e)
        {
            //Debug.Log("Tick: " + e.tick);
        };

        TickManager.OnEventTick += TickManager_OnEventTick;

        UpdateEssentialsUI();

        if (StartOnHeatDaze)
        {
            _weatherEvent = 1; // Typhoon
        }
        if (StartOnTyphoon)
        {
            _weatherEvent = 2; // Typhoon
        }
    }

    void TickManager_OnEventTick(object sender, TickManager.OnTickEventArgs e)
    {
        if (_weatherEvent == 0)
        {
            _weatherCooldownTimer++;
        }
        else
        {
            _weatherDurationTimer++;
        }

        _infestationTimer++;

        if (_weatherDurationTimer >= _weatherDuration)
        {
            if (dontChangeWeather) return;
            Debug.Log("Weather event over!");
            _weatherDurationTimer = 0;
            _weatherEvent = 0;
        }

        // --- FIXED: Removed the old blocky override logic that was breaking weather states every 5 seconds ---
        if (!dontChangeWeather)
        {
            if (_weatherCooldownTimer >= _weatherCooldown)
            {
                Debug.Log("Weather event cooldown over!");
                _weatherCooldownTimer = 0;
                CalcWeatherEventOdds(TimeOfDayUI.isDrySeason);
                RollForWeatherEvent();
            }

          
        }
        if (!dontGetInfested)
        {
            if (_infestationTimer >= _infestationCooldown)
            {
                Debug.Log("Bug infestation cooldown over!");
                _infestationTimer = 0;
                CalcInfestationOdds();
                RollForBugInfestations();
            }
        }
      

        // Update the screen state once at the end of the tick loop
        UpdateEssentialsUI();
    }

    void CalcWeatherEventOdds(bool isDrySeason)
    {
        if (isDrySeason)
        {
            _weatherEventOdds[1] += _HeatWaveChance;
            _weatherEventOdds[2] -= _TyphoonChance;
        }
        else
        {
            _weatherEventOdds[1] -= _HeatWaveChance;
            _weatherEventOdds[2] += _TyphoonChance;
        }

        for (int i = 0; i < _weatherEventOdds.Length; i++)
        {
            // Fixed assignment clamping bug
            _weatherEventOdds[i] = Mathf.Clamp(_weatherEventOdds[i], 0, 50);
        }
    }

    void CalcInfestationOdds()
    {
        switch (_weatherEvent)
        {
            case 1:
                _infestationOdds += _infestationChances;
                break;
            case 2:
                _infestationOdds -= _infestationChances;
                break;
        }
    }

    void RollForWeatherEvent()
    {
        System.Random randomizer = new System.Random();
        int roll = randomizer.Next(0, 100);

        if (roll >= 0 && roll <= _weatherEventOdds[1])
        {
            _weatherEvent = 1; // Heat wave
        }
        else if (roll >= _weatherEventOdds[1] + 1 && roll <= _weatherEventOdds[1] + _weatherEventOdds[2])
        {
            _weatherEvent = 2; // Typhoon
        }
        else
        {
            _weatherEvent = 0; // Clear weather
        }
    }

    void RollForBugInfestations()
    {
        System.Random randomizer = new System.Random();
        int roll = randomizer.Next(0, 100);

        if (roll >= 0 && roll <= _infestationOdds) {
            
            isInfested = true;
            Debug.Log("INFESTING TIME!");
        
        }
        else isInfested = false;
    }

    // --- NEW ESSENTIALS UI CONTROLLER ---
    // --- NEW ESSENTIALS UI CONTROLLER ---
    // --- NEW ESSENTIALS UI CONTROLLER ---
    private void UpdateEssentialsUI()
    {
        GameObject activeContainer = null;
        int targetIconIndex = 0;
        int targetText = 2;

        // 1. Evaluate state based on weather events and seasonal conditions
        if (_weatherEvent == 2) // Typhoon Event active
        {
            activeContainer = TyphoonContinaer; // Main event identifier
            targetIconIndex = 1;                // Typhoon/Storm Icon
            targetText = 1;
            AudioManager.instance.Play("Typhoon");
            AudioManager.instance.Stop("HeatDaze");
            AudioManager.instance.Stop("SproutxelBGMusic");
        }
        else if (_weatherEvent == 1) // Heat Wave Event active
        {
            activeContainer = HeatDazeContainer;
            targetIconIndex = 0;
            targetText = 0; // Sunny Icon
            AudioManager.instance.Play("HeatDaze");
            AudioManager.instance.Stop("Typhoon");
            AudioManager.instance.Stop("SproutxelBGMusic");
        }
        else // Base weather clear (Checks seasonal state)
        {
            AudioManager.instance.Stop("HeatDaze");
            AudioManager.instance.Stop("Typhoon");
            AudioManager.instance.Play("SproutxelBGMusic");
            if (!TimeOfDayUI.isDrySeason) // Wet Season active (!isDrySeason)
            {
                activeContainer = RainyContinaer;
                targetIconIndex = 1;              // Rainy Icon
            }
            else // Dry Season active
            {
                activeContainer = HeatDazeContainer;
                targetIconIndex = 0;              // Sunny/Clear Icon
            }
        }

        // 2. Safely swap the UI icon image
        if (weatherIcon != null && weatherIcons != null && targetIconIndex < weatherIcons.Length)
        {
            weatherIcon.sprite = weatherIcons[targetIconIndex];
            WeatherText.sprite = WeatherTextSprites[targetText];
        }

        // 3.MULTI - PANEL ACTIVATION RULES:
        // Fixed: Heat Daze now ONLY turns on if a heat wave event is actively running
        if (HeatDazeContainer != null)
        {
            HeatDazeContainer.SetActive(_weatherEvent == 1);
           
        }

        // Typhoon panel overlay turns on ONLY when a typhoon event is actively rolling
        if (TyphoonContinaer != null)
        {
            TyphoonContinaer.SetActive(_weatherEvent == 2);
   

        }

        // Rainy panel is strictly forced active if it's the Wet Season OR during a Typhoon
        if (RainyContinaer != null)
        {
            bool isWetSeason = !TimeOfDayUI.isDrySeason;
            bool isTyphoon = (_weatherEvent == 2);

            RainyContinaer.SetActive(isWetSeason || isTyphoon);
        }
    }



    // Keep old references so external calls from other scripts do not break compilation
    public void ChangeIcon(int change) { }
    public void WeatherEventPanel(int w) { }
}
