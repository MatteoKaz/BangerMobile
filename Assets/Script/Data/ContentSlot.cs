using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Slot visuel dans un Content. Affiche un placeholder quand vide,
/// le masque quand un DraggableItem l'occupe.
/// </summary>
[RequireComponent(typeof(LayoutElement))]
public class ContentSlot : MonoBehaviour
{
    [SerializeField] private GameObject emptyVisual;

    private DraggableItem _occupant;

    public bool IsEmpty => _occupant == null;

    /// <summary>Place un item dans ce slot et masque le visuel vide.</summary>
    public void PlaceItem(DraggableItem item)
    {
        _occupant = item;

        item.transform.SetParent(transform, false);

        RectTransform itemRect = item.GetComponent<RectTransform>();
        itemRect.anchorMin = Vector2.zero;
        itemRect.anchorMax = Vector2.one;
        itemRect.offsetMin = Vector2.zero;
        itemRect.offsetMax = Vector2.zero;

        if (emptyVisual != null)
            emptyVisual.SetActive(false);
    }

    /// <summary>Libère le slot et réaffiche le visuel vide.</summary>
    public void Release()
    {
        _occupant = null;

        if (emptyVisual != null)
            emptyVisual.SetActive(true);
    }
}