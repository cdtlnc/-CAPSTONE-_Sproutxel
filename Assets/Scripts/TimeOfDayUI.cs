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
    [SerializeField] private float timeScale = 3f;      // 60 = 1 real second = 1 in-game minute (set to 3 is [3 IN-GAME SECONDS])
    [SerializeField] private int startHour = 5;         // What hour of the day to start

    private float currentTime;      // stores time in minutes (0 - 1440) 0 = 12:00 AM, 720 = 12:00 PM

    void Start()
    {
        currentTime = startHour * 60f;      // convert start hour to minutes, only happens once on scene load
    }

    void Update()
    {
        currentTime += Time.deltaTime * timeScale;      // Advance in-game time
        if (currentTime >= 1440f)                   // Loop after 24 hours
        {
            currentTime -= 1440f;
        }

        UpdateUI();         // syncs UI with current time
    }

    void UpdateUI()
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
            dayNightText.text = "Day";
            dayNightText.color = dayColor;
        }
        else
        {
            dayNightText.text = "Night";
            dayNightText.color = nightColor;
        }
    }
}
