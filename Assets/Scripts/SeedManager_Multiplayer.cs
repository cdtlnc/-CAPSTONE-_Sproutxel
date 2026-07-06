using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SeedManager_Multiplayer : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public SeedData seedType;

    private CanvasGroup canvasGroup;
    private GameObject dragIconInstance;
    private RectTransform dragIconRect;
    private Canvas parentCanvas;

    [Header("Seed Availability")]
    [SerializeField] public TextMeshProUGUI seedNum;
    [SerializeField] public int available;
    [SerializeField] public Camera playerCam;

    [Header("Canvas")]
    [SerializeField] private Canvas dragCanvas;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        if (seedType != null) GetComponent<Image>().sprite = seedType.seedBagIcon;
        parentCanvas = GetComponentInParent<Canvas>();
        seedType.remainingSeedBags = available;
        if (dragCanvas == null) dragCanvas = parentCanvas;
    }

    private void FixedUpdate()
    {
        if (seedNum != null) seedNum.text = "" + seedType.remainingSeedBags;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (seedType == null) return;

        AudioManager audioMan = FindFirstObjectByType<AudioManager>();
        if (audioMan != null) audioMan.Play("TapSound1");

        dragIconInstance = new GameObject("SeedDragGhost");

        Transform canvasTransform = dragCanvas != null
            ? dragCanvas.transform
            : (parentCanvas != null ? parentCanvas.transform : transform.root);

        dragIconInstance.transform.SetParent(canvasTransform, false);

        dragIconRect = dragIconInstance.AddComponent<RectTransform>();
        dragIconRect.anchorMin = new Vector2(0.5f, 0.5f);
        dragIconRect.anchorMax = new Vector2(0.5f, 0.5f);
        dragIconRect.pivot = new Vector2(0.5f, 0.5f);
        dragIconRect.sizeDelta = new Vector2(90f, 90f);
        dragIconRect.localScale = Vector3.one;
        dragIconRect.rotation = transform.rotation;
        dragIconRect.position = transform.position;

        Image ghostImage = dragIconInstance.AddComponent<Image>();
        ghostImage.sprite = seedType.seedBagIcon;
        ghostImage.raycastTarget = false;

        CanvasGroup ghostGroup = dragIconInstance.AddComponent<CanvasGroup>();
        ghostGroup.alpha = 0.6f;
        canvasGroup.alpha = 0.4f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIconRect == null) return;

        Canvas c = dragCanvas ?? parentCanvas;
        if (c == null) return;

        if (c.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            dragIconRect.position = eventData.position;
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                c.transform as RectTransform,
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

        Camera cam = playerCam != null ? playerCam : Camera.main;

        if (cam != null && seedType != null)
        {
            // Include LocalFarm layer so soil colliders are hit
            int layerMask = LayerMask.GetMask("Default", "LocalFarm");

            Vector3 viewportPoint = cam.ScreenToViewportPoint(eventData.position);
            Ray ray = cam.ViewportPointToRay(viewportPoint);

            if (Physics.Raycast(ray, out RaycastHit hit, 5000f, layerMask))
            {
                if (hit.collider.CompareTag("Soil"))
                {
                    GrowthManager_Multiplayer plot =
                        hit.collider.GetComponent<GrowthManager_Multiplayer>();
                    if (plot != null)
                    {
                        Debug.Log("[SeedManager] Planting seed.");
                        plot.PlantSeed(seedType);
                    }
                }
            }
        }

        // Always destroy ghost — runs whether raycast hit or missed
        if (dragIconInstance != null)
        {
            Destroy(dragIconInstance);
            dragIconInstance = null;
            dragIconRect = null;
        }
    }
}