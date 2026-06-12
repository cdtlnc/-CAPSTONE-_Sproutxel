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

    [Header("Watering Can Effects")]
    [SerializeField] private GameObject WateringCan;
    [SerializeField] private Image WaterImage;
    [SerializeField] private Sprite[] Waterings;
    [SerializeField] private Vector2 canOffset = new Vector2(-60f, 0f); // X/Y position offset relative to bar
    [SerializeField] private float animationSpeed = 0.1f; // Seconds per frame for water pouring sprite sheet
    [SerializeField] private float tiltAngle = -30f;

    private float _fillAmount = 0f;
    private float _timeLeft;
    private RectTransform _barParentRect;
    private RectTransform _canRectTransform;
    private float _animationTimer;
    private int _currentFrame;

    private void OnEnable()
    {
        if (waterFillImage != null && waterFillImage.transform.parent != null)
            _barParentRect = waterFillImage.transform.parent.GetComponent<RectTransform>();

        if (WateringCan != null)
            _canRectTransform = WateringCan.GetComponent<RectTransform>();

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
        UpdateCanPosition();
        if (WaterImage != null) WaterImage.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (GameOver) return;
        FindFirstObjectByType<AudioManager>().Play("WaterFilling");

        bool holding = Pointer.current != null && Pointer.current.press.isPressed;

        _fillAmount += (holding ? fillRate : -drainRate) * Time.deltaTime;
        _fillAmount = Mathf.Clamp01(_fillAmount);
        waterFillImage.fillAmount = _fillAmount;

        // Dynamic Position Following
        UpdateCanPosition();

        // Sprite Frame Animation Sequence Logic
        AnimateWaterPour(holding);

        _timeLeft -= Time.deltaTime;

        if (timerText != null)
            timerText.text = Mathf.CeilToInt(Mathf.Max(_timeLeft, 0f)).ToString();

        if (_timeLeft <= 0f)
        {
            if (WaterImage != null) WaterImage.gameObject.SetActive(false);

            bool won = _fillAmount >= zoneMin && _fillAmount <= zoneMax;
            if (won)
            {
                EndGame(won);
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

    private void UpdateCanPosition()
    {
        if (_canRectTransform == null || _barParentRect == null) return;

        float targetY = _fillAmount * _barParentRect.rect.height;

        _canRectTransform.anchorMin = new Vector2(0.5f, 0f);
        _canRectTransform.anchorMax = new Vector2(0.5f, 0f);
        _canRectTransform.pivot = new Vector2(0.5f, 0.5f);
        _canRectTransform.anchoredPosition = new Vector2(canOffset.x, targetY + canOffset.y);

        // CHECK HOLD STATE: Rotate if pouring water, reset to straight (0) if released
        bool holding = Pointer.current != null && Pointer.current.press.isPressed;
        float currentRotation = (holding && _fillAmount < 1f) ? tiltAngle : 0f;

        _canRectTransform.localRotation = Quaternion.Euler(0f, 0f, currentRotation);
    }

    private void AnimateWaterPour(bool isHolding)
    {
        if (WaterImage == null || Waterings == null || Waterings.Length == 0) return;

        // Hide water sprite instantly when player releases control loop
        if (!isHolding || _fillAmount >= 1f)
        {
            WaterImage.gameObject.SetActive(false);
            return;
        }

        WaterImage.gameObject.SetActive(true);
        _animationTimer += Time.deltaTime;

        if (_animationTimer >= animationSpeed)
        {
            _animationTimer = 0f;
            _currentFrame = (_currentFrame + 1) % Waterings.Length;
            WaterImage.sprite = Waterings[_currentFrame];
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
