using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class DraggableItems : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image image;

    private const float PickupScaleMultiplier = 1.15f;
    private const float PickupAnimDuration = 0.15f;
    private const float DropAnimDuration = 0.2f;
    private const int DragSortingOrder = 99999;
    private const string DragSortingLayer = "Score";

    /// <summary>True pendant qu'un item est en cours de drag.</summary>
    public static bool IsDragging { get; private set; }

    [HideInInspector] public Transform originalParent;
    [HideInInspector] public Transform parentAfterDrag;

    private Canvas _parentCanvas;
    private Canvas _overrideCanvas;
    private GraphicRaycaster _overrideRaycaster;
    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    private Vector3 _originalWorldScale;
    private Coroutine _scaleCoroutine;
    public Employe linkedEmploye;

    [SerializeField] private AudioEventDispatcher audioEventDispatcher;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

        // Trouver le canvas parent (pas soi-même)
        Canvas[] parents = GetComponentsInParent<Canvas>(true);
        _parentCanvas = parents.Length > 0 ? parents[0] : null;

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        _overrideCanvas = GetComponent<Canvas>();
        if (_overrideCanvas == null)
            _overrideCanvas = gameObject.AddComponent<Canvas>();

        _overrideRaycaster = GetComponent<GraphicRaycaster>();
        if (_overrideRaycaster == null)
            _overrideRaycaster = gameObject.AddComponent<GraphicRaycaster>();

        _overrideCanvas.overrideSorting = false;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_parentCanvas == null) return;

        IsDragging = true;
        originalParent = transform.parent;
        parentAfterDrag = transform.parent;
        _originalWorldScale = transform.lossyScale;

        // Reparenter directement sous le canvas parent (pas de changement de canvas)
        transform.SetParent(_parentCanvas.transform, true);
        transform.SetAsLastSibling();

        // Monter en sorting order au-dessus de tout dans ce canvas
        _overrideCanvas.overrideSorting = true;
        _overrideCanvas.sortingLayerName = DragSortingLayer;
        _overrideCanvas.sortingOrder = DragSortingOrder;

        _canvasGroup.blocksRaycasts = false;
        if (audioEventDispatcher != null)
            audioEventDispatcher.PlayAudio(AudioType.MouseClick);

        if (image != null)
            image.raycastTarget = false;

        Vector3 pickupTarget = WorldToLocalScale(_parentCanvas.transform, _originalWorldScale * PickupScaleMultiplier);
        AnimateScale(pickupTarget, PickupAnimDuration);
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Conversion correcte screen → local pour Screen Space Camera
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)_parentCanvas.transform,
            eventData.position,
            _parentCanvas.worldCamera,
            out Vector2 localPoint))
        {
            _rectTransform.localPosition = localPoint;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        IsDragging = false;
        transform.SetParent(parentAfterDrag, false);
        transform.localPosition = Vector3.zero;
        transform.localScale = WorldToLocalScale(parentAfterDrag, _originalWorldScale * PickupScaleMultiplier);

        _overrideCanvas.overrideSorting = false;

        _canvasGroup.blocksRaycasts = true;

        if (image != null)
            image.raycastTarget = true;

        AnimateScale(WorldToLocalScale(parentAfterDrag, _originalWorldScale), DropAnimDuration);
        //  if (audioEventDispatcher != null)
        //      audioEventDispatcher.PlayAudio(AudioType.MouseClick);
    }

    private Vector3 WorldToLocalScale(Transform parent, Vector3 worldScale)
    {
        Vector3 parentWorld = parent.lossyScale;
        return new Vector3(
            worldScale.x / parentWorld.x,
            worldScale.y / parentWorld.y,
            worldScale.z / parentWorld.z
        );
    }

    private void AnimateScale(Vector3 targetScale, float duration)
    {
        if (_scaleCoroutine != null)
            StopCoroutine(_scaleCoroutine);

        _scaleCoroutine = StartCoroutine(ScaleRoutine(targetScale, duration));
    }

    private IEnumerator ScaleRoutine(Vector3 targetScale, float duration)
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t);
            transform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        transform.localScale = targetScale;
        _scaleCoroutine = null;
    }
}
