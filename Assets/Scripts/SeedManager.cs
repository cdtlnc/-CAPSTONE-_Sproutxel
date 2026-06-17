using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SeedManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public SeedData seedType;
    [SerializeField] private Camera playerCamera;
    private CanvasGroup canvasGroup;

    private GameObject dragIconInstance;
    private RectTransform dragIconRect;
    private Image dragIconImage;
    private Canvas parentCanvas;

    [Header("Seed Availability")]
    [SerializeField] public TextMeshProUGUI seedNum;
    [SerializeField] public int available;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        if (seedType != null) GetComponent<Image>().sprite = seedType.seedBagIcon;

        parentCanvas = GetComponentInParent<Canvas>();

        // Initialize available seeds safely
        if (seedType != null)
        {
            seedType.remainingSeedBags = available;
        }
    }

    private void FixedUpdate()
    {
        if (seedType != null && seedNum != null)
        {
            seedNum.text = "" + seedType.remainingSeedBags;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (seedType == null) return;

        AudioManager.instance.Play("TapSound1");

        dragIconInstance = new GameObject("SeedDragGhost");

        // FIX 1: Safely parent to the current player's canvas so it inherits the 180-degree rotation
        if (parentCanvas != null)
        {
            dragIconInstance.transform.SetParent(parentCanvas.transform, false);
        }
        else
        {
            dragIconInstance.transform.SetParent(GameObject.FindAnyObjectByType<Canvas>().transform, false);
        }

        RectTransform sourceRect = GetComponent<RectTransform>();
        dragIconRect = dragIconInstance.AddComponent<RectTransform>();

        dragIconRect.anchorMin = new Vector2(0.5f, 0.5f);
        dragIconRect.anchorMax = new Vector2(0.5f, 0.5f);
        dragIconRect.pivot = sourceRect.pivot;

        // Uses exact screen pixels to prevent auto-layout stretching
        dragIconRect.sizeDelta = sourceRect.rect.size;
        dragIconRect.localScale = transform.lossyScale;
        dragIconRect.position = transform.position;

        dragIconImage = dragIconInstance.AddComponent<Image>();
        dragIconImage.sprite = seedType.seedBagIcon;
        dragIconImage.raycastTarget = false;

        CanvasGroup ghostGroup = dragIconInstance.AddComponent<CanvasGroup>();
        ghostGroup.alpha = 0.6f;
        canvasGroup.alpha = 0.4f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIconRect == null || parentCanvas == null) return;

        if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            dragIconRect.position = eventData.position;
        }
        else
        {
            // FIX 2: Uses parentCanvas.worldCamera instead of eventData.pressEventCamera
            // This forces Player 2's drags to respect their upside-down viewport
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                eventData.position,
                parentCanvas.worldCamera,
                out Vector2 localPoint
            );
            dragIconRect.localPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;

        Camera raycastCamera = playerCamera != null ? playerCamera : Camera.main;

        if (raycastCamera != null)
        {
            Ray ray = raycastCamera.ScreenPointToRay(eventData.position);
            if (Physics.Raycast(ray, out RaycastHit hit, 5000f))
            {
                if (hit.collider.CompareTag("Soil"))
                {
                    GrowthManager plot = hit.collider.GetComponent<GrowthManager>();
                    if (plot != null)
                    {
                        plot.PlantSeed(seedType);
                    }
                }
            }
        }

        if (dragIconInstance != null)
        {
            Destroy(dragIconInstance);
        }
    }
}