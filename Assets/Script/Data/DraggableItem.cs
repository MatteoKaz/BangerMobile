using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Hold-to-grab avec animation de scale dès le hold validé,
/// et ghost placeholder dans la drop zone cible.
/// </summary>
public class DraggableItem : MonoBehaviour, IPointerDownHandler, IPointerUpHandler,
                                            IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private const float HoldDuration = 0.3f;
    private const float PickupScaleMultiplier = 1.15f;
    private const float PickupAnimDuration = 0.15f;
    private const float DropAnimDuration = 0.2f;

    private Canvas _canvas;
    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;
    private ScrollRect _activeScrollRect;

    private Transform _originalParent;
    private Vector2 _originalAnchoredPosition;
    private Vector3 _originalLocalScale;

    private bool _dragActivated;
    private bool _isDraggingForScroll;
    private bool _dropWasAccepted;
    private bool _dragStarted;
    private bool _dragBegin;

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

    public void OnPointerDown(PointerEventData eventData)
    {
        _dragStarted = false;
        _dragActivated = false;
        _isDraggingForScroll = false;
        _dropWasAccepted = false;
        _dragStarted = true; 
        CancelHold();
        _holdCoroutine = StartCoroutine(HoldRoutine());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CancelHold();
        _dragStarted = false; 
    }

    private IEnumerator HoldRoutine()
    {
        
        if (_dragBegin == true )
        {
            yield return new WaitForSeconds(HoldDuration);
            if (_dragBegin == true)
                StopCoroutine(HoldRoutine());
        }
        _dragActivated = true;
        _holdCoroutine = null;
        Debug.Log("CoroutineLancé");
        
    
        _originalLocalScale = _rectTransform.localScale;
        AnimateScale(_originalLocalScale * PickupScaleMultiplier, PickupAnimDuration);
        // return new WaitForSeconds(0.01f);
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
        _dragStarted = true;
        _dropWasAccepted = false;
        Debug.Log("Drag Commencé");

        if (!_dragActivated)
        {
            CancelHold();
            _isDraggingForScroll = true;
            _activeScrollRect = GetComponentInParent<ScrollRect>();
            if (_activeScrollRect != null)
                ExecuteEvents.Execute(_activeScrollRect.gameObject, eventData, ExecuteEvents.beginDragHandler);
            return;
        }

        _isDraggingForScroll = false;
        _originalParent = transform.parent;
        _originalAnchoredPosition = _rectTransform.anchoredPosition;

        if (_scaleCoroutine != null) { StopCoroutine(_scaleCoroutine); _scaleCoroutine = null; }

        CreateGhost();

        transform.SetParent(_canvas.transform, true);

        _canvasGroup.alpha = 0.7f;
        _canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (_isDraggingForScroll)
        {
            if (_activeScrollRect != null)
                ExecuteEvents.Execute(_activeScrollRect.gameObject, eventData, ExecuteEvents.dragHandler);
            return;
        }

        if (!_dragActivated) return;

        _rectTransform.anchoredPosition += eventData.delta / _canvas.scaleFactor;
        UpdateGhost(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_isDraggingForScroll)
        {
            _isDraggingForScroll = false;
            if (_activeScrollRect != null)
                ExecuteEvents.Execute(_activeScrollRect.gameObject, eventData, ExecuteEvents.endDragHandler);
            _activeScrollRect = null;
            return;
        }

        _canvasGroup.alpha = 1f;
        _canvasGroup.blocksRaycasts = true;
        _dragActivated = false;
        DestroyGhost();

        if (!_dropWasAccepted && transform.parent == _canvas.transform)
        {
            transform.SetParent(_originalParent, true);
            _rectTransform.anchoredPosition = _originalAnchoredPosition;
            AnimateScale(_originalLocalScale, DropAnimDuration);
        }
    }

    /// <summary>
    /// Called by ScrollDropZone when the item is successfully dropped.
    /// OnEndDrag handles visual cleanup after this.
    /// </summary>
    public void OnDropAccepted(Transform newParent)
    {
        _dropWasAccepted = true;
        DestroyGhost();
        transform.SetParent(newParent, true);
        AnimateScale(_originalLocalScale, DropAnimDuration);
    }

    // ─── Ghost ────────────────────────────────────────────────────────────────

    private void CreateGhost()
    {
        if (_ghost != null) return;

        _ghost = Instantiate(gameObject, _canvas.transform, false);

        // Désactive immédiatement pour bloquer tout event avant la fin du frame
        if (_ghost.TryGetComponent<DraggableItem>(out var ghostDraggable))
        {
            ghostDraggable.enabled = false;
            Destroy(ghostDraggable);
        }

        // Stoppe toutes les coroutines sur le clone pour éviter tout HoldRoutine parasite
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

        if (hoveredZone != null && hoveredZone.TargetContent != null)
        {
            if (_currentHoveredZone != hoveredZone)
            {
                _currentHoveredZone = hoveredZone;
                _ghost.transform.SetParent(hoveredZone.TargetContent, false);
                _ghost.GetComponent<RectTransform>().localScale = _originalLocalScale;
            }
            _ghost.SetActive(true);
        }
        else
        {
            _currentHoveredZone = null;
            _ghost.SetActive(false);
        }
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
