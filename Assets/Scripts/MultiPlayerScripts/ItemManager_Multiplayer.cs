using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Photon.Pun;

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

        if (FindFirstObjectByType<AudioManager>() != null)
        {
            FindFirstObjectByType<AudioManager>().Play("TapSound1");
        }

        startPos = transform.position;
        originalParent = transform.parent;

        Transform canvasTransform = GameObject.Find("GameplayCanvas")?.transform;
        if (canvasTransform != null)
        {
            transform.SetParent(canvasTransform);
        }

        canvasGroup.blocksRaycasts = false; // Essential for the raycast to see the target underneath
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

        int groundLayerMask = LayerMask.GetMask("Default");
        Vector2 finalScreenPos = eventData.position;

        // Auto-assign the camera if it's missing
        Camera activeCam = assignedCam != null ? assignedCam : Camera.main;

        if (activeCam == null)
        {
            ResetItemPosition();
            return;
        }

        // Handle viewport flip adjustments smoothly for Player 2
        if (activeCam.transform.eulerAngles.z > 160f && activeCam.transform.eulerAngles.z < 200f)
        {
            finalScreenPos.x = Screen.width - eventData.position.x;
            finalScreenPos.y = Screen.height - eventData.position.y;
        }

        Ray ray = activeCam.ScreenPointToRay(finalScreenPos);
        RaycastHit[] hits = Physics.RaycastAll(ray, 5000f, groundLayerMask);
        bool actionSuccessful = false;

        // Loop 1: Check if we dropped the item onto a CANNON
        foreach (RaycastHit hit in hits)
        {
            if (hit.collider.CompareTag("Canon"))
            {
                CanonFire Canoner = hit.collider.GetComponent<CanonFire>();
                if (Canoner != null)
                {
                    PhotonView canonPV = Canoner.GetComponent<PhotonView>();

                    // Only interact if this cannon belongs to us or is unowned in the hierarchy
                    switch (gameObject.tag)
                    {
                        case "Soil Adder":
                            if (AudioManager.instance != null) AudioManager.instance.Play("SoilAddler");
                            Canoner.GetWaterLogged(); // Internally synced via RPC now!
                            actionSuccessful = true;
                            break;
                        case "Shovel":
                            if (AudioManager.instance != null) AudioManager.instance.Play("Shovel");
                            Canoner.RemoveLePlants();
                            actionSuccessful = true;
                            break;
                        case "Soil Tiller":
                            if (AudioManager.instance != null) AudioManager.instance.Play("SoilTiller");
                            Canoner.SOILEDIT();
                            actionSuccessful = true;
                            break;
                        case "Pesticide":
                            if (AudioManager.instance != null) AudioManager.instance.Play("PesticideSpray");
                            Canoner.GiveBugs();
                            actionSuccessful = true;
                            break;
                        case "Fertilizer":
                            if (AudioManager.instance != null) AudioManager.instance.Play("Fertilizer");
                            Canoner.GetOld();
                            actionSuccessful = true;
                            break;
                    }

                    if (actionSuccessful)
                    {
                        maxPickTime--;
                        break;
                    }
                }
            }
        }

        // Loop 2: Fallback directly to SOIL PLOTS if no Cannon was targeted
        if (!actionSuccessful)
        {
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("Soil"))
                {
                    GrowthManager_Multiplayer growthScript = hit.collider.GetComponent<GrowthManager_Multiplayer>();
                    if (growthScript != null)
                    {
                        PhotonView plotPV = growthScript.GetComponent<PhotonView>();
                        if (plotPV != null)
                        {
                            // Route the direct item usage actions through the soil plot's networked views
                            switch (gameObject.tag)
                            {
                                case "Soil Adder":
                                    if (AudioManager.instance != null) AudioManager.instance.Play("SoilAddler");
                                    plotPV.RPC("RPC_WaterClear", RpcTarget.All);
                                    actionSuccessful = true;
                                    break;
                                case "Shovel":
                                    if (AudioManager.instance != null) AudioManager.instance.Play("Shovel");
                                    plotPV.RPC("RPC_RemovePlant", RpcTarget.All);
                                    actionSuccessful = true;
                                    break;
                                case "Soil Tiller":
                                    if (AudioManager.instance != null) AudioManager.instance.Play("SoilTiller");
                                    plotPV.RPC("RPC_RefreshPlot", RpcTarget.All);
                                    actionSuccessful = true;
                                    break;
                                case "Pesticide":
                                    if (AudioManager.instance != null) AudioManager.instance.Play("PesticideSpray");
                                    plotPV.RPC("RPC_unBug", RpcTarget.All);
                                    actionSuccessful = true;
                                    break;
                                case "Fertilizer":
                                    if (AudioManager.instance != null) AudioManager.instance.Play("Fertilizer");
                                    plotPV.RPC("RPC_SuperCharge", RpcTarget.All);
                                    actionSuccessful = true;
                                    break;
                            }
                        }
                        else
                        {
                            // Local fallback for offline sandbox testing
                            ExecuteLocalPlotAction(growthScript);
                            actionSuccessful = true;
                        }

                        if (actionSuccessful)
                        {
                            maxPickTime--;
                            break;
                        }
                    }
                }
            }
        }

        ResetItemPosition();
    }

    private void ResetItemPosition()
    {
        if (originalParent != null)
        {
            transform.SetParent(originalParent);
        }
        transform.position = startPos;
    }

    private void ExecuteLocalPlotAction(GrowthManager_Multiplayer growthScript)
    {
        switch (gameObject.tag)
        {
            case "Soil Adder": growthScript.WaterClear(); break;
            case "Shovel": growthScript.RemovePlant(); break;
            case "Soil Tiller": growthScript.RefreshPlot(); break;
            case "Pesticide": growthScript.unBug(); break;
            case "Fertilizer": growthScript.SuperCharge(); break;
        }
    }
}