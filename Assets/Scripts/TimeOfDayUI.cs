using TMPro;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class TimeOfDayUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] public TMP_Text dayNightText;
    [SerializeField] public TMP_Text clockText;

    [Header("Visual Settings")]
    [SerializeField] public Color dayColor = Color.yellow;
    [SerializeField] public Color nightColor = Color.purple;

    [Header("Time Settings")]
    [SerializeField] private float timeScale = 8f; // in-game minutes per real second, change this to adjust day length
    [SerializeField] private int startHour = 5;    // What hour of the day to start
    [SerializeField] private int seasonTimer = 1; // How many days will pass before the seasons change. I set it to 1 for testing purposes. Feel free to tinker with it as you wish.


    [Header("Directional Light")]
    [SerializeField] public Light thelight;
    private Vector3 sunriseRotation = new Vector3(0f, 90f, -90f);

    [Header("Set Colors")]
    [SerializeField] private Color nightColorLight = new Color(1f, 0.5f, 0f);
    [SerializeField] private Color dayColorLight = new Color(1f, 0.5f, 0f);
    [SerializeField] private Color twilightColor = new Color(1f, 0.5f, 0f);

    // equal to timeCycleTickTimer * _TICK_TIMER_MAX in TickManager
    // timeCycleTickTimer = 2, _TICK_TIMER_MAX = 0.2 -> 2 * 0.2 = 0.4
    // If you change timeCycleTickTimer in TickManager, change this as well to match
    [Header("Tick Timer")]
    [SerializeField] private float TICK_INTERVAL = 0.4f;

    private float currentTime; // stores time in minutes (0 - 1440) 0 = 12:00 AM, 720 = 12:00 PM
    private float daysPassed = 0;

    [Header("Season Initialization")]
    [SerializeField] public bool startWithDrySeason = true;
    [SerializeField] public bool dontchangeSeasons = true;
    public static bool isDrySeason { get; private set; } // This variable will be used to communicate to other scripts what season it is.
    public static bool isDay { get; private set; }       // This variable will be used to communicate to other scripts what time of day it is.

    private void UpdateLightRotation()
    {
        if (thelight == null) return;

        float minutesSinceSunrise = currentTime - 360f;

        if (minutesSinceSunrise < 0)
        {
            minutesSinceSunrise += 1440f;
        }


        float currentAngle = (minutesSinceSunrise / 1440f) * 360f;


        thelight.transform.localRotation = Quaternion.Euler(sunriseRotation) * Quaternion.AngleAxis(currentAngle, Vector3.up);
    }
    private void Start()
    {
        isDrySeason = startWithDrySeason;
        // THIS SETS THE SEASON TO DRY BY DEFAULT. IF YOU WANT THE LEVEL TO START IN WET SEASON, CHANGE THIS VALUE.
        currentTime = startHour * 60f;

        UpdateLightRotation();

        TickManager.OnTimeCycleTick += TickManager_OnTimeCycleTick;
    }

    private void OnDestroy()
    {
        TickManager.OnTimeCycleTick -= TickManager_OnTimeCycleTick;
    }
    public void GetTickSpeed(float tick_speed)
    {
        TICK_INTERVAL = tick_speed;
    }
    private void TickManager_OnTimeCycleTick(object sender, TickManager.OnTickEventArgs e)
    {
        // Advance time by the fixed tick interval * scale
        currentTime += TICK_INTERVAL * timeScale;
        UpdateLightRotation();
        UpdateLightColor();
        if (currentTime >= 1440f)
        {
            currentTime -= 1440f;
            daysPassed++;

        }

        UpdateUI();

        if (daysPassed >= seasonTimer)
        {
            daysPassed = 0;
            if (dontchangeSeasons == true)
                UpdateSeasons();
        }

        //Debug.Log($"Current Time: {currentTime:0} | Days Passed: {daysPassed} | Dry Season? {isDrySeason}");
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

        if (clockText != null)
        {
            clockText.text = $"{hours12}{(isPM ? "pm" : "am")}";
        }

        if (hours24 >= 6 && hours24 < 18)       // Daytime is from 6:00 AM to 5:59 PM
        {
            isDay = true;

            if (dayNightText != null)
            {
                dayNightText.text = "Day";
                dayNightText.color = dayColor;
            }
        }
        else
        {
            isDay = false;

            if (dayNightText != null)
            {
                dayNightText.text = "Night";
                dayNightText.color = nightColor;
            }
        }

        //Debug.Log($"{hours12}{(isPM ? "pm" : "am")}");
    }



    public void setDay()
    {

    }
    public void setNight()
    {

    }
    private void UpdateLightColor()
    {
        if (thelight == null) return;

        // Peaks at 1.0 during noon, drops to 0.0 at midnight
        float lerpPercent = Mathf.Sin((currentTime / 1440f) * Mathf.PI);

        if (lerpPercent > 0.5f)
        {
            // Upper half of the curve: Lerp between Twilight and Day
            float segmentPercent = (lerpPercent - 0.5f) / 0.5f;
            thelight.color = Color.Lerp(twilightColor, dayColorLight, segmentPercent);
        }
        else
        {
            // Lower half of the curve: Lerp between Night and Twilight
            float segmentPercent = lerpPercent / 0.5f;
            thelight.color = Color.Lerp(nightColorLight, twilightColor, segmentPercent);
        }
    }
}