using UnityEngine;
using UnityEngine.EventSystems;

public class SeedManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 startPos;
    private Transform originalParent;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPos = transform.position;
        originalParent = transform.parent;
        transform.SetParent(GameObject.Find("GameplayCanvas").transform);
        transform.SetAsLastSibling();
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("1. Drag Ended!"); // If you don't see this, the UI is broken
        canvasGroup.blocksRaycasts = true;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // Increased distance significantly for your high camera
        if (Physics.Raycast(ray, out RaycastHit hit, 5000f))
        {
            Debug.Log("2. Hit Something: " + hit.collider.name);
            if (hit.collider.CompareTag("Soil"))
            {
                Debug.Log("3. Hit Soil! Planting...");
                hit.collider.GetComponent<GrowthManager>().PlantSeed();
            }
        }
        else
        {
            Debug.Log("2. Ray hit absolutely nothing in 3D space.");
        }

        transform.SetParent(originalParent);
        transform.position = startPos;
    }
}