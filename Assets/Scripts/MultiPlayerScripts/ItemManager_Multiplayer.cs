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

        int groundLayerMask = LayerMask.GetMask("Default");
        Vector2 finalScreenPos = eventData.position;

        if (assignedCam.transform.eulerAngles.z > 160f && assignedCam.transform.eulerAngles.z < 200f)
        {
            finalScreenPos.x = Screen.width - eventData.position.x;
            finalScreenPos.y = Screen.height - eventData.position.y;
        }

        Ray ray = assignedCam.ScreenPointToRay(finalScreenPos);

        // FIX: Pierce through all overlapping colliders (Soil AND Canon)
        RaycastHit[] hits = Physics.RaycastAll(ray, 5000f, groundLayerMask);
        bool actionSuccessful = false;

        // Loop 1: Look for a Canon first to ensure priority when overlapping
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Canon"))
            {
                Debug.Log($"I pierced and found a Canon: {hit.collider.gameObject.name}");
                Debug.Log("entering canons");
                CanonFire Canoner = hit.collider.GetComponent<CanonFire>();

                if (Canoner != null)
                {
                    switch (gameObject.tag)
                    {
                        case "Soil Adder":
                            AudioManager.instance.Play("SoilAddler");
                            Canoner.GetWaterLogged();
                            actionSuccessful = true;
                            break;
                        case "Shovel":
                            AudioManager.instance.Play("Shovel");
                            Canoner.RemoveLePlants();
                            actionSuccessful = true;
                            break;
                        case "Soil Tiller":
                            AudioManager.instance.Play("SoilTiller");
                            Canoner.SOILEDIT();
                            actionSuccessful = true;
                            break;
                        case "Pesticide":
                            AudioManager.instance.Play("PesticideSpray");
                            Canoner.GiveBugs();
                            actionSuccessful = true;
                            break;
                        case "Fertilizer":
                            AudioManager.instance.Play("Fertilizer");
                            Canoner.GetOld();
                            actionSuccessful = true;
                            break;
                    }

                    if (actionSuccessful)
                    {
                        maxPickTime--;
                        break; // Exit the loop since action finished
                    }
                }
            }
        }

        // Loop 2: Fallback to Soil only if no Canon was hit and used
        if (!actionSuccessful)
        {
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("Soil"))
                {
                    Debug.Log($"I hit a Soil: {hit.collider.gameObject.name}");
                    GrowthManager_Multiplayer growthScript = hit.collider.GetComponent<GrowthManager_Multiplayer>();

                    if (growthScript != null)
                    {
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

                        if (actionSuccessful)
                        {
                            maxPickTime--;
                            break; // Exit loop
                        }
                    }
                }
            }
        }

        transform.SetParent(originalParent);
        transform.position = startPos;
    }
}