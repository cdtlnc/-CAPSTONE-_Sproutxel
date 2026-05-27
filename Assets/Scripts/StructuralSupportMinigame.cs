using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StructuralSupportMinigame : MinigameBase
{
    [Header("Settings")]
    [SerializeField] private int totalPrompts = 7;
    [SerializeField] private float overallDuration = 20f;
    [SerializeField] private float promptDuration = 2.5f;   // seconds to complete each swipe
    [SerializeField] private float swipeThreshold = 60f;    // pixels to register as a swipe

    [Header("UI")]
    [SerializeField] private Image arrowImage;
    [SerializeField] private Image promptTimerBar;
    [SerializeField] private TMP_Text overallTimerText;
    [SerializeField] private TMP_Text progressText;

    // Directions: 0=Up, 1=Down, 2=Left, 3=Right
    // ArrowImage is rotated to point the right way (0°=Up, 90°=Left, 180°=Down, 270°=Right)
    [Header("Arrow Rotations")]
    private static readonly float[] ArrowRotations = { 0f, 180f, 90f, 270f };

    [Header("Dirt Sprite")]
    [SerializeField] private Sprite[] soilplot;
    [SerializeField] private float soilStatus;
    [SerializeField] private Image soilImage;


    private int _promptsLeft;
    private int _currentDirection;
    private float _overallTimeLeft;
    private float _promptTimeLeft;

    private Vector2 _swipeStart;
    private bool _swiping;

    private void Start()
    {
        _promptsLeft = totalPrompts;
        _overallTimeLeft = overallDuration;
        soilStatus= 1f;

        resultText.gameObject.SetActive(false);
        instructionText.text = "Swipe in the direction of the arrow!";

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

        if (_overallTimeLeft <= 0f) { EndGame(false); return; }
        if (_promptTimeLeft <= 0f) NextPrompt(); // missed this one, move to next (no penalty for now)

        DetectSwipe();
    }
    private void changeSoil()
    {
        soilStatus -= Time.deltaTime;
        Debug.Log(soilStatus + " Soil Status ");
        int currentPlotIndex = Mathf.FloorToInt(soilStatus);
        currentPlotIndex = Mathf.Clamp(currentPlotIndex, 0, soilplot.Length - 1);
        var currentPlot = soilplot[currentPlotIndex];
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
        Vector2 inputPos = Input.touchCount > 0
            ? (Vector2)Input.GetTouch(0).position
            : (Vector2)Input.mousePosition;

        bool pressed = Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
        bool released = Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended);

        if (pressed) { _swipeStart = inputPos; _swiping = true; }

        if (released && _swiping)
        {
            _swiping = false;
            Vector2 delta = inputPos - _swipeStart;

            if (delta.magnitude < swipeThreshold) return; // too short

            // Determine dominant direction: 0=Up,1=Down,2=Left,3=Right
            int dir;
            if (Mathf.Abs(delta.y) >= Mathf.Abs(delta.x))
                dir = delta.y > 0 ? 0 : 1;
            else
                dir = delta.x < 0 ? 2 : 3;

            if (dir == _currentDirection)
            {
                _promptsLeft--;
                soilStatus += 1;
                soilStatus += 1;
                RefreshUI();

                if (_promptsLeft <= 0)
                {
                    Invoke(nameof(DisableThisPanel), 5f);
                    EndGame(true);
                    //Change Conditions when ending the minigame
                 
                } 
                else NextPrompt();
            }
            else
            {
                // Wrong direction — give a new prompt without penalty
                NextPrompt();
            }
        }
    }

    private void DisableThisPanel()
    {
        Debug.Log("Disabling Structural Support Minigame");
        transform.parent.gameObject.SetActive(false);
    }
    private void RefreshUI()
    {
        if (progressText)
            progressText.text = $"{totalPrompts - _promptsLeft} / {totalPrompts}";
    }

    protected override string GetWinMessage() => "Soil mounded!";
    protected override string GetLoseMessage() => "Not fast enough!";
}



  