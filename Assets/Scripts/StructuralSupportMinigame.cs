using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StructuralSupportMinigame : MinigameBase
{
    [Header("Settings")]
    [SerializeField] private int totalPrompts = 7;
    [SerializeField] private float overallDuration = 20f;
    [SerializeField] private float promptDuration = 2.5f;
    [SerializeField] private float swipeThreshold = 60f;

    [Header("UI")]
    [SerializeField] private Image arrowImage;
    [SerializeField] private Image promptTimerBar;
    [SerializeField] private TMP_Text overallTimerText;
    [SerializeField] private TMP_Text progressText;

    [Header("Arrow Rotations")]
    private static readonly float[] ArrowRotations = { 0f, 180f, 90f, 270f };

    [Header("Dirt Sprite")]
    [SerializeField] private Sprite[] soilplot;
    [SerializeField] private float soilStatus;
    [SerializeField] private Image soilImage;

    [Header("Faux Plant Sprite")]
    [SerializeField] private Image fakePlantImage;

    private int _promptsLeft;
    private int _currentDirection;
    private float _overallTimeLeft;
    private float _promptTimeLeft;

    private Vector2 _swipeStart;
    private bool _swiping;

    private void OnEnable()
    {
        ResetMinigame();
    }

    private void ResetMinigame()
    {
        ResetGame();
        _promptsLeft = totalPrompts;
        _overallTimeLeft = overallDuration;
        soilStatus = 1f;

        if (resultText != null) resultText.gameObject.SetActive(false);
        if (instructionText != null) instructionText.text = "Swipe in the direction of the arrow!";

        if (CurrentPlot != null && CurrentPlot.plantRenderer != null && fakePlantImage != null)
            fakePlantImage.sprite = CurrentPlot.plantRenderer.sprite;

        NextPrompt();
        RefreshUI();
    }

    private void Update()
    {
        if (GameOver) return;

        _overallTimeLeft -= Time.deltaTime;
        _promptTimeLeft -= Time.deltaTime;
        changeSoil();

        if (overallTimerText)
            overallTimerText.text = Mathf.CeilToInt(Mathf.Max(_overallTimeLeft, 0f)).ToString();

        if (promptTimerBar)
            promptTimerBar.fillAmount = Mathf.Clamp01(_promptTimeLeft / promptDuration);

        if (_overallTimeLeft <= 0f)
        {
            EndGame(false);
            return;
        }

        if (_promptTimeLeft <= 0f) NextPrompt();

        DetectSwipe();
    }

    private void changeSoil()
    {
        if (soilplot == null || soilplot.Length == 0 || soilImage == null) return;

        soilStatus -= Time.deltaTime;
        int currentPlotIndex = Mathf.FloorToInt(soilStatus);
        currentPlotIndex = Mathf.Clamp(currentPlotIndex, 0, soilplot.Length - 1);
        soilImage.sprite = soilplot[currentPlotIndex];
    }

    private void NextPrompt()
    {
        _currentDirection = Random.Range(0, 4);
        _promptTimeLeft = promptDuration;

        if (arrowImage)
            arrowImage.transform.localRotation = Quaternion.Euler(0f, 0f, ArrowRotations[_currentDirection]);
    }

    private void DetectSwipe()
    {
        Vector2 inputPos = Input.touchCount > 0 ? (Vector2)Input.GetTouch(0).position : (Vector2)Input.mousePosition;

        bool pressed = Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
        bool released = Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended);

        if (pressed) { _swipeStart = inputPos; _swiping = true; }

        if (released && _swiping)
        {
            _swiping = false;
            Vector2 delta = inputPos - _swipeStart;

            if (delta.magnitude < swipeThreshold) return;

            int dir;
            if (Mathf.Abs(delta.y) >= Mathf.Abs(delta.x))
                dir = delta.y > 0 ? 0 : 1;
            else
                dir = delta.x < 0 ? 2 : 3;

            if (dir == _currentDirection)
            {
                _promptsLeft--;
                soilStatus += 2f; // keeps the soil status healthy
                RefreshUI();

                if (_promptsLeft <= 0)
                {
                    EndGame(true);
                }
                else NextPrompt();
            }
            else
            {
                NextPrompt();
            }
        }
    }

    private void RefreshUI()
    {
        if (progressText)
            progressText.text = $"{totalPrompts - _promptsLeft} / {totalPrompts}";
    }

    protected override string GetWinMessage() => "Soil mounded!";
    protected override string GetLoseMessage() => "Not fast enough!";
}