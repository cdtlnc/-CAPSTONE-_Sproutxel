using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PestControlMinigame : MinigameBase
{
    [Header("Settings")]
    [SerializeField] private int totalPests = 8;
    [SerializeField] private int mistakeLimit = 3;
    [SerializeField] private float beatInterval = 1.2f;  // seconds per beat
    [SerializeField] private float tapWindow = 0.35f; // valid tap window either side of beat

    [Header("UI")]
    [SerializeField] private Button pestButton;
    [SerializeField] private Image beatIndicator;
    [SerializeField] private TMP_Text mistakeText;
    [SerializeField] private TMP_Text pestCountText;

    private int _pestsLeft;
    private int _mistakes;
    private float _beatTimer;
    private bool _tappedThisBeat;

    private void Start()
    {
        _pestsLeft = totalPests;
        _mistakes = 0;
        _beatTimer = 0f;
        _tappedThisBeat = false;

        pestButton.onClick.AddListener(OnPestTapped);
        resultText.gameObject.SetActive(false);
        instructionText.text = "Tap the pest on the beat!";
        RefreshUI();
    }

    private void Update()
    {
        if (GameOver) return;

        _beatTimer += Time.deltaTime;

        // beat indicator
        float t = Mathf.PingPong(_beatTimer / beatInterval * 2f, 1f);
        beatIndicator.transform.localScale = Vector3.one * Mathf.Lerp(0.85f, 1.15f, t);

        if (_beatTimer >= beatInterval)
        {
            if (!_tappedThisBeat)
                AddMistake("Missed the beat!");

            _beatTimer -= beatInterval;
            _tappedThisBeat = false;
        }
    }

    private void OnPestTapped()
    {
        if (GameOver) return;

        // distance to the nearest beat
        float distToBeat = Mathf.Min(_beatTimer, beatInterval - _beatTimer);

        if (distToBeat <= tapWindow)
        {
            _tappedThisBeat = true;
            _pestsLeft--;
            RefreshUI();

            if (_pestsLeft <= 0)
                EndGame(true);
        }
        else
        {
            AddMistake("Off beat!");
        }
    }

    private void AddMistake(string reason)
    {
        _mistakes++;
        Debug.Log($"[PestControl] Mistake ({reason}): {_mistakes}/{mistakeLimit}");
        RefreshUI();

        if (_mistakes >= mistakeLimit)
            EndGame(false);
    }

    private void RefreshUI()
    {
        if (mistakeText) mistakeText.text = $"Mistakes: {_mistakes} / {mistakeLimit}";
        if (pestCountText) pestCountText.text = $"Pests left: {_pestsLeft}";
    }

    protected override string GetWinMessage() => "Pests cleared!";
    protected override string GetLoseMessage() => "Too many mistakes!";
}
