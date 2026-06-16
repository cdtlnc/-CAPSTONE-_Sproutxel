using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemManager_Multiplayer : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Item")]
    [SerializeField] public int maxPickTime;
    [SerializeField] public Sprite _itemSprite;
    [SerializeField] private Vector3 startPos;
    [SerializeField] private Transform originalParent;
    [SerializeField] private CanvasGroup canvasGroup;
    [Header("Camera")]
    [SerializeField] private Camera assignedCam;

    [Header("Assigned Canon")]
    [SerializeField] public string[] canons;
 
    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        if (_itemSprite != null) GetComponent<Image>().sprite = _itemSprite;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {

        if (maxPickTime <= 0) return;
        FindFirstObjectByType<AudioManager>().Play("TapSound1");
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

        // Use a LayerMask if your plant colliders are blocking the ground raycast
        // Replace "Default" with whatever layer your Soil object uses
        int groundLayerMask = LayerMask.GetMask("Default");

        Ray ray = assignedCam.ScreenPointToRay(eventData.position);

        // Perform raycast using the layer mask to isolate the soil
        if (Physics.Raycast(ray, out RaycastHit hit, 5000f, groundLayerMask))
        {
            if (hit.collider.CompareTag("Soil"))
            {
                GrowthManager_Multiplayer growthScript = hit.collider.GetComponent<GrowthManager_Multiplayer>();

                if (growthScript != null)
                {
                    bool actionSuccessful = false;

                    switch (gameObject.tag)
                    {
                        case "Soil Adder":
                            AudioManager.instance.Play("SoilAddler");
                            growthScript.WaterClear();
                            actionSuccessful = true;
                            break;

                        case "Shovel":
                            AudioManager.instance.Play("Shovel");
                            growthScript.RemovePlant();
                            actionSuccessful = true;
                            break;

                        case "Soil Tiller":
                            AudioManager.instance.Play("SoilTiller");
                            growthScript.RefreshPlot();
                            actionSuccessful = true;
                            break;

                        case "Pesticide":
                            // Safely apply pesticide to the soil slot
                            AudioManager.instance.Play("PesticideSpray");
                            growthScript.unBug();
                            actionSuccessful = true;
                            break;

                        case "Fertilizer":
                            AudioManager.instance.Play("Fertilizer");
                            growthScript.SuperCharge();
                            actionSuccessful = true;
                            break;
                    }

                    // Only consume an item charge if a tool action actually fired
                    if (actionSuccessful)
                    {
                        maxPickTime--;
                    }
                }
            }
           
        }

        transform.SetParent(originalParent);
        transform.position = startPos;
    }

}