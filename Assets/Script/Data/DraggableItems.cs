using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DraggableItems : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image image;

    [HideInInspector] public Transform originalParent;
    [HideInInspector] public Transform parentAfterDrag;

    private Canvas _canvas;
    private CanvasGroup _canvasGroup;
    private Vector3 _originalLocalScale; // <-- ajout

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        parentAfterDrag = transform.parent;
        _originalLocalScale = transform.localScale; // <-- sauvegarde avant reparenting

        transform.SetParent(_canvas.transform, true);
        transform.SetAsLastSibling();

        _canvasGroup.blocksRaycasts = false;

        if (image != null)
            image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(parentAfterDrag, false);
        transform.localPosition = Vector3.zero;
        transform.localScale = _originalLocalScale; // <-- restauration du scale d'origine

        _canvasGroup.blocksRaycasts = true;

        if (image != null)
            image.raycastTarget = true;
    }
}