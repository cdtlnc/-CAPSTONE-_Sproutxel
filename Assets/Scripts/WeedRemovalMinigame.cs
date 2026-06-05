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


    [Header("Weed Removal Paramaterers")]
    [SerializeField] private float plantExclusionRadius = 150f; // Minimum distance from center
    [SerializeField] private int maxSpawnAttempts = 10;
    [SerializeField] private int _weedsLeft;
    [SerializeField] private int _plantTaps;
    [SerializeField] private float _timeLeft;
    [SerializeField] private readonly List<GameObject> _weeds = new List<GameObject>();

    [Header("Weeding Visuals")]
    [SerializeField] private Sprite WeedTexture;    
    [SerializeField] public Image exclusionVisualizer;
// <-- Add your line sprite here
    

    private void OnEnable()
    {
      ResetMinigame();

      
    }
    private void ResetMinigame()
    {
        ResetGame();
        _weedsLeft = totalWeeds;
        _plantTaps = 0;
        _timeLeft = gameDuration;
        resultText.gameObject.SetActive(false);
        instructionText.text = "Tap the weeds — not the plant!";
        SetupVisualizer();

        SpawnWeeds();
        RefreshUI();
    }

    private void Update()
    {
        SetupVisualizer();
        if (GameOver) return;

        _timeLeft -= Time.deltaTime;
        if (timerText) timerText.text = Mathf.CeilToInt(Mathf.Max(_timeLeft, 0f)).ToString();

        if (_timeLeft <= 0f)
            EndGame(false);
    }

    private void SpawnWeeds()
    {
        RectTransform plantRT = plantButton.GetComponent<RectTransform>();

        for (int i = 0; i < totalWeeds; i++)
        {
            var weed = new GameObject($"Weed_{i}", typeof(RectTransform), typeof(Image), typeof(Button));
            weed.transform.SetParent(weedContainer, false);

            var rt = weed.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(weedSize, weedSize);

            Vector2 randomPos = Vector2.zero;
            bool positionIsInsidePlant = true;
            int attempts = 0;

            do
            {
                // 1. Roll a random position inside the weed container
                randomPos = new Vector2(
                    Random.Range(-weedContainer.rect.width * 0.4f, weedContainer.rect.width * 0.4f),
                    Random.Range(-weedContainer.rect.height * 0.4f, weedContainer.rect.height * 0.4f)
                );

                // 2. Convert this container position to the Plant Button's local coordinate space
                Vector3 worldPos = weedContainer.TransformPoint(randomPos);
                Vector2 positionInPlantSpace = plantRT.InverseTransformPoint(worldPos);

                // 3. Check if the point falls directly inside the plant button's Rect bounds
                // We use plantExclusionRadius as extra padding around the rectangle edges
                float padding = plantExclusionRadius;
                Rect paddedRect = new Rect(
                    plantRT.rect.x - padding,
                    plantRT.rect.y - padding,
                    plantRT.rect.width + (padding * 2f),
                    plantRT.rect.height + (padding * 2f)
                );

                positionIsInsidePlant = paddedRect.Contains(positionInPlantSpace);
                attempts++;
            }
            while (positionIsInsidePlant && attempts < maxSpawnAttempts);

            rt.anchoredPosition = randomPos;

            Image img = weed.GetComponent<Image>();
            img.sprite = WeedTexture;
            img.color = Color.white;

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
            if (CurrentPlot != null)
            {
                CurrentPlot.winMinigame();
                Invoke(nameof(DisableThisPanel), 5f);
                EndGame(true);
            }
 
            else
            {
                Debug.LogWarning("Minigame ended, but no CurrentPlot reference was passed!");
            }
           
           
        }
            
    }
    private void DisableThisPanel()
    {
        Debug.Log("Disabling Weed Removal Minigame");
        transform.parent.gameObject.SetActive(false);
    }

    private void OnPlantTapped()
    {
        if (GameOver) return;

        _plantTaps++;
        RefreshUI();

        if (_plantTaps >= maxPlantTaps)
        {
            CurrentPlot.LoseMinigame();
            Invoke(nameof(DisableThisPanel), 10f);
            EndGame(false);
        }
            
    }
    private void SetupVisualizer()
    {
        if (exclusionVisualizer == null) return;

        RectTransform plantRT = plantButton.GetComponent<RectTransform>();
        RectTransform visualizerRT = exclusionVisualizer.GetComponent<RectTransform>();

        // Matches your exact SpawnWeeds paddedRect math
        float paddedWidth = plantRT.rect.width + (plantExclusionRadius * 2f);
        float paddedHeight = plantRT.rect.height + (plantExclusionRadius * 2f);

        visualizerRT.sizeDelta = new Vector2(paddedWidth, paddedHeight);
        visualizerRT.position = plantRT.position;
    }
    private void RefreshUI()
    {
        if (plantTapText)
            plantTapText.text = $"Plant taps remaining: {maxPlantTaps - _plantTaps}";
    }

    protected override string GetWinMessage() => "Weeds cleared!";
    
    protected override string GetLoseMessage() => _plantTaps >= maxPlantTaps ? "You damaged the plant!" : "Too slow!";
}
