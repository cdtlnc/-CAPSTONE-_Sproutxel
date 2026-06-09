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

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        if (seedType != null) GetComponent<Image>().sprite = seedType.seedBagIcon;

        parentCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (seedType == null) return;

        FindFirstObjectByType<AudioManager>().Play("TapSound1");

        dragIconInstance = new GameObject("SeedDragGhost");

        Transform canvasTransform = parentCanvas != null ? parentCanvas.transform : GameObject.Find("GameplayCanvas").transform;
        dragIconInstance.transform.SetParent(canvasTransform, false);

        RectTransform sourceRect = GetComponent<RectTransform>();
        dragIconRect = dragIconInstance.AddComponent<RectTransform>();

        dragIconRect.anchorMin = new Vector2(0.5f, 0.5f);
        dragIconRect.anchorMax = new Vector2(0.5f, 0.5f);
        dragIconRect.pivot = sourceRect.pivot;

        dragIconRect.sizeDelta = sourceRect.rect.size;
        dragIconRect.localScale = sourceRect.localScale;
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
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentCanvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
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