using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SeedManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public SeedData seedType;
    private CanvasGroup canvasGroup;

    // Ghost copy variables so the actual hotbar slot never moves
    private GameObject dragIconInstance;
    private Image dragIconImage;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        if (seedType != null) GetComponent<Image>().sprite = seedType.seedBagIcon;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        FindFirstObjectByType<AudioManager>().Play("TapSound1");
        // 1. Create a dummy object to act as our ghost drag visual
        dragIconInstance = new GameObject("SeedDragGhost");
        dragIconInstance.transform.SetParent(GameObject.Find("GameplayCanvas").transform, false);

        // 2. Set up its size and position to match this slot exactly
        RectTransform sourceRect = GetComponent<RectTransform>();
        RectTransform ghostRect = dragIconInstance.AddComponent<RectTransform>();
        ghostRect.sizeDelta = sourceRect.sizeDelta;
        ghostRect.position = transform.position;

        // 3. Match the seed bag icon image and make it see-through
        dragIconImage = dragIconInstance.AddComponent<Image>();
        dragIconImage.sprite = seedType != null ? seedType.seedBagIcon : null;
        dragIconImage.raycastTarget = false; // Makes it completely invisible to physics checks

        // 4. Fade out the ghost copy and the main slot slightly for visual feedback
        CanvasGroup ghostGroup = dragIconInstance.AddComponent<CanvasGroup>();
        ghostGroup.alpha = 0.6f;
        canvasGroup.alpha = 0.4f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Track the mouse cursor position using our ghost visual icon instead of the slot
        if (dragIconInstance != null)
        {
            dragIconInstance.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Restore full opacity to the real hotbar slot box
        canvasGroup.alpha = 1f;

        // Run your existing plant-on-soil raycast validation check
        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        if (Physics.Raycast(ray, out RaycastHit hit, 5000f))
        {
            if (hit.collider.CompareTag("Soil"))
            {
                hit.collider.GetComponent<GrowthManager>().PlantSeed(seedType);
            }
        }

        // 5. Clean up and vaporize the ghost drag object from memory
        if (dragIconInstance != null)
        {
            Destroy(dragIconInstance);
        }
    }
}