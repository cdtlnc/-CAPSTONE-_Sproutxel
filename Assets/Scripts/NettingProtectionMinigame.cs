using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class NettingProtectionMinigame : MinigameBase
{
    [Header("Settings")]
    [SerializeField] private float gameDuration = 20f;

    [Header("UI")]
    [SerializeField] private RectTransform anchorContainer;
    [SerializeField] private RectTransform lineContainer;
    [SerializeField] private TMP_Text timerText;

    [Header("Anchor Pool / Difficulty")]
    [SerializeField] private int minAnchorCount = 4;
    [SerializeField] private int maxAnchorCount = 6;

    [Header("Random Ring Placement")]
    [SerializeField] private RectTransform ringCenter;
    [SerializeField] private float ringRadius = 220f;
    [SerializeField] private float radiusJitter = 40f;
    [SerializeField] private float angleJitter = 15f;

    [Header("Netting Parameters")]
    [SerializeField] private float _timeLeft;
    [SerializeField] private List<RectTransform> _allAnchors = new List<RectTransform>();
    [SerializeField] private List<RectTransform> _activeAnchors = new List<RectTransform>();
    [SerializeField] private int _nextAnchor = 0;
    [SerializeField] private bool _dragging = false;
    [SerializeField] private Vector2 _dragStart;
    [SerializeField] private GameObject _activeLine;

    [Header("Netting Visuals")]
    [SerializeField] private Sprite lineTexture;
    [SerializeField] private float lineWidth = 20f;

    private List<GameObject> _lines = new List<GameObject>();

    private void OnEnable()
    {
        CancelInvoke(nameof(DisableThisPanel));
        ResetMinigame();

        if (_allAnchors.Count == 0)
        {
            Debug.LogWarning("[NettingProtection] No anchors found in AnchorContainer.");
            EndGame(false);
            return;
        }

        PickActiveAnchorSubset();
        RandomizeAnchorPositions();
        HighlightNextAnchor();

        if (resultText != null) resultText.gameObject.SetActive(false);

        if (instructionText != null)
        {
            instructionText.gameObject.SetActive(true);
            instructionText.text = "Connect the anchor points in order!";
        }
    }

    private void ResetMinigame()
    {
        ResetGame();
        _timeLeft = gameDuration;
        _nextAnchor = 0;
        _dragging = false;

        _allAnchors.Clear();
        foreach (RectTransform child in anchorContainer)
            _allAnchors.Add(child);

        if (lineContainer)
        {
            for (int i = lineContainer.transform.childCount - 1; i >= 0; i--)
                Destroy(lineContainer.transform.GetChild(i).gameObject);
        }
        _lines.Clear();
        _activeAnchors.Clear();
    }

    private void PickActiveAnchorSubset()
    {
        int count = Random.Range(minAnchorCount, maxAnchorCount + 1);
        count = Mathf.Clamp(count, 1, _allAnchors.Count);

        _activeAnchors.Clear();

        for (int i = 0; i < _allAnchors.Count; i++)
        {
            bool isActive = i < count;
            _allAnchors[i].gameObject.SetActive(isActive);

            if (isActive)
                _activeAnchors.Add(_allAnchors[i]);
        }

        Debug.Log($"[NettingProtection] Round using {count} anchors.");
    }

    private void RandomizeAnchorPositions()
    {
        if (ringCenter == null)
        {
            Debug.LogWarning("[NettingProtection] No Ring Center assigned — anchors will use their existing positions.");
            return;
        }

        int count = _activeAnchors.Count;

        // Build a list of evenly-spaced ring slot angles then shuffle the order in which anchors are assigned to those slots.
        List<int> slotOrder = new List<int>(count);
        for (int i = 0; i < count; i++) slotOrder.Add(i);
        ShuffleList(slotOrder);

        float baseAngle = 360f / count;
        float startOffset = Random.Range(0f, 360f);

        for (int i = 0; i < count; i++)
        {
            int slot = slotOrder[i]; // which ring position this anchor (in sequence order) gets
            float angle = startOffset + (baseAngle * slot) + Random.Range(-angleJitter, angleJitter);
            float radius = ringRadius + Random.Range(-radiusJitter, radiusJitter);

            float radians = angle * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians)) * radius;

            _activeAnchors[i].anchoredPosition = ringCenter.anchoredPosition + offset;
        }
    }

    // Fisher-Yates shuffle
    private void ShuffleList(List<int> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }

    private void DisableThisPanel()
    {
        Debug.Log("Disabling NettingProtection Minigame");
        transform.parent.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (GameOver) return;

        _timeLeft -= Time.deltaTime;
        if (timerText) timerText.text = Mathf.CeilToInt(Mathf.Max(_timeLeft, 0f)).ToString();

        if (_timeLeft <= 0f)
        {
            EndGame(false);
            Invoke(nameof(DisableThisPanel), 1f);
            return;
        }

        HandleDraw();
    }

    private void HandleDraw()
    {
        if (Pointer.current == null) return;

        Vector2 screenPos = Pointer.current.position.ReadValue();

        bool pressed = Pointer.current.press.wasPressedThisFrame;
        bool held = Pointer.current.press.isPressed;
        bool released = Pointer.current.press.wasReleasedThisFrame;

        if (pressed && !_dragging)
        {
            RectTransform target = _activeAnchors[_nextAnchor];
            if (RectTransformUtility.RectangleContainsScreenPoint(target, screenPos))
            {
                _dragging = true;
                _dragStart = screenPos;
                _activeLine = CreateLine(screenPos, screenPos);
            }
        }

        if (held && _dragging && _activeLine != null)
            UpdateLine(_activeLine, _dragStart, screenPos);

        if (released && _dragging)
        {
            _dragging = false;

            bool isLastAnchor = _nextAnchor >= _activeAnchors.Count - 1;

            if (isLastAnchor)
            {
                if (_activeLine != null) Destroy(_activeLine);
                _activeLine = null;
            }
            else
            {
                int nextIdx = _nextAnchor + 1;
                RectTransform dest = _activeAnchors[nextIdx];

                if (RectTransformUtility.RectangleContainsScreenPoint(dest, screenPos))
                {
                    UpdateLine(_activeLine, GetAnchorScreenPos(_activeAnchors[_nextAnchor]), GetAnchorScreenPos(dest));
                    _lines.Add(_activeLine);
                    _activeLine = null;
                    AudioManager.instance.Play("ConnectingAnchor");
                    _nextAnchor = nextIdx;
                    HighlightNextAnchor();

                    if (_nextAnchor >= _activeAnchors.Count - 1)
                    {
                        EndGame(true);
                        Invoke(nameof(DisableThisPanel), 1f);
                    }
                }
                else
                {
                    if (_activeLine != null) Destroy(_activeLine);
                    _activeLine = null;
                }
            }
        }
    }

    private GameObject CreateLine(Vector2 from, Vector2 to)
    {
        var line = new GameObject("Line", typeof(RectTransform), typeof(Image));
        line.transform.SetParent(lineContainer, false);

        Image img = line.GetComponent<Image>();
        img.sprite = lineTexture;
        img.color = Color.white;
        img.type = Image.Type.Simple;
        AudioManager.instance.Play("DrawingTheLine");
        UpdateLine(line, from, to);
        return line;
    }

    private void UpdateLine(GameObject line, Vector2 fromScreen, Vector2 toScreen)
    {
        RectTransform rt = line.GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(lineContainer, fromScreen, null, out Vector2 fromLocal);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(lineContainer, toScreen, null, out Vector2 toLocal);

        Vector2 dir = toLocal - fromLocal;
        float length = dir.magnitude;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        rt.anchoredPosition = fromLocal + dir * 0.5f;
        rt.sizeDelta = new Vector2(length, lineWidth);
        rt.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    private Vector2 GetAnchorScreenPos(RectTransform anchor)
    {
        Vector3[] corners = new Vector3[4];
        anchor.GetWorldCorners(corners);
        return (corners[0] + corners[2]) / 2f;
    }

    private void HighlightNextAnchor()
    {
        for (int i = 0; i < _activeAnchors.Count; i++)
        {
            var img = _activeAnchors[i].GetComponent<Image>();
            if (img == null) continue;
            img.color = (i == _nextAnchor) ? Color.yellow : Color.white;
        }
    }

    protected override string GetWinMessage() => "Plant protected!";
    protected override string GetLoseMessage() => "Netting incomplete!";
}