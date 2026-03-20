using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    /// <summary>Appelé quand un DraggableItems est relâché sur ce slot.</summary>
    public void OnDrop(PointerEventData eventData)
    {
        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        DraggableItems draggable = dropped.GetComponent<DraggableItems>();
        if (draggable == null) return;

        // Cherche un éventuel item déjà présent dans ce slot cible
        DraggableItems occupant = GetOccupant();

        if (occupant != null)
        {
            // Swap : l'occupant va dans le slot source
            occupant.parentAfterDrag = draggable.originalParent;
            occupant.transform.SetParent(draggable.originalParent, false);
            occupant.transform.localPosition = Vector3.zero;
        }

        // Place le draggable dans ce slot
        draggable.parentAfterDrag = transform;
    }

    /// <summary>Retourne le premier DraggableItems enfant direct de ce slot, ou null.</summary>
    private DraggableItems GetOccupant()
    {
        foreach (Transform child in transform)
        {
            DraggableItems item = child.GetComponent<DraggableItems>();
            if (item != null)
                return item;
        }
        return null;
    }
}