// NettingProtectionMinigame.cs
// Category: Fixed
// Anchor points are arranged around the plant image.
// The player taps and drags to draw lines connecting them in order.
// Connecting all anchors before time runs out = win.
// Timer runs out with anchors unconnected = lose.
//
// ── Scene setup ──────────────────────────────────────────────────────────────
// Manager GameObject   → NettingProtectionMinigame component
// Canvas
//   PlantImage         → Image  (the plant in the centre — visual only)
//   AnchorContainer    → RectTransform  (anchor buttons are children of this)
//     Anchor_0..N      → Button + Image  (circular buttons placed around plant)
//   LineContainer      → RectTransform  (drawn lines placed here)
//   TimerText          → TMP_Text
//   InstructionText    → TMP_Text
//   ResultText         → TMP_Text  (inactive by default)
//
// Anchor positions: place them manually in a rough circle around PlantImage.
// They don't need to be exactly circular — any arrangement works.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NettingProtectionMinigame : MinigameBase
{
    [Header("Settings")]
    [SerializeField] private float gameDuration = 20f;

    [Header("UI")]
    [SerializeField] private RectTransform anchorContainer;  // parent of anchor buttons
    [SerializeField] private RectTransform lineContainer;    // where drawn lines appear
    [SerializeField] private TMP_Text timerText;

    [Header("Netting Parameters")]
    [SerializeField] private float _timeLeft;
    [SerializeField] private List<RectTransform> _anchors = new List<RectTransform>();
    [SerializeField] private int _nextAnchor = 0;   // index of the anchor to connect to next
    [SerializeField] private bool _dragging = false;
    [SerializeField] private Vector2 _dragStart;
    [SerializeField] private GameObject _activeLine;              // the line currently being drawn
    [SerializeField] private int anchorcount;              // the line currently being drawn

     [Header("Netting Visuals")]
    [SerializeField] private Sprite lineTexture;             // <-- Add your line sprite here
    [SerializeField] private float lineWidth = 20f;  
    // All completed line GameObjects (kept for visual)
    private List<GameObject> _lines = new List<GameObject>();

    private void Start()
    {
        _timeLeft = gameDuration;

        // Collect all anchor buttons in order
        foreach (RectTransform child in anchorContainer)
            _anchors.Add(child);

        if (_anchors.Count == 0)
        {
            Debug.LogWarning("[NettingProtection] No anchors found in AnchorContainer.");
            CurrentPlot.LoseMinigame();
            Invoke(nameof(DisableThisPanel), 5f);
            EndGame(false);
            return;
        }

        HighlightNextAnchor();

        resultText.gameObject.SetActive(false);
        instructionText.text = "Connect all the anchor points around the plant!";
    }

    private void Update()
    {
        if (GameOver) return;

        _timeLeft -= Time.deltaTime;
        if (timerText) timerText.text = Mathf.CeilToInt(Mathf.Max(_timeLeft, 0f)).ToString();
        if (_timeLeft <= 0f) { EndGame(false); return; }

        HandleDraw();
    }
    private void DisableThisPanel()
    {
        Debug.Log("Disabling NettingProtection Minigame");
        transform.parent.gameObject.SetActive(false);
    }
    private void HandleDraw()
    {
        Vector2 screenPos = Input.touchCount > 0
            ? (Vector2)Input.GetTouch(0).position
            : (Vector2)Input.mousePosition;

        bool pressed = Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
        bool held = Input.GetMouseButton(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved);
        bool released = Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended);

        // Must start drag on the current target anchor
        if (pressed && !_dragging)
        {
            RectTransform target = _anchors[_nextAnchor];
            if (RectTransformUtility.RectangleContainsScreenPoint(target, screenPos))
            {
                _dragging = true;
                _dragStart = screenPos;
                _activeLine = CreateLine(screenPos, screenPos);
            }
        }

        // Update live line while dragging
        if (held && _dragging && _activeLine != null)
            UpdateLine(_activeLine, _dragStart, screenPos);

        // On release, check if player reached the next anchor
        if (released && _dragging)
        {
            _dragging = false;

            int nextIdx = (_nextAnchor + 1) % _anchors.Count;
            RectTransform dest = _anchors[nextIdx];

            if (RectTransformUtility.RectangleContainsScreenPoint(dest, screenPos))
            {
                // Snap line to exact anchor positions
                UpdateLine(_activeLine, GetAnchorScreenPos(_anchors[_nextAnchor]), GetAnchorScreenPos(dest));
                _lines.Add(_activeLine);
                _activeLine = null;

                _nextAnchor = nextIdx;
                HighlightNextAnchor();

                // All anchors connected = win
                if (_nextAnchor == 0 && _lines.Count >= _anchors.Count)
                {
                    CurrentPlot.winMinigame();
                    Invoke(nameof(DisableThisPanel), 5f);
                    EndGame(true);
                    
                }
                    
            }
            else
            {
                // Missed — destroy the incomplete line
                if (_activeLine != null) Destroy(_activeLine);
                _activeLine = null;
            }
        }
    }

    // Creates a line image between two screen points
    private GameObject CreateLine(Vector2 from, Vector2 to)
    {
        var line = new GameObject("Line", typeof(RectTransform), typeof(Image));
        line.transform.SetParent(lineContainer, false);

        Image img = line.GetComponent<Image>();
        img.sprite = lineTexture;
        img.color = Color.white; // Keeps original texture colors intact
        img.type = Image.Type.Simple; // Use Sliced or Tiled depending on your asset

        UpdateLine(line, from, to);
        return line;
    }

    private void UpdateLine(GameObject line, Vector2 fromScreen, Vector2 toScreen)
    {
        RectTransform rt = line.GetComponent<RectTransform>();

        // Convert screen positions to local canvas positions
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            lineContainer, fromScreen, null, out Vector2 fromLocal);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            lineContainer, toScreen, null, out Vector2 toLocal);

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
        return (corners[0] + corners[2]) / 2f; // centre of the anchor
    }

    private void HighlightNextAnchor()
    {
        for (int i = 0; i < _anchors.Count; i++)
        {
            var img = _anchors[i].GetComponent<Image>();
            if (img == null) continue;
            // Bright yellow = next target, white = others
            img.color = (i == _nextAnchor) ? Color.yellow : Color.white;
        }
    }

    protected override string GetWinMessage() => "Plant protected!";
    protected override string GetLoseMessage() => "Netting incomplete!";
}
