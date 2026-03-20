using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Color highlightColor = new Color(0.6f, 0.9f, 1f, 1f);

    private Image _image;
    private Color _originalColor;

    private void Awake()
    {
        _image = GetComponent<Image>();
        if (_image != null)
            _originalColor = _image.color;
    }

    /// <summary>Appelé quand un DraggableItems est relâché sur ce slot.</summary>
    public void OnDrop(PointerEventData eventData)
    {
        ResetColor();

        GameObject dropped = eventData.pointerDrag;
        if (dropped == null) return;

        DraggableItems draggable = dropped.GetComponent<DraggableItems>();
        if (draggable == null) return;

        DraggableItems occupant = GetOccupant();

        if (occupant != null)
        {
            occupant.parentAfterDrag = draggable.originalParent;
            occupant.transform.SetParent(draggable.originalParent, false);
            occupant.transform.localPosition = Vector3.zero;
        }

        draggable.parentAfterDrag = transform;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!DraggableItems.IsDragging || _image == null) return;

        _image.color = highlightColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        ResetColor();
    }

    private void ResetColor()
    {
        if (_image != null)
            _image.color = _originalColor;
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