using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Hold-to-grab avec animation de scale dès le hold validé,
/// ghost placeholder dans le premier slot libre de la drop zone cible.
/// </summary>
public class DraggableItem : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    private const float HoldDuration = 0.3f;
    private const float PickupScaleMultiplier = 1.15f;
    private const float PickupAnimDuration = 0.15f;
    private const float DropAnimDuration = 0.2f;

    private Canvas _canvas;
    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;

    private ContentSlot _originalSlot;
    private Vector3 _originalLocalScale;

    private bool _dragActivated;
    private bool _dropWasAccepted;

    private Coroutine _holdCoroutine;
    private Coroutine _scaleCoroutine;

    private GameObject _ghost;
    private ScrollDropZone _currentHoveredZone;
    private readonly List<RaycastResult> _raycastResults = new List<RaycastResult>();

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();

        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();

        _canvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        // Failsafe : item perdu dans le Canvas sans drag actif
        if (!_dragActivated && transform.parent == _canvas.transform)
            ForceReset();
    }

    private void OnDisable()
    {
        ForceReset();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _dragActivated = false;
        _dropWasAccepted = false;

        CancelHold();
        _holdCoroutine = StartCoroutine(HoldRoutine());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CancelHold();

        if (!_dragActivated && transform.parent == _canvas.transform)
            ForceReset();
    }

    private IEnumerator HoldRoutine()
    {
        yield return new WaitForSeconds(HoldDuration);

        _dragActivated = true;
        _holdCoroutine = null;

        _originalLocalScale = _rectTransform.localScale;
        AnimateScale(_originalLocalScale * PickupScaleMultiplier, PickupAnimDuration);
    }

    private void CancelHold()
    {
        if (_holdCoroutine != null)
        {
            StopCoroutine(_holdCoroutine);
            _holdCoroutine = null;
        }
    }

    private void AnimateScale(Vector3 targetScale, float duration)
    {
        if (_scaleCoroutine != null) StopCoroutine(_scaleCoroutine);
        _scaleCoroutine = StartCoroutine(ScaleRoutine(targetScale, duration));
    }

    private IEnumerator ScaleRoutine(Vector3 targetScale, float duration)
    {
        Vector3 startScale = _rectTransform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t);
            _rectTransform.localScale = Vector3.Lerp(startScale, targetScale, t);
            yield return null;
        }

        _rectTransform.localScale = targetScale;
        _scaleCoroutine = null;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_dragActivated) return;

        _dropWasAccepted = false;

        // Sauvegarde et libère le slot d'origine
        _originalSlot = transform.parent.GetComponent<ContentSlot>();
        _originalSlot?.Release();

        _originalLocalScale = _rectTransform.localScale;

        if (_scaleCoroutine != null) { StopCoroutine(_scaleCoroutine); _scaleCoroutine = null; }

        CreateGhost();

        transform.SetParent(_canvas.transform, true);

        _canvasGroup.alpha = 0.7f;
        _canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_dragActivated) return;

        _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
        UpdateGhost(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!_dragActivated && !_dropWasAccepted) return;

        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;
        _dragActivated = false;
        DestroyGhost();

        if (!_dropWasAccepted && transform.parent == _canvas.transform)
            ReturnToOriginalSlot();
    }

    /// <summary>Called by ScrollDropZone when the item is successfully dropped into a slot.</summary>
    public void OnDropAccepted(ContentSlot slot)
    {
        _dropWasAccepted = true;
        DestroyGhost();

        Vector3 worldScale = _rectTransform.lossyScale;
        slot.PlaceItem(this);

        Vector3 slotWorld = slot.transform.lossyScale;
        _rectTransform.localScale = new Vector3(
            worldScale.x / slotWorld.x,
            worldScale.y / slotWorld.y,
            worldScale.z / slotWorld.z
        );

        AnimateScale(_originalLocalScale, DropAnimDuration);
    }

    private void ReturnToOriginalSlot()
    {
        if (_originalSlot == null) return;

        Vector3 worldScale = _rectTransform.lossyScale;
        _originalSlot.PlaceItem(this);

        Vector3 slotWorld = _originalSlot.transform.lossyScale;
        _rectTransform.localScale = new Vector3(
            worldScale.x / slotWorld.x,
            worldScale.y / slotWorld.y,
            worldScale.z / slotWorld.z
        );

        AnimateScale(_originalLocalScale, DropAnimDuration);
    }

    private void ForceReset()
    {
        DestroyGhost();
        CancelHold();

        if (_originalSlot != null)
        {
            _originalSlot.PlaceItem(this);
            _rectTransform.localScale = _originalLocalScale;
        }

        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;
    }

    // ───────── GHOST ─────────

    private void CreateGhost()
    {
        if (_ghost != null) return;

        _ghost = Instantiate(gameObject, _canvas.transform, false);

        if (_ghost.TryGetComponent<DraggableItem>(out var ghostDraggable))
        {
            ghostDraggable.enabled = false;
            Destroy(ghostDraggable);
        }

        foreach (var mono in _ghost.GetComponents<MonoBehaviour>())
        {
            if (mono != null && mono.enabled)
                mono.StopAllCoroutines();
        }

        CanvasGroup ghostCg = _ghost.GetComponent<CanvasGroup>();
        if (ghostCg == null) ghostCg = _ghost.AddComponent<CanvasGroup>();
        ghostCg.alpha = 0.4f;
        ghostCg.blocksRaycasts = false;
        ghostCg.interactable = false;

        _ghost.GetComponent<RectTransform>().localScale = _originalLocalScale;
        _ghost.SetActive(false);
    }

    private void UpdateGhost(PointerEventData eventData)
    {
        if (_ghost == null) return;

        ScrollDropZone hoveredZone = FindHoveredDropZone(eventData);

        if (hoveredZone != null)
        {
            ContentSlot emptySlot = hoveredZone.GetFirstEmptySlot();

            if (emptySlot != null)
            {
                _currentHoveredZone = hoveredZone;
                _ghost.transform.SetParent(emptySlot.transform, false);

                RectTransform ghostRect = _ghost.GetComponent<RectTransform>();
                ghostRect.anchorMin = Vector2.zero;
                ghostRect.anchorMax = Vector2.one;
                ghostRect.offsetMin = Vector2.zero;
                ghostRect.offsetMax = Vector2.zero;
                ghostRect.localScale = _originalLocalScale;

                _ghost.SetActive(true);
                return;
            }
        }

        _currentHoveredZone = null;
        _ghost.SetActive(false);
    }

    private void DestroyGhost()
    {
        if (_ghost != null)
        {
            Destroy(_ghost);
            _ghost = null;
        }
        _currentHoveredZone = null;
    }

    private ScrollDropZone FindHoveredDropZone(PointerEventData eventData)
    {
        _raycastResults.Clear();
        EventSystem.current.RaycastAll(eventData, _raycastResults);

        foreach (var result in _raycastResults)
        {
            if (result.gameObject.TryGetComponent<ScrollDropZone>(out var zone))
                return zone;
        }

        return null;
    }
}
