using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemManager : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Item")]
    [SerializeField] public int maxPickTime;
    [SerializeField] public Sprite _itemSprite;
    [SerializeField] private Vector3 startPos;
    [SerializeField] private Transform originalParent;
    [SerializeField] private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        if (_itemSprite != null) GetComponent<Image>().sprite = _itemSprite;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {

        if (maxPickTime <= 0) return;

        startPos = transform.position;
        originalParent = transform.parent;
        transform.SetParent(GameObject.Find("GameplayCanvas").transform);
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
            if (hit.collider.CompareTag("Soil"))
            {
                switch (gameObject.tag)
                {
                    case "Soil Adder":
                        hit.collider.GetComponent<GrowthManager>().WaterClear();
                        break;

                    case "Shovel":
                        hit.collider.GetComponent<GrowthManager>().RemovePlant();
                        break;

                    case "Soil Tiller":
                        hit.collider.GetComponent<GrowthManager>().RefreshPlot();
                        break;

                    case "Pesticide":
                        hit.collider.GetComponent<GrowthManager>().unBug();
                        break;

                    case "Fertilizer":
                        hit.collider.GetComponent<GrowthManager>().SuperCharge();
                        break;

                    default:
                        break;
                     
                }
                    
               

            }
        }
        transform.SetParent(originalParent);
        transform.position = startPos;
        maxPickTime--;
    }
}