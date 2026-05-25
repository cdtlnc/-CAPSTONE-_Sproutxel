using UnityEngine;
using UnityEngine.EventSystems;

public class PlantViewDrop : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        // This fires automatically if the user releases the mouse/finger over this panel
        if (eventData.pointerDrag != null)
        {
            Debug.Log($"Dropped {eventData.pointerDrag.name} onto {gameObject.name}!");

            // Do your logic here (e.g., planting the seed)
        }
    }
}
