using UnityEngine;
using UnityEngine.EventSystems;

public class PlantViewDrop : MonoBehaviour, IDropHandler
{
    public GrowthManager CurrentPlot { get; set; }

    [Header("Plant Minigames")]
    [SerializeField] public GameObject Watering;
    [SerializeField] public GameObject SoilEnrichment;
    [SerializeField] public GameObject PestControl;
    [SerializeField] public GameObject Structural_support;
    [SerializeField] public GameObject Weed_Manager;
    [SerializeField] public GameObject Netting;

    [Header("Minigame Scripts")]
    [SerializeField] public PrecisionWateringMinigame WateringScript;
    [SerializeField] public WeedRemovalMinigame WeedScript;
    [SerializeField] public NettingProtectionMinigame NettingScript;
    [SerializeField] public StructuralSupportMinigame StructScript;
    [SerializeField] public SoilEnrichmentMinigame SoilScript;
    [SerializeField] public PestControlMinigame pestScript;

    private void Start()
    {
        Watering.SetActive(false);
        SoilEnrichment.SetActive(false);
        PestControl.SetActive(false);
        Structural_support.SetActive(false);
        Weed_Manager.SetActive(false);
        Netting.SetActive(false);

    }
    private void Awake()
    {
        Debug.Log("Waking UP");
    }
    public void OnDrop(PointerEventData eventData)
    {
        // This fires automatically if the user releases the mouse/finger over this panel
        if (eventData.pointerDrag != null)
        {
            Debug.Log($"DROPPING {eventData.pointerDrag.name} onto {gameObject.name}!");
            string droppedObjectName = eventData.pointerDrag.name;


           
            switch (droppedObjectName)
            {

                case "Watering":
                    Debug.Log("Going to Watering");

                    // 1. Pass the active plot down to the watering minigame
                    if (Watering != null)
                    {
                        WateringScript.CurrentPlot = CurrentPlot;
                        Watering.gameObject.SetActive(true);
                    }
                    break;

                case "SoilEnrichment":
                    Debug.Log("Going to Soil Enrichment");

                    // 1. Pass the active plot down to the watering minigame
                    if (SoilEnrichment != null)
                    {
                        SoilScript.CurrentPlot = CurrentPlot;
                        SoilEnrichment.gameObject.SetActive(true);
                    }


                    break;

                case "PestControl":
                    Debug.Log("Going to Pest Control");

                    // 1. Pass the active plot down to the watering minigame
                    if (PestControl != null)
                    {
                        pestScript.CurrentPlot = CurrentPlot;
                        PestControl.gameObject.SetActive(true);
                    }


                    break;

                case "Struct":
                    Debug.Log("Going to Structure");

                    // 1. Pass the active plot down to the watering minigame
                    if (Structural_support != null)
                    {
                        StructScript.CurrentPlot = CurrentPlot;
                        Structural_support.gameObject.SetActive(true);
                    }


                    break;

                case "Weeding":
                    Debug.Log("Going to Weed");

                    // 1. Pass the active plot down to the watering minigame
                    if (Weed_Manager != null)
                    {
                        WeedScript.CurrentPlot = CurrentPlot;
                        Weed_Manager.gameObject.SetActive(true);
                    }


                    break;

                case "Netting":
                    Debug.Log("Going to Netting");

                    // 1. Pass the active plot down to the watering minigame
                    if (Netting != null)
                    {
                        NettingScript.CurrentPlot = CurrentPlot;
                        Netting.gameObject.SetActive(true);
                    }


                    break;

                default:

                    break
                    ;

            }



        }
    }
}
