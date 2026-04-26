using TMPro;
using UnityEngine;

public class TimeOfDayUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text dayNightText;
    [SerializeField] private TMP_Text clockText;

    [Header("Visual Settings")]
    [SerializeField] private Color dayColor = Color.yellow;
    [SerializeField] private Color nightColor = Color.purple;

    [Header("Time Settings")]
    [SerializeField] private float timeScale = 3f; // 60 = 1 real second = 1 in-game minute (set to 3 is [3 IN-GAME SECONDS])
    [SerializeField] private int startHour = 5;    // What hour of the day to start
    [SerializeField] private int seasonTimer = 1; // How many days will pass before the seasons change. I set it to 1 for testing purposes. Feel free to tinker with it as you wish.

    private float currentTime; // stores time in minutes (0 - 1440) 0 = 12:00 AM, 720 = 12:00 PM
    private float daysPassed = 0;

    public static bool isDrySeason { get; private set; } // This variable will be used to communicate to other scripts what season it is.
    public static bool isDay { get; private set; }       // This variable will be used to communicate to other scripts what time of day it is.

    void Start()
    {
        isDrySeason = true; // THIS SETS THE SEASON TO DRY BY DEFAULT. IF YOU WANT THE LEVEL TO START IN WET SEASON, CHANGE THIS VALUE.

        currentTime = startHour * 60f; // convert start hour to minutes, only happens once on scene load

        TickManager.OnTimeCycleTick += delegate (object sender, TickManager.OnTickEventArgs e)
        {
            Debug.Log("Tick: " + e.tick);
        };

        TickManager.OnTimeCycleTick += TickManager_UpdateTime;
    }

    void Update()
    {
        
    }

    void TickManager_UpdateTime(object sender, TickManager.OnTickEventArgs e)
    {
        currentTime += Time.deltaTime * timeScale;   // Advance in-game time
        if (currentTime >= 1440f)                   // Loop after 24 hours
        {
            currentTime -= 1440f;
            daysPassed++;
        }

        if (daysPassed >= seasonTimer)
        {
            daysPassed = 0;
            UpdateSeasons();
        }

        Debug.Log("Current Time: " + currentTime + "| Days Passed: " + daysPassed + " | Dry Season? " + isDrySeason);

        UpdateUI();         
    }

    void UpdateSeasons()
    {
        isDrySeason = !isDrySeason;
    }

    void UpdateUI() // syncs UI with current time
    {
        int hours24 = Mathf.FloorToInt(currentTime / 60f);      // converts minutes to hours (0 - 23), FloorToInt truncates decimal to avoid rounding issues

        bool isPM = hours24 >= 12;          // determines AM/PM
        int hours12 = hours24 % 12;         // converts to 12-hour format
        if (hours12 == 0)
        {
            hours12 = 12;
        }

        clockText.text = hours12 + (isPM ? "pm" : "am");

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
