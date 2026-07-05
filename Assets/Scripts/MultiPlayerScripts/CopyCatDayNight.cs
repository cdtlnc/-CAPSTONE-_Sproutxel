using System.Net.NetworkInformation;
using TMPro;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UI;

public class CopyCatDayNight : MonoBehaviour
{
    [Header("UI Weather Changes")]
    [SerializeField] private Sprite[] weatherIcons;
    [SerializeField] private Image weatherIcon;

    [Header("UI References")]
    [SerializeField] private TMP_Text dayNightText;
    [SerializeField] private TMP_Text clockText;

    [Header("Others")]
    [SerializeField] private EventManager EventMan;
    [SerializeField] private TimeOfDayUI TimeOfDay;
    [Header("Tick Timer")]
    [SerializeField] private float TICK_INTERVAL = 0.4f;



    private void Start()
    {


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
        UpdateUI();
    }
    public void UpdateUI()
    {
        dayNightText.text = TimeOfDay.dayNightText.text;
        dayNightText.color = TimeOfDay.dayNightText.color;

        clockText.text = TimeOfDay.clockText.text;

        weatherIcon.sprite = EventMan.weatherIcon.sprite;
    }


}