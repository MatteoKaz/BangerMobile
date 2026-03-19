using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attaches to the Scroll Menu root. Drop zone covers the full visible scroll area.
/// Assign targetContent to the Content child of this scroll menu.
/// </summary>
public class ScrollDropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private Transform targetContent;

    public Transform TargetContent => targetContent;

    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem draggable = eventData.pointerDrag?.GetComponent<DraggableItem>();
        if (draggable == null) return;

        draggable.OnDropAccepted(targetContent);
    }
}