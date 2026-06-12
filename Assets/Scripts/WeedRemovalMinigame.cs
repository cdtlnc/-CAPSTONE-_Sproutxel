using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // 1. ADD THIS NAMESPACE

public class WeedRemovalMinigame : MinigameBase
{
    [Header("Settings")]
    [SerializeField] private int totalWeeds = 6;
    [SerializeField] private int maxPlantTaps = 3;
    [SerializeField] private float gameDuration = 15f;
    [SerializeField] private float weedSize = 60f;

    [Header("UI")]
    [SerializeField] private Button plantButton; // Keeps your original button assignment intact
    [SerializeField] private RectTransform weedContainer;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text plantTapText;

    [Header("Weed Removal Parameters")]
    [SerializeField] private float plantExclusionRadius = 150f; // Minimum distance from center
    [SerializeField] private int maxSpawnAttempts = 10;
    [SerializeField] private int _weedsLeft;
    [SerializeField] private int _plantTaps;
    [SerializeField] private float _timeLeft;
    private readonly List<GameObject> _weeds = new List<GameObject>();

    [Header("Weeding Visuals")]
    [SerializeField] private Sprite WeedTexture;
    [SerializeField] public Image exclusionVisualizer;

    private void OnEnable()
    {
        ResetMinigame();
    }

    private void ResetMinigame()
    {
        CancelInvoke(nameof(DisableThisPanel));
        ResetGame();
        _weedsLeft = totalWeeds;
        _plantTaps = 0;
        _timeLeft = gameDuration;

        if (resultText != null) resultText.gameObject.SetActive(false);

        if (instructionText != null)
        {
            instructionText.gameObject.SetActive(true);
            instructionText.text = "Tap the weeds — not the plant!";
        }
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
        {
            EndGame(false);
            Invoke(nameof(DisableThisPanel), 5f);
            return;
        }

        // 2. NEW INPUT SYSTEM: Listen for taps directly every single frame
        HandleInput();
    }

    private void HandleInput()
    {
        if (Pointer.current == null || !Pointer.current.press.wasPressedThisFrame) return;
        Vector2 screenPos = Pointer.current.position.ReadValue();

        // 1. Check weed hits
        for (int i = _weeds.Count - 1; i >= 0; i--)
        {
            if (_weeds[i] == null) continue;
            RectTransform weedRt = _weeds[i].GetComponent<RectTransform>();
            if (RectTransformUtility.RectangleContainsScreenPoint(weedRt, screenPos))
            {
                OnWeedTapped(_weeds[i]);
                return;
            }
        }

        // 2. Check green visualizer zone hit instead of tiny button
        if (exclusionVisualizer != null && RectTransformUtility.RectangleContainsScreenPoint(exclusionVisualizer.rectTransform, screenPos))
        {
            FindFirstObjectByType<AudioManager>().Play("TouchaThePlant");
            OnPlantTapped();
        }
    }

    private void SpawnWeeds()
    {
        foreach (var oldWeed in _weeds) if (oldWeed != null) Destroy(oldWeed);
        _weeds.Clear();

        // Use the large green visualizer as the absolute blocking zone
        RectTransform exclusionRT = (exclusionVisualizer != null) ? exclusionVisualizer.rectTransform : plantButton.GetComponent<RectTransform>();

        // Get actual screen boundary box of the expanded WeedContainer
        Vector3[] containerCorners = new Vector3[4];
        weedContainer.GetWorldCorners(containerCorners);
        Vector2 bottomLeftLocal = weedContainer.InverseTransformPoint(containerCorners[0]);
        Vector2 topRightLocal = weedContainer.InverseTransformPoint(containerCorners[2]);

        float halfWeed = weedSize * 0.5f;
        float minX = bottomLeftLocal.x + halfWeed;
        float maxX = topRightLocal.x - halfWeed;
        float minY = bottomLeftLocal.y + halfWeed;
        float maxY = topRightLocal.y - halfWeed;

        for (int i = 0; i < totalWeeds; i++)
        {
            var weed = new GameObject($"Weed_{i}", typeof(RectTransform), typeof(Image));
            weed.transform.SetParent(weedContainer, false);

            var rt = weed.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(weedSize, weedSize);
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f); // Lock anchors to center

            Vector2 randomPos = Vector2.zero;
            bool insideExclusion = true;
            int attempts = 0;

            do
            {
                randomPos = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
                Vector3 worldPos = weedContainer.TransformPoint(randomPos);
                Vector2 posInPlantSpace = exclusionRT.InverseTransformPoint(worldPos);

                float padding = halfWeed;
                Rect paddedRect = new Rect(
                    exclusionRT.rect.x - padding, exclusionRT.rect.y - padding,
                    exclusionRT.rect.width + (padding * 2f), exclusionRT.rect.height + (padding * 2f)
                );

                insideExclusion = paddedRect.Contains(posInPlantSpace);
                attempts++;
            }
            while (insideExclusion && attempts < 100);

            rt.anchoredPosition = randomPos;
            Image img = weed.GetComponent<Image>();
            img.sprite = WeedTexture;
            _weeds.Add(weed);
        }
    }


    private void OnWeedTapped(GameObject weed)
    {
        if (GameOver) return;

        _weeds.Remove(weed);
        Destroy(weed); // Safely completely release the object asset container from memory
        _weedsLeft--;
        FindFirstObjectByType<AudioManager>().Play("PluckWeed");
        RefreshUI();

        if (_weedsLeft <= 0)
        {
            EndGame(true);
            Invoke(nameof(DisableThisPanel), 5f);
        }
    }

    private void OnPlantTapped()
    {
        if (GameOver) return;

        _plantTaps++;
        RefreshUI();

        if (_plantTaps >= maxPlantTaps)
        {
            EndGame(false);
            Invoke(nameof(DisableThisPanel), 5f);
        }
    }

    private void SetupVisualizer()
    {
        if (exclusionVisualizer == null) return;

        RectTransform plantRT = plantButton.GetComponent<RectTransform>();
        RectTransform visualizerRT = exclusionVisualizer.GetComponent<RectTransform>();

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

    private void DisableThisPanel()
    {
        Debug.Log("Disabling WeedRemoval Minigame");
        transform.parent.gameObject.SetActive(false);
    }

    protected override string GetWinMessage() => "Weeds cleared!";
    protected override string GetLoseMessage() => _plantTaps >= maxPlantTaps ? "You damaged the plant!" : "Too slow!";
}
