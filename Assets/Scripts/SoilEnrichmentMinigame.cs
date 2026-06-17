using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;

public class SoilEnrichmentMinigame : MinigameBase
{
    [Header("Settings")]
    [SerializeField] private int totalItems = 8;
    [SerializeField] private float gameDuration = 20f;
    [SerializeField] private float swipeThreshold = 80f;

    [Header("UI")]
    [SerializeField] private RectTransform itemContainer;
    [SerializeField] private TMP_Text timerText;
    [SerializeField] private TMP_Text remainingText;

    [Header("Netting Visuals")]
    [SerializeField] private Sprite[] SoilTextures;
    [SerializeField] private Sprite[] TrashTextures;

    [Header("SoilEnrichment Parameters")]
    [SerializeField] private float _timeLeft;
    [SerializeField] private float ItemSize;
    [SerializeField] private float XField;
    [SerializeField] private float YField;
    [SerializeField] private int _itemsLeft;

    private class ItemEntry
    {
        public bool isOrganic;
        public RectTransform rect;
        public bool dragging;
        public Vector2 dragStartInput;
        public Vector2 dragStartPos;
    }

    private readonly List<ItemEntry> _items = new List<ItemEntry>();
    private ItemEntry _activeItem = null;

    private void OnEnable()
    {
        ResetMinigame();
        if (resultText != null) resultText.gameObject.SetActive(false);
        if (instructionText != null)
        {
            instructionText.gameObject.SetActive(true);
            instructionText.text = "Swipe GREEN items DOWN into soil. Swipe GREY items UP to discard.";
        } 
    }

    private void ResetMinigame()
    {
        ResetGame();
        _items.Clear();
        if (itemContainer)
        {
            for (int i = itemContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(itemContainer.GetChild(i).gameObject);
            }
        }
        _timeLeft = gameDuration;
        _itemsLeft = totalItems;

        SpawnItems();
        RefreshUI();
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

        HandleInput();
    }

    private void SpawnItems()
    {
        for (int i = 0; i < totalItems; i++)
        {
            bool organic = i < totalItems / 2;

            var obj = new GameObject(organic ? $"Organic_{i}" : $"Inorganic_{i}", typeof(RectTransform), typeof(Image));
            obj.transform.SetParent(itemContainer, false);

            var rt = obj.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(ItemSize, ItemSize);
            rt.anchoredPosition = new Vector2(
                Random.Range(-XField, XField),
                Random.Range(-YField, YField)
            );

            Image img = obj.GetComponent<Image>();

            if (organic)
            {
                if (SoilTextures != null && SoilTextures.Length > 0)
                {
                    img.sprite = SoilTextures[Random.Range(0, SoilTextures.Length)];
                }
                img.color = Color.white;
            }
            else
            {
                if (TrashTextures != null && TrashTextures.Length > 0)
                {
                    img.sprite = TrashTextures[Random.Range(0, TrashTextures.Length)];
                }
            }

            _items.Add(new ItemEntry { isOrganic = organic, rect = rt });
        }
    }

    private void HandleInput()
    {
        if (Pointer.current == null) return;

        Vector2 screenPos = Pointer.current.position.ReadValue();

        bool pressed = Pointer.current.press.wasPressedThisFrame;
        bool held = Pointer.current.press.isPressed;
        bool released = Pointer.current.press.wasReleasedThisFrame;

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

        if (held && _activeItem != null)
        {
            Vector2 delta = screenPos - _activeItem.dragStartInput;
            _activeItem.rect.anchoredPosition = _activeItem.dragStartPos + delta;
        }

        if (released && _activeItem != null)
        {
            float verticalDelta = screenPos.y - _activeItem.dragStartInput.y;
            bool swipedDown = verticalDelta < -swipeThreshold;
            bool swipedUp = verticalDelta > swipeThreshold;

            bool correct = (_activeItem.isOrganic && swipedDown) || (!_activeItem.isOrganic && swipedUp);

            if (correct)
            {
                _activeItem.rect.gameObject.SetActive(false);
                _items.Remove(_activeItem);
                _itemsLeft--;
                AudioManager.instance.Play("GoodItem");
                RefreshUI();

                if (_itemsLeft <= 0)
                {
                    EndGame(true);
                    Invoke(nameof(DisableThisPanel), 1f);
                }
            }
            else
            {
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

    protected override string GetWinMessage() => "Soil enriched!";
    protected override string GetLoseMessage() => "Not enough materials sorted!";

    private void DisableThisPanel()
    {
        Debug.Log("Disabling SoilEnrichment Minigame");
        transform.parent.gameObject.SetActive(false);
    }
}