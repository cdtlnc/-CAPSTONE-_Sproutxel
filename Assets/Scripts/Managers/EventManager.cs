
using UnityEngine;
using UnityEngine.UI;

public class EventManager : MonoBehaviour
{
    [Header("Base Event Odds")]
    [SerializeField] private int[] _weatherEventOdds = new int[3]; // The odds of each weather event occuring. 1 - odds that a Heat Wave will happen, 2 - odds that a Typhoon will happen.
    [Header("Infestation Chances")]
    [SerializeField] private int _infestationOdds; // The odds that a bug infestation will occur.
    [SerializeField] private int _infestationChances; // 


    [Header("Weather Parameters")]
    [SerializeField] private int _weatherDuration = 20;     // The duration of a weather event, measured in ticks
    [SerializeField] private int _weatherCooldown = 10;     // The cooldown between weather events, measured in ticks
    [SerializeField] private int _infestationCooldown = 10; // The cooldown between bug infestations, measured in ticks
    [SerializeField] private int _weatherDurationTimer = 0;
    [SerializeField] private int _weatherCooldownTimer = 0;
    [SerializeField] private int _infestationTimer     = 0;

    [Header("Weather Meters")]
    [SerializeField] private int _HeatWaveTimer     = 0;
    [SerializeField] private int _HeatWaveDuration     = 0;
    [SerializeField] private int _TyphoonDuration     = 0;
    [SerializeField] private int _TyphoonTimer     = 0;

    [Header("Weather Chances")]
    [SerializeField] private int _HeatWaveChance = 0;
    [SerializeField] private int _TyphoonChance = 0;

    [Header("UI Weather Changes")]
    [SerializeField] private Sprite[] weatherIcons;
    [SerializeField] private Image weatherIcon;

    [Header("UI Weather Panels")]
    [SerializeField] private GameObject HeatDazeContainer;
    [SerializeField] private GameObject TyphoonContinaer;
    [SerializeField] private GameObject RainyContinaer;

    public static bool isInfested { get; private set; } = false; // This will communicate to other scripts whether or not a plant is infested
    public static int _weatherEvent { get; private set; } = 0;   // This will communicate to other scripts whether or not a typhoon or heat wave is occuring. 0 - Clear weather, 1 - Heat Wave, 2 - Typhoon


    void Start()
    {
        TickManager.OnEventTick += delegate (object sender, TickManager.OnTickEventArgs e)
        {
            Debug.Log("Tick: " + e.tick);
        };

        TickManager.OnEventTick += TickManager_OnEventTick;

        // FORCE RAIN EVENT TO TEST, REMOVE
        _weatherEvent = 2;
    }

    // Everything in this function occurs on an "Event Tick"
    void TickManager_OnEventTick(object sender, TickManager.OnTickEventArgs e)
    {
        if (_weatherEvent == 0)
        {
            _weatherCooldownTimer++; // If weather is clear, update the cooldown timer
        }
        else
        {
            _weatherDurationTimer++; // If weather event is occuring, update the duration timer
        }

        _infestationTimer++;

        Debug.Log($"Weather Cooldown Timer: {_weatherCooldownTimer} | Weather Duration Timer: {_weatherDurationTimer} | Infestation Timer: {_infestationTimer}");

        if (_weatherDurationTimer >= _weatherDuration)
        {
            Debug.Log("Weather event over!");

            _weatherDurationTimer = 0;
            _weatherEvent = 0;
        }

        if (_weatherCooldownTimer >= _weatherCooldown)
        {
            Debug.Log("Weather event cooldown over!");

            _weatherCooldownTimer = 0;
            CalcWeatherEventOdds(TimeOfDayUI.isDrySeason);
            RollForWeatherEvent();
        }

        if (_infestationTimer >= _infestationCooldown)
        {
            Debug.Log("Bug infestation cooldown over!");

            _infestationTimer = 0;
            CalcInfestationOdds();
            RollForBugInfestations();
        }
        if (TimeOfDayUI.isDrySeason)
        {
            ChangeIcon(0);
            WeatherEventPanel(1);

        }
        else
        {
            ChangeIcon(1);
            WeatherEventPanel(2);
        }
    }

    // Calculates the odds of each weather event depending on the season
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
            Mathf.Clamp(_weatherEventOdds[i], 0, 50);
        }
    }

    // Calculates the odds of infestation depending on the weather event
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

    // Decides whether or not a weather event happens given the corresponding odds
    void RollForWeatherEvent()
    { 
        // For example, if a Heat Wave's odds is set at 33 and a Typhoon's at 60...
        // A random number between 0 and 100 is picked.
        // If the number ranges from 0 to 33, a heat wave happens
        // If the number ranges from [33 + 1] to [33 + 60], a typhoon happens
        // if [33 + 60 + 1] to 100, there is no weather event
        // The code is set up so that even if the sum of the odds of a Heat Wave and a Typhoon exceed 100, the code still works. It just means it's impossible to have clear weather.

        System.Random randomizer = new System.Random();

        int roll = randomizer.Next(0, 100);
        Debug.Log($"Rolling for weather events... Heat Wave Odds: {_weatherEventOdds[1]} | Typhoon Odds: {_weatherEventOdds[2]} | Clear Weather Odds: {_weatherEventOdds[0]};");

        if (roll >= 0 && roll <= _weatherEventOdds[1])
        {
            // Heat wave
            _weatherEvent = 1;
            Debug.Log($"Player rolled {roll}, triggering a heat wave!");
            ChangeIcon(0);
            WeatherEventPanel(1);
        }
        else if (roll >= _weatherEventOdds[1] + 1 && roll <= _weatherEventOdds[1] + _weatherEventOdds[2])
        {
            // Typhoon
            _weatherEvent = 2;
            Debug.Log($"Player rolled {roll}, triggering a typhoon!");
            ChangeIcon(1);
            WeatherEventPanel(2);
        }
        else if (roll >= _weatherEventOdds[1] + _weatherEventOdds[2] + 1 && roll <= _weatherEventOdds[1] + _weatherEventOdds[2] + _weatherEventOdds[0])
        {
            // Clear weather
            _weatherEvent = 0;
            Debug.Log($"Player rolled {roll}, triggering no weather event!");
            WeatherEventPanel(3);
        }
    }

    // Decides whether or not an infestation happens given the corresponding odds
    void RollForBugInfestations()
    {
        System.Random randomizer = new System.Random();

        int roll = randomizer.Next(0, 100);
        Debug.Log($"Rolling for bug infestation... Odds: {_infestationOdds}.");

        if (roll >= 0 && roll <= _infestationOdds)
        {
            isInfested = true;
            Debug.Log($"Player rolled {roll}, triggering an infestation!");
        }
        else
        {
            isInfested = false;
            Debug.Log($"Player rolled {roll}, triggering no infestation!");
        }
    }

    public void ChangeIcon(int change)
    {
        if (change == 0)
        {
            weatherIcon.sprite = weatherIcons[change];
        }
        else
        {
            weatherIcon.sprite = weatherIcons[change];
        }
    }

    public void WeatherEventPanel(int w)
    {
        if (w == 1)
        {
            HeatDazeContainer.SetActive(true);
            TyphoonContinaer.SetActive(false);
        }
        else if (w == 2)
        {
            HeatDazeContainer.SetActive(false);
            TyphoonContinaer.SetActive(true);
        }
        else
        {
            HeatDazeContainer.SetActive(false);
            TyphoonContinaer.SetActive(false);
        }





    }
}
