using TMPro;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class TimeOfDayUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text dayNightText;
    [SerializeField] private TMP_Text clockText;

    [Header("Visual Settings")]
    [SerializeField] private Color dayColor = Color.yellow;
    [SerializeField] private Color nightColor = Color.purple;

    [Header("Time Settings")]
    [SerializeField] private float timeScale = 8f; // in-game minutes per real second, change this to adjust day length
    [SerializeField] private int startHour = 5;    // What hour of the day to start
    [SerializeField] private int seasonTimer = 1; // How many days will pass before the seasons change. I set it to 1 for testing purposes. Feel free to tinker with it as you wish.


    [Header("Directional Light")]
    [SerializeField] public Light thelight;
    [SerializeField] private Vector3 sunriseRotation = new Vector3(0f, 0f, 0f);

    // equal to timeCycleTickTimer * _TICK_TIMER_MAX in TickManager
    // timeCycleTickTimer = 2, _TICK_TIMER_MAX = 0.2 -> 2 * 0.2 = 0.4
    // If you change timeCycleTickTimer in TickManager, change this as well to match
    private const float TICK_INTERVAL = 0.4f;

    private float currentTime; // stores time in minutes (0 - 1440) 0 = 12:00 AM, 720 = 12:00 PM
    private float daysPassed = 0;

    public static bool isDrySeason { get; private set; } // This variable will be used to communicate to other scripts what season it is.
    public static bool isDay { get; private set; }       // This variable will be used to communicate to other scripts what time of day it is.
    private void UpdateLightRotation()
    {
        // 1. Calculate minutes passed since 6:00 AM (360 minutes)
        float minutesSinceSunrise = currentTime - 360f;

        // 2. Wrap negative values for the night cycle
        if (minutesSinceSunrise < 0)
        {
            minutesSinceSunrise += 1440f;
        }

        // 3. Convert time to 360 degrees (1440 mins total = 360 degrees)
        // This ensures 12 hours later (720 mins) is exactly 180 degrees
        float currentAngle = (minutesSinceSunrise / 1440f) * 360f;

        // 4. Set absolute rotation along the local right axis
        thelight.transform.localRotation = Quaternion.Euler(sunriseRotation) * Quaternion.AngleAxis(currentAngle, Vector3.right);
    }
    private void Start()
    {
        
        isDrySeason = true; // THIS SETS THE SEASON TO DRY BY DEFAULT. IF YOU WANT THE LEVEL TO START IN WET SEASON, CHANGE THIS VALUE.
        currentTime = startHour * 60f;

        UpdateLightRotation();

        TickManager.OnTimeCycleTick += TickManager_OnTimeCycleTick;
    }

    private void OnDestroy()
    {
        TickManager.OnTimeCycleTick -= TickManager_OnTimeCycleTick;
    }

    private void TickManager_OnTimeCycleTick(object sender, TickManager.OnTickEventArgs e)
    {
        // Advance time by the fixed tick interval * scale
        currentTime += TICK_INTERVAL * timeScale;
        UpdateLightRotation();
        if (currentTime >= 1440f)
        {
            currentTime -= 1440f;
            daysPassed++;

        }

        UpdateUI();

        if (daysPassed >= seasonTimer)
        {
            daysPassed = 0;
            UpdateSeasons();
        }

        Debug.Log($"Current Time: {currentTime:0} | Days Passed: {daysPassed} | Dry Season? {isDrySeason}");
    }

    private void UpdateSeasons()
    {
        isDrySeason = !isDrySeason;
    }

    private void UpdateUI() // syncs UI with current time
    {
        int hours24 = Mathf.FloorToInt(currentTime / 60f);      // converts minutes to hours (0 - 23), FloorToInt truncates decimal to avoid rounding issues

        bool isPM = hours24 >= 12;          // determines AM/PM
        int hours12 = hours24 % 12;         // converts to 12-hour format
        if (hours12 == 0) hours12 = 12;

        clockText.text = $"{hours12}{(isPM ? "pm" : "am")}";

        if (hours24 >= 6 && hours24 < 18)       // Daytime is from 6:00 AM to 5:59 PM
        {
            isDay = true;
            dayNightText.text = "Day";
            dayNightText.color = dayColor;
        }
        else
        {
            isDay = false;
            dayNightText.text = "Night";
            dayNightText.color = nightColor;
        }
    }
}