using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem; // 1. ADD THIS NAMESPACE

public class PestControlMinigame : MinigameBase
{
    [Header("Settings")]
    [SerializeField] private int totalPests = 8;
    [SerializeField] private int mistakeLimit = 3;
    [SerializeField] private float beatInterval = 1.2f;
    [SerializeField] private float tapWindow = 0.35f;
    [SerializeField] private float XField;
    [SerializeField] private float YField;
    [SerializeField] private List<GameObject> bugList = new List<GameObject>();

    [Header("UI")]
    // REMOVED: pestButton field is gone since we check the pointer directly
    [SerializeField] private Image beatIndicator;
    [SerializeField] private TMP_Text mistakeText;
    [SerializeField] private TMP_Text pestCountText;
    [SerializeField] private RectTransform bugContainer;
    [SerializeField] private int bugSize;

    [Header("Assets")]
    [SerializeField] private Sprite[] bugSprite;

    private int _pestsLeft;
    private int _mistakes;
    private float _beatTimer;
    private bool _tappedThisBeat;

    private void OnEnable()
    {
        ResetMinigame();
    }

    private void ResetMinigame()
    {
        CancelInvoke(nameof(DisableThisPanel));
        ResetGame();
        _pestsLeft = totalPests;
        _mistakes = 0;
        _beatTimer = 0f;
        _tappedThisBeat = false;

        if (resultText != null) resultText.gameObject.SetActive(false);

        if (instructionText != null)
        {
            instructionText.gameObject.SetActive(true);
            instructionText.text = "Tap the pest on the beat!";
        }
        RefreshUI();
        SpawnBugs();
    }

    public void SpawnBugs()
    {
        // FIX: Clean up old bug objects from memory before clearing the list
        foreach (var bug in bugList)
        {
            if (bug != null) Destroy(bug);
        }
        bugList.Clear();

        for (int i = 0; i < totalPests; i++)
        {
            Vector2 randomPosition = new Vector2(Random.Range(-XField, XField), Random.Range(-YField, YField));

            var newBug = new GameObject($"Bug_{i}", typeof(RectTransform), typeof(Image));
            newBug.transform.SetParent(bugContainer, false);
            RectTransform rt = newBug.GetComponent<RectTransform>();
            rt.anchoredPosition = randomPosition;
            rt.sizeDelta = new Vector2(bugSize, bugSize);

            BugScript script = newBug.AddComponent<BugScript>();
            script.SetupAnimation(bugSprite);

            bugList.Add(newBug);
        }
    }

    private void Update()
    {
        if (GameOver) return;

        _beatTimer += Time.deltaTime;

        float t = Mathf.PingPong(_beatTimer / beatInterval * 2f, 1f);
        if (beatIndicator != null)
        {
            AudioManager.instance.Play("Pulse");
            beatIndicator.transform.localScale = Vector3.one * Mathf.Lerp(0.3f, 0.6f, t);
        }

        if (_beatTimer >= beatInterval)
        {
            if (!_tappedThisBeat)
            {
                AudioManager.instance.Play("Offbeatmistake2");
                AddMistake("Missed the beat!");
            }
                

            _beatTimer -= beatInterval;
            _tappedThisBeat = false;
        }

        // 2. NEW INPUT SYSTEM: Check for screen taps/clicks every frame in Update
        if (Pointer.current != null && Pointer.current.press.wasPressedThisFrame)
        {
            OnPestTapped();
            AudioManager.instance.Play("OnBeat");
        }
    }

    private void OnPestTapped()
    {
        if (GameOver) return;

        float distToBeat = Mathf.Min(_beatTimer, beatInterval - _beatTimer);

        if (distToBeat <= tapWindow)
        {
            _tappedThisBeat = true;
            _pestsLeft--;
            PopBug();
            RefreshUI();

            if (_pestsLeft <= 0)
            {
                EndGame(true);
                AudioManager.instance.Stop("Pulse");
                Invoke(nameof(DisableThisPanel), 1f);
            }
        }
        else
        {
            AddMistake("Off beat!");
         
       


            AudioManager.instance.Play("Offbeatmistake1");
        }
    }

    private void DisableThisPanel()
    {
        Debug.Log("Disabling PestControl Minigame");
        transform.parent.gameObject.SetActive(false);
    }

    private void AddMistake(string reason)
    {
        if (GameOver) return;

        _mistakes++;
        RefreshUI();

        if (_mistakes >= mistakeLimit)
        {
            EndGame(false);
            Invoke(nameof(DisableThisPanel), 1f);
        }
    }

    private void RefreshUI()
    {
        if (mistakeText) mistakeText.text = $"Mistakes: {_mistakes} / {mistakeLimit}";
        if (pestCountText) pestCountText.text = $"Pests left: {_pestsLeft}";
    }

    private void PopBug()
    {
        if (bugList.Count > 0)
        {
            int lastIndex = bugList.Count - 1;
            GameObject targetBug = bugList[lastIndex];

            if (targetBug != null)
            {
                if (targetBug.TryGetComponent<BugScript>(out BugScript bugScript))
                {
                    bugScript.Removed();
                }
                else
                {
                    Destroy(targetBug);
                }
            }
            bugList.RemoveAt(lastIndex);
        }
    }

    protected override string GetWinMessage() => "Pests cleared!";
    protected override string GetLoseMessage() => "Too many mistakes!";
}
