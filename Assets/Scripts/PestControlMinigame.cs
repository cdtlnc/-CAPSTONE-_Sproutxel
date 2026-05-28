using System.Collections.Generic;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.ShaderGraph.Internal;
#endif
using UnityEngine;
using UnityEngine.UI;

public class PestControlMinigame : MinigameBase
{
    [Header("Settings")]
    [SerializeField] private int totalPests = 8;
    [SerializeField] private int mistakeLimit = 3;
    [SerializeField] private float beatInterval = 1.2f;  // seconds per beat
    [SerializeField] private float tapWindow = 0.35f; // valid tap window either side of beat
    [SerializeField] private float XField;
    [SerializeField] private float YField;
    [SerializeField] private List<GameObject> bugList = new List<GameObject>();

    [Header("UI")]
    [SerializeField] private Button pestButton;
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
        SpawnBugs();
    }

    public void SpawnBugs()
    {
        for (int i = 0; i < totalPests; i++)
        {

            Vector2 randomPosition = new Vector2(Random.Range(-XField, XField), Random.Range(-YField, YField));

            // 2. Spawn the prefab at that position with no rotation
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

        // beat indicator
        float t = Mathf.PingPong(_beatTimer / beatInterval * 2f, 1f);
        beatIndicator.transform.localScale = Vector3.one * Mathf.Lerp(0.3f, 0.6f, t);//Need to update, too big

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
            PopBug();
            RefreshUI();

            if (_pestsLeft <= 0)
            {
                Invoke(nameof(DisableThisPanel), 10f);
                EndGame(true);

                
            }

        }
        else
        {
            AddMistake("Off beat!");
        }
    }
    private void DisableThisPanel()
        {
        Debug.Log("Disabling Pest Control Minigame");
        transform.parent.gameObject.SetActive(false);
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
    private void PopBug()
    {
        if (bugList.Count > 0)
        {
            // Get the index of the very last bug in the list safely
            int lastIndex = bugList.Count - 1;

            GameObject targetBug = bugList[lastIndex];

            if (targetBug != null)
            {
                // Execute the physical throw animation script
                if (targetBug.TryGetComponent<BugScript>(out BugScript bugScript))
                {
                    bugScript.Removed();
                }
                else
                {
                    // Fallback protection if component is missing
                    Destroy(targetBug);
                }
            }

            // Wipe it from data array tracking
            bugList.RemoveAt(lastIndex);
        }
    }

    protected override string GetWinMessage() => "Pests cleared!";
    protected override string GetLoseMessage() => "Too many mistakes!";
}
