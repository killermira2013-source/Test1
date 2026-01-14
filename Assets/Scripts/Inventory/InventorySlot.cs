using UnityEngine;
using UnityEngine.EventSystems;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObj = eventData.pointerDrag;
        DraggableItem draggableItem = droppedObj.GetComponent<DraggableItem>();

        if (draggableItem != null)
        {
            if (transform.childCount == 0)
            {
                draggableItem.parentAfterDrag = transform;
            }
        }
    }
}