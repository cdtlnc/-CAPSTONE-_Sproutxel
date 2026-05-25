using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MinigameManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    //public SeedData seedType;
    [Header("Visual Components")]

    [SerializeField] private Vector3 startPos;
    [SerializeField] private Transform originalParent;
    [SerializeField] private CanvasGroup canvasGroup;

    void Awake()
    {

        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
  
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        startPos = transform.position;
        originalParent = transform.parent;
        transform.SetParent(GameObject.Find("PlaceholderUI").transform);
        canvasGroup.blocksRaycasts = false; // Essential for the raycast to see the soil
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData) { transform.position = eventData.position; }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        Ray ray = Camera.main.ScreenPointToRay(eventData.position);
        if (Physics.Raycast(ray, out RaycastHit hit, 5000f))
        {
            if (hit.collider.CompareTag("PlantView"))
            {
                Debug.Log("Itouchathe plant view");
            }
        }
        transform.SetParent(originalParent);
        transform.position = startPos;
    }
}