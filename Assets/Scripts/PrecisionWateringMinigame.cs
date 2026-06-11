using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class PrecisionWateringMinigame : MinigameBase
{
    [Header("Bar")]
    [SerializeField] private Image waterFillImage;
    [SerializeField] private Image greenZoneImage;

    [Header("Settings")]
    [SerializeField] private float fillRate = 0.4f;   // per second while holding
    [SerializeField] private float drainRate = 0.25f;  // per second while releasing
    [SerializeField] private float gameDuration = 10f;   // total seconds before win/lose is decided
    [SerializeField] private float zoneMin = 0.50f;  // green zone bottom (0-1)
    [SerializeField] private float zoneMax = 0.75f;  // green zone top    (0-1)

    [Header("Timer UI")]
    [SerializeField] private TMP_Text timerText;

    private float _fillAmount = 0f;
    private float _timeLeft;

    private void OnEnable()
    {
        ResetMinigame();
        if (resultText != null) resultText.gameObject.SetActive(false);
       
        if (instructionText != null)
        {
            instructionText.gameObject.SetActive(true);
            instructionText.text = "Hold to fill - keep the level in the green zone when time runs out!";

        }

    }

    private void ResetMinigame()
    {
        CancelInvoke(nameof(DisableThisPanel));
        _timeLeft = gameDuration;
        _fillAmount = 0f;
        waterFillImage.fillAmount = 0f;
        ResetGame();
        PositionGreenZone();
       

    }

    private void Update()
    {
        if (GameOver) return;
        FindFirstObjectByType<AudioManager>().Play("WaterFilling");
        // 2. NEW INPUT SYSTEM: Unified holding check for both touch and mouse clicks
        bool holding = Pointer.current != null && Pointer.current.press.isPressed;

        _fillAmount += (holding ? fillRate : -drainRate) * Time.deltaTime;
        _fillAmount = Mathf.Clamp01(_fillAmount);
        waterFillImage.fillAmount = _fillAmount;

        _timeLeft -= Time.deltaTime;

       
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(Mathf.Max(_timeLeft, 0f)).ToString();

        if (_timeLeft <= 0f)
        {
            bool won = _fillAmount >= zoneMin && _fillAmount <= zoneMax;
            if (won)
            {
                EndGame(won); // Manager cleanly takes the true/false result here!
                FindFirstObjectByType<AudioManager>().Play("WinWatering");
                Invoke(nameof(DisableThisPanel), 5f);
            }
            else
            {
                EndGame(false);
                FindFirstObjectByType<AudioManager>().Stop("WaterFilling");
                Invoke(nameof(DisableThisPanel), 5f);

            }
        }
    }

    protected override string GetWinMessage() => "Great watering!";
    protected override string GetLoseMessage() => _fillAmount > zoneMax ? "Too much water!" : "Not enough water!";

    private void PositionGreenZone()
    {
        FindFirstObjectByType<AudioManager>().Play("WaterFilling");
        RectTransform barRect = waterFillImage.transform.parent.GetComponent<RectTransform>();
        RectTransform zoneRect = greenZoneImage.rectTransform;
        float h = barRect.rect.height;

        zoneRect.anchorMin = new Vector2(0f, 0f);
        zoneRect.anchorMax = new Vector2(1f, 0f);
        zoneRect.pivot = new Vector2(0.5f, 0f);
        zoneRect.anchoredPosition = new Vector2(0f, zoneMin * h);
        zoneRect.sizeDelta = new Vector2(0f, (zoneMax - zoneMin) * h);
    }
    private void DisableThisPanel()
    {
        Debug.Log("Disabling PrecisionWatering Minigame");
        transform.parent.gameObject.SetActive(false);
    }
}