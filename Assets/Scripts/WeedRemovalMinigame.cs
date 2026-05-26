using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeedRemovalMinigame : MinigameBase
{
    [Header("Settings")]
    [SerializeField] private int totalWeeds = 6;
    [SerializeField] private int maxPlantTaps = 3;
    [SerializeField] private float gameDuration = 15f;
    [SerializeField] private float weedSize = 60f;

    [Header("UI")]
    [SerializeField] private Button plantButton;
    [SerializeField] private RectTransform weedContainer;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text plantTapText;

    private int _weedsLeft;
    private int _plantTaps;
    private float _timeLeft;
    private readonly List<GameObject> _weeds = new List<GameObject>();

    private void Start()
    {
        _weedsLeft = totalWeeds;
        _plantTaps = 0;
        _timeLeft = gameDuration;

        plantButton.onClick.AddListener(OnPlantTapped);
        resultText.gameObject.SetActive(false);
        instructionText.text = "Tap the weeds — not the plant!";

        SpawnWeeds();
        RefreshUI();
    }

    private void Update()
    {
        if (GameOver) return;

        _timeLeft -= Time.deltaTime;
        if (timerText) timerText.text = Mathf.CeilToInt(Mathf.Max(_timeLeft, 0f)).ToString();

        if (_timeLeft <= 0f)
            EndGame(false);
    }

    private void SpawnWeeds()
    {
        for (int i = 0; i < totalWeeds; i++)
        {
            // weed placeholder
            var weed = new GameObject($"Weed_{i}", typeof(RectTransform), typeof(Image), typeof(Button));
            weed.transform.SetParent(weedContainer, false);

            var rt = weed.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(weedSize, weedSize);
            rt.anchoredPosition = new Vector2(
                Random.Range(-weedContainer.rect.width * 0.4f, weedContainer.rect.width * 0.4f),
                Random.Range(-weedContainer.rect.height * 0.4f, weedContainer.rect.height * 0.4f)
            );

            weed.GetComponent<Image>().color = new Color(0.2f, 0.6f, 0.15f);

            var btn = weed.GetComponent<Button>();
            var captured = weed;
            btn.onClick.AddListener(() => OnWeedTapped(captured));

            _weeds.Add(weed);
        }
    }

    private void OnWeedTapped(GameObject weed)
    {
        if (GameOver) return;

        weed.SetActive(false);
        _weeds.Remove(weed);
        _weedsLeft--;
        RefreshUI();

        if (_weedsLeft <= 0)
        {
            Invoke(nameof(DisableThisPanel), 10f);
            EndGame(true);
           
        }
            
    }
    private void DisableThisPanel()
    {
        gameObject.SetActive(false);
    }

    private void OnPlantTapped()
    {
        if (GameOver) return;

        _plantTaps++;
        RefreshUI();

        if (_plantTaps >= maxPlantTaps)
            EndGame(false);
    }

    private void RefreshUI()
    {
        if (plantTapText)
            plantTapText.text = $"Plant taps remaining: {maxPlantTaps - _plantTaps}";
    }

    protected override string GetWinMessage() => "Weeds cleared!";
    protected override string GetLoseMessage() => _plantTaps >= maxPlantTaps ? "You damaged the plant!" : "Too slow!";
}
