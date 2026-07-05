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

    [Header("Canvas")]
    [SerializeField] private Canvas dragCanvas;

    [Header("Assigned Canon")]
    [SerializeField] public string[] canons;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        if (_itemSprite != null) GetComponent<Image>().sprite = _itemSprite;
        if (dragCanvas == null) dragCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (maxPickTime <= 0) return;
        FindFirstObjectByType<AudioManager>().Play("TapSound1");
        startPos = transform.position;
        originalParent = transform.parent;

        if (dragCanvas != null)
            transform.SetParent(dragCanvas.transform, true);

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        if (maxPickTime > 0)
        {
            Camera cam = assignedCam != null ? assignedCam : Camera.main;
            int groundLayerMask = LayerMask.GetMask("Default", "LocalFarm");

            Vector2 screenPos = eventData.position;

            // Handle 180-degree rotated camera
            if (cam != null &&
                cam.transform.eulerAngles.z > 160f &&
                cam.transform.eulerAngles.z < 200f)
            {
                screenPos.x = Screen.width - screenPos.x;
                screenPos.y = Screen.height - screenPos.y;
            }

            Ray ray;
            if (cam != null)
            {
                Vector3 viewportPoint = cam.ScreenToViewportPoint(screenPos);
                ray = cam.ViewportPointToRay(viewportPoint);
            }
            else
            {
                ray = Camera.main.ScreenPointToRay(screenPos);
            }

            RaycastHit[] hits = Physics.RaycastAll(ray, 5000f, groundLayerMask);
            bool actionSuccessful = false;

            // Loop 1: Canon priority
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("Canon"))
                {
                    CanonFire canoner = hit.collider.GetComponent<CanonFire>();
                    if (canoner != null)
                    {
                        switch (gameObject.tag)
                        {
                            case "Soil Adder":
                                AudioManager.instance.Play("SoilAddler");
                                canoner.GetWaterLogged(); actionSuccessful = true; break;
                            case "Shovel":
                                AudioManager.instance.Play("Shovel");
                                canoner.RemoveLePlants(); actionSuccessful = true; break;
                            case "Soil Tiller":
                                AudioManager.instance.Play("SoilTiller");
                                canoner.SOILEDIT(); actionSuccessful = true; break;
                            case "Pesticide":
                                AudioManager.instance.Play("PesticideSpray");
                                canoner.GiveBugs(); actionSuccessful = true; break;
                            case "Fertilizer":
                                AudioManager.instance.Play("Fertilizer");
                                canoner.GetOld(); actionSuccessful = true; break;
                        }
                        if (actionSuccessful) { maxPickTime--; break; }
                    }
                }
            }

            // Loop 2: Fallback to Soil
            if (!actionSuccessful)
            {
                foreach (RaycastHit hit in hits)
                {
                    if (hit.collider.CompareTag("Soil"))
                    {
                        GrowthManager_Multiplayer growthScript =
                            hit.collider.GetComponent<GrowthManager_Multiplayer>();
                        if (growthScript != null)
                        {
                            switch (gameObject.tag)
                            {
                                case "Soil Adder":
                                    AudioManager.instance.Play("SoilAddler");
                                    growthScript.WaterClear(); actionSuccessful = true; break;
                                case "Shovel":
                                    AudioManager.instance.Play("Shovel");
                                    growthScript.RemovePlant(); actionSuccessful = true; break;
                                case "Soil Tiller":
                                    AudioManager.instance.Play("SoilTiller");
                                    growthScript.RefreshPlot(); actionSuccessful = true; break;
                                case "Pesticide":
                                    AudioManager.instance.Play("PesticideSpray");
                                    growthScript.unBug(); actionSuccessful = true; break;
                                case "Fertilizer":
                                    AudioManager.instance.Play("Fertilizer");
                                    growthScript.SuperCharge(); actionSuccessful = true; break;
                            }
                            if (actionSuccessful) { maxPickTime--; break; }
                        }
                    }
                }
            }
        }

        // Always snap back
        transform.SetParent(originalParent, true);
        transform.position = startPos;
    }
}