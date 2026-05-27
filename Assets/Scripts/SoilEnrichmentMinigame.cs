using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SoilEnrichmentMinigame : MinigameBase
{
    [Header("Settings")]
    [SerializeField] private int totalItems = 8;
    [SerializeField] private float gameDuration = 20f;
    [SerializeField] private float swipeThreshold = 80f;  // pixels to register as a swipe

    [Header("UI")]
    [SerializeField] private RectTransform itemContainer;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text remainingText;

    private float _timeLeft;
    private int _itemsLeft;

    // Track each item: is it organic, current rect, drag state
    private class ItemEntry
    {
        public bool isOrganic;
        public RectTransform rect;
        public bool dragging;
        public Vector2 dragStartInput;
        public Vector2 dragStartPos;
    }

    private readonly List<ItemEntry> _items = new List<ItemEntry>();
    private ItemEntry _activeItem = null; // the item currently being dragged

    private void Start()
    {
        _timeLeft = gameDuration;
        _itemsLeft = totalItems;

        resultText.gameObject.SetActive(false);
        instructionText.text = "Swipe GREEN items DOWN into soil. Swipe GREY items UP to discard.";

        SpawnItems();
        RefreshUI();
    }

    private void Update()
    {
        if (GameOver) return;

        _timeLeft -= Time.deltaTime;
        if (timerText) timerText.text = Mathf.CeilToInt(Mathf.Max(_timeLeft, 0f)).ToString();
        if (_timeLeft <= 0f) { EndGame(false); return; }

        HandleInput();
    }

    private void SpawnItems()
    {
        for (int i = 0; i < totalItems; i++)
        {
            bool organic = i < totalItems / 2;

            var obj = new GameObject(organic ? $"Organic_{i}" : $"Inorganic_{i}",
                typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(itemContainer, false);

            var rt = obj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(70f, 70f);
            rt.anchoredPosition = new Vector2(
                Random.Range(-120f, 120f),
                Random.Range(-80f, 80f)
            );

            // Green = organic, grey = inorganic
            obj.GetComponent<Image>().color = organic
                ? new Color(0.3f, 0.75f, 0.25f)
                : new Color(0.55f, 0.55f, 0.6f);

            _items.Add(new ItemEntry { isOrganic = organic, rect = rt });
        }
    }

    private void HandleInput()
    {
        Vector2 screenPos = Input.touchCount > 0
            ? (Vector2)Input.GetTouch(0).position
            : (Vector2)Input.mousePosition;

        bool pressed = Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began);
        bool held = Input.GetMouseButton(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved);
        bool released = Input.GetMouseButtonUp(0) || (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended);

        // Pick up an item
        if (pressed && _activeItem == null)
        {
            foreach (var item in _items)
            {
                if (RectTransformUtility.RectangleContainsScreenPoint(item.rect, screenPos))
                {
                    _activeItem = item;
                    _activeItem.dragging = true;
                    _activeItem.dragStartInput = screenPos;
                    _activeItem.dragStartPos = item.rect.anchoredPosition;
                    break;
                }
            }
        }

        // Drag
        if (held && _activeItem != null)
        {
            Vector2 delta = screenPos - _activeItem.dragStartInput;
            _activeItem.rect.anchoredPosition = _activeItem.dragStartPos + delta;
        }

        // Release — check swipe direction
        if (released && _activeItem != null)
        {
            float verticalDelta = screenPos.y - _activeItem.dragStartInput.y;
            bool swipedDown = verticalDelta < -swipeThreshold;
            bool swipedUp = verticalDelta > swipeThreshold;

            bool correct = (_activeItem.isOrganic && swipedDown) ||
                           (!_activeItem.isOrganic && swipedUp);

            if (correct)
            {
                _activeItem.rect.gameObject.SetActive(false);
                _items.Remove(_activeItem);
                _itemsLeft--;
                RefreshUI();

                if (_itemsLeft <= 0)
                {
                    Invoke(nameof(DisableThisPanel), 5f);
                    EndGame(true);
                   
                }

            }
            else
            {
                // Snap back to original position if wrong direction
                _activeItem.rect.anchoredPosition = _activeItem.dragStartPos;
            }

            _activeItem.dragging = false;
            _activeItem = null;
        }
    }

    private void RefreshUI()
    {
        if (remainingText) remainingText.text = $"Items left: {_itemsLeft}";
    }
    private void DisableThisPanel()
    {
        Debug.Log("Disabling Soil Enrichment Minigame");
        transform.parent.gameObject.SetActive(false);
    }

    protected override string GetWinMessage() => "Soil enriched!";
    protected override string GetLoseMessage() => "Not enough materials sorted!";
}
