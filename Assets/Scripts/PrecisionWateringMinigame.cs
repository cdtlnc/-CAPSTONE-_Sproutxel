using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

    private void Start()
    {
        _timeLeft = gameDuration;
        waterFillImage.fillAmount = 0f;
        resultText.gameObject.SetActive(false);
        instructionText.text = "Hold to fill - keep the level in the green zone when time runs out!";
        PositionGreenZone();
    }

    private void Update()
    {
        if (GameOver) return;

        // Input — works for both mouse and touch
        bool holding = Input.GetMouseButton(0) ||
                       (Input.touchCount > 0 && Input.GetTouch(0).phase != TouchPhase.Ended);

        _fillAmount += (holding ? fillRate : -drainRate) * Time.deltaTime;
        _fillAmount = Mathf.Clamp01(_fillAmount);
        waterFillImage.fillAmount = _fillAmount;

        _timeLeft -= Time.deltaTime;
        if (timerText != null)
            timerText.text = Mathf.CeilToInt(Mathf.Max(_timeLeft, 0f)).ToString();

        if (_timeLeft <= 0f)
        {
            bool won = _fillAmount >= zoneMin && _fillAmount <= zoneMax;
            EndGame(won);
        }
    }

    protected override string GetWinMessage() => "Great watering!";
    protected override string GetLoseMessage() => _fillAmount > zoneMax ? "Too much water!" : "Not enough water!";

    // Sizes and positions the green zone image relative to the bar at runtime
    private void PositionGreenZone()
    {
        RectTransform barRect = waterFillImage.transform.parent.GetComponent<RectTransform>();
        RectTransform zoneRect = greenZoneImage.rectTransform;
        float h = barRect.rect.height;

        zoneRect.anchorMin = new Vector2(0f, 0f);
        zoneRect.anchorMax = new Vector2(1f, 0f);
        zoneRect.pivot = new Vector2(0.5f, 0f);
        zoneRect.anchoredPosition = new Vector2(0f, zoneMin * h);
        zoneRect.sizeDelta = new Vector2(0f, (zoneMax - zoneMin) * h);
    }
}
