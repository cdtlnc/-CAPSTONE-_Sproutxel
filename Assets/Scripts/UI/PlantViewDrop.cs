using UnityEngine;
using UnityEngine.EventSystems;

public class PlantViewDrop : MonoBehaviour, IDropHandler
{
    [Header("Plant Minigames")]
    [SerializeField] public GameObject Watering;
    [SerializeField] public GameObject SoilEnrichment;
    [SerializeField] public GameObject PestControl;
    [SerializeField] public GameObject Structural_support;
    [SerializeField] public GameObject Weed_Manager;
    [SerializeField] public GameObject Netting;

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
                    Watering.SetActive(true);

                    break;

                case "SoilEnrichment":
                    SoilEnrichment.SetActive(true);


                    break;

                case "PestControl":
                    PestControl.SetActive(true);


                    break;

                case "Struct":
                    Structural_support.SetActive(true);


                    break;

                case "Weeding":
                    Weed_Manager.SetActive(true);


                    break;

                case "Netting":
                    Netting.SetActive(true);


                    break;

                default:

                    break
                    ;

            }



        }
    }
}
