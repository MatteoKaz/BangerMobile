using UnityEngine;
using System.Collections;

/// <summary>
/// Change le sprite et la taille du SpriteRenderer de la pile selon le nombre de papiers.
/// Anime un punch de scale à chaque changement de count.
/// Contre-scale le Canvas enfant pour que le compteur garde toujours la même taille.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class PileVisual : MonoBehaviour
{
    [System.Serializable]
    public struct PileThreshold
    {
        [Tooltip("Nombre minimum de papiers pour afficher ce sprite.")]
        public int minCount;
        public Sprite sprite;
        [Tooltip("Scale appliqué au GameObject pour ce seuil.")]
        public float scale;
    }

    private const float PunchScaleMultiplier = 1.2f;
    private const float PunchGrowDuration = 0.12f;
    private const float PunchShrinkDuration = 0.2f;
    private const int HiddenSortingOrder = -999;

    [SerializeField] private Pile pile;
    [SerializeField] private PileThreshold[] thresholds;
    [Tooltip("Canvas enfant contenant le compteur — son scale sera compensé automatiquement.")]
    [SerializeField] private Transform counterCanvas;

    private SpriteRenderer _spriteRenderer;
    private Vector3 _originalCanvasLocalScale;
    private Vector3 _targetScale;
    private Coroutine _punchCoroutine;
    private int _originalSortingOrder;
    private string _originalSortingLayer;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (counterCanvas != null)
            _originalCanvasLocalScale = counterCanvas.localScale;
    }

    private void Start()
    {
        _originalSortingOrder = _spriteRenderer.sortingOrder;
        _originalSortingLayer = _spriteRenderer.sortingLayerName;

        UpdateVisual();
        pile.UpdateCount += OnCountUpdated;
        DraggableItems.OnDragStateChanged += OnDragStateChanged;
    }

    private void OnDestroy()
    {
        pile.UpdateCount -= OnCountUpdated;
        DraggableItems.OnDragStateChanged -= OnDragStateChanged;
    }

    private void OnCountUpdated()
    {
        UpdateVisual();
        PlayPunch();
    }

    /// <summary>Abaisse le sorting order pendant le drag pour éviter que la pile passe devant l'employé.</summary>
    private void OnDragStateChanged(bool isDragging)
    {
        if (isDragging)
        {
            _spriteRenderer.sortingLayerName = "Default";
            _spriteRenderer.sortingOrder = HiddenSortingOrder;
        }
        else
        {
            _spriteRenderer.sortingLayerName = _originalSortingLayer;
            _spriteRenderer.sortingOrder = _originalSortingOrder;
        }
    }

    /// <summary>Sélectionne le sprite et le scale correspondant au paperCount actuel.</summary>
    private void UpdateVisual()
    {
        int count = pile.paperCount;
        Sprite selected = null;
        float selectedScale = 1f;

        foreach (PileThreshold threshold in thresholds)
        {
            if (count >= threshold.minCount)
            {
                selected = threshold.sprite;
                selectedScale = threshold.scale;
            }
        }

        if (selected == null) return;

        _spriteRenderer.sprite = selected;
        _targetScale = Vector3.one * selectedScale;
        ApplyScale(_targetScale);
    }

    /// <summary>Lance l'animation de punch : grossit puis revient à _targetScale.</summary>
    private void PlayPunch()
    {
        if (_punchCoroutine != null)
            StopCoroutine(_punchCoroutine);

        _punchCoroutine = StartCoroutine(PunchRoutine());
    }

    private IEnumerator PunchRoutine()
    {
        Vector3 punchTarget = _targetScale * PunchScaleMultiplier;

        yield return StartCoroutine(ScaleRoutine(punchTarget, PunchGrowDuration));
        yield return StartCoroutine(ScaleRoutine(_targetScale, PunchShrinkDuration));

        _punchCoroutine = null;
    }

    private IEnumerator ScaleRoutine(Vector3 target, float duration)
    {
        Vector3 start = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t);
            ApplyScale(Vector3.Lerp(start, target, t));
            yield return null;
        }

        ApplyScale(target);
    }

    /// <summary>Applique le scale à la pile et compense sur le Canvas pour garder le texte constant.</summary>
    private void ApplyScale(Vector3 scale)
    {
        transform.localScale = scale;

        if (counterCanvas != null && scale.x != 0f)
            counterCanvas.localScale = _originalCanvasLocalScale / scale.x;
    }
}
