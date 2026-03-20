using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Attaches to the Scroll Menu root.
/// Crée MaxSlots slots au démarrage, migre les items existants,
/// et refuse les drops si tous les slots sont occupés.
/// </summary>
public class ScrollDropZone : MonoBehaviour, IDropHandler
{
    private const int MaxSlots = 4;

    [SerializeField] private Transform targetContent;
    [SerializeField] private ContentSlot slotPrefab;

    public Transform TargetContent => targetContent;

    private readonly List<ContentSlot> _slots = new List<ContentSlot>();

    private void Start()
    {
        // Récupère les items existants avant de créer les slots
        List<DraggableItem> existingItems = new List<DraggableItem>();
        foreach (Transform child in targetContent)
        {
            if (child.TryGetComponent<DraggableItem>(out var item))
                existingItems.Add(item);
        }

        // Crée les MaxSlots slots et migre les items existants dedans
        for (int i = 0; i < MaxSlots; i++)
        {
            ContentSlot slot = Instantiate(slotPrefab, targetContent);
            _slots.Add(slot);

            if (i < existingItems.Count)
                slot.PlaceItem(existingItems[i]);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem draggable = eventData.pointerDrag?.GetComponent<DraggableItem>();
        if (draggable == null) return;

        ContentSlot emptySlot = GetFirstEmptySlot();
        if (emptySlot == null) return;

        draggable.OnDropAccepted(emptySlot);
    }

    /// <summary>Retourne le premier slot vide, null si le content est plein.</summary>
    public ContentSlot GetFirstEmptySlot()
    {
        foreach (ContentSlot slot in _slots)
        {
            if (slot != null && slot.IsEmpty)
                return slot;
        }
        return null;
    }
}