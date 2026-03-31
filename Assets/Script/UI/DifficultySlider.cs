using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Handles the vertical difficulty slider with a heavy, spring-bouncy feel.
/// Three snap positions: top = Easy, middle = Medium, bottom = Hard.
/// Drives the LED sprite and calls QuotatManager.SelectQuotat() on GO.
/// </summary>
public class DifficultySlider : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    // ── Snap positions (anchoredPosition.y) ──────────────────────────────
    [Header("Snap Positions (anchoredPosition Y)")]
    [SerializeField] private float positionEasy   =  200f;
    [SerializeField] private float positionMedium =    0f;
    [SerializeField] private float positionHard   = -200f;

    // ── LED sprites ───────────────────────────────────────────────────────
    [Header("LED")]
    [SerializeField] private Image  ledImage;
    [SerializeField] private Sprite spriteEasy;
    [SerializeField] private Sprite spriteMedium;
    [SerializeField] private Sprite spriteHard;

    // ── Spring / bounce feel ─────────────────────────────────────────────
    [Header("Spring Feel")]
    [SerializeField] private float snapSpeed    = 18f;   // fréquence angulaire du ressort
    [SerializeField] private float dampingRatio = 0.35f; // <1 = rebondissant, 1 = critique
    [SerializeField] private float heavyDragMult = 0.55f; // résistance au drag (0..1)

    // ── References ────────────────────────────────────────────────────────
    [Header("References")]
    [SerializeField] private QuotatManager quotatManager;

    // ── Public read state ─────────────────────────────────────────────────
    public int CurrentDifficulty { get; private set; } = 0; // 0=Easy 1=Medium 2=Hard

    // ── Private state ─────────────────────────────────────────────────────
    private RectTransform _rect;
    private bool          _isDragging;
    private float         _dragStartLocalY;
    private float         _rectStartY;
    private float         _velocity;
    private float         _targetY;
    private Coroutine     _springCoroutine;
    private float[]       _snapPositions;

    // ─────────────────────────────────────────────────────────────────────
    private void Awake()
    {
        _rect          = GetComponent<RectTransform>();
        _snapPositions = new float[] { positionEasy, positionMedium, positionHard };

        _targetY = positionEasy;
        SetAnchoredY(positionEasy);
        UpdateLED(0);
    }

    // ── Drag handling ─────────────────────────────────────────────────────

    public void OnPointerDown(PointerEventData eventData)
    {
        StopCurrentSpring();
        _isDragging = true;
        _velocity   = 0f;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rect.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        _dragStartLocalY = localPoint.y;
        _rectStartY      = _rect.anchoredPosition.y;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_isDragging) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rect.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out Vector2 localPoint);

        float delta = (localPoint.y - _dragStartLocalY) * heavyDragMult;
        float newY  = _rectStartY + delta;

        // Rubber-band au delà des extrêmes
        float minY = positionHard;
        float maxY = positionEasy;
        if (newY > maxY) newY = maxY + (newY - maxY) * 0.25f;
        if (newY < minY) newY = minY + (newY - minY) * 0.25f;

        _velocity = (newY - _rect.anchoredPosition.y) / Time.deltaTime;
        SetAnchoredY(newY);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (!_isDragging) return;
        _isDragging = false;

        int nearest = GetNearestSnapIndex(_rect.anchoredPosition.y);
        SnapTo(nearest);
    }

    // ── Snap + spring ─────────────────────────────────────────────────────

    private void SnapTo(int index)
    {
        CurrentDifficulty = index;
        _targetY          = _snapPositions[index];
        UpdateLED(index);

        StopCurrentSpring();
        _springCoroutine = StartCoroutine(SpringCoroutine());
    }

    private IEnumerator SpringCoroutine()
    {
        float omega      = snapSpeed;
        float zeta       = dampingRatio;
        float currentY   = _rect.anchoredPosition.y;
        float currentVel = _velocity * 0.4f; // on conserve une fraction de l'élan

        while (true)
        {
            float displacement = currentY - _targetY;
            float springForce  = -omega * omega * displacement;
            float dampForce    = -2f * zeta * omega * currentVel;
            float acceleration = springForce + dampForce;

            currentVel += acceleration * Time.deltaTime;
            currentY   += currentVel  * Time.deltaTime;

            SetAnchoredY(currentY);

            if (Mathf.Abs(displacement) < 0.5f && Mathf.Abs(currentVel) < 2f)
            {
                SetAnchoredY(_targetY);
                break;
            }

            yield return null;
        }
    }

    // ── Helpers ───────────────────────────────────────────────────────────

    private void StopCurrentSpring()
    {
        if (_springCoroutine != null)
        {
            StopCoroutine(_springCoroutine);
            _springCoroutine = null;
        }
    }

    private int GetNearestSnapIndex(float y)
    {
        int   best     = 0;
        float bestDist = float.MaxValue;
        for (int i = 0; i < _snapPositions.Length; i++)
        {
            float dist = Mathf.Abs(y - _snapPositions[i]);
            if (dist < bestDist) { bestDist = dist; best = i; }
        }
        return best;
    }

    private void SetAnchoredY(float y)
    {
        Vector2 pos = _rect.anchoredPosition;
        pos.y       = y;
        _rect.anchoredPosition = pos;
    }

    private void UpdateLED(int difficultyIndex)
    {
        if (ledImage == null) return;
        switch (difficultyIndex)
        {
            case 0: ledImage.sprite = spriteEasy;   break;
            case 1: ledImage.sprite = spriteMedium; break;
            case 2: ledImage.sprite = spriteHard;   break;
        }
    }

    // ── ButtonGo callback ─────────────────────────────────────────────────

    /// <summary>
    /// Branche cette méthode sur ButtonGo.onClick.
    /// Transmet la difficulté courante à QuotatManager.
    /// </summary>
    public void OnGoPressed()
    {
        if (quotatManager == null)
        {
            Debug.LogError("DifficultySlider: QuotatManager reference is missing.");
            return;
        }
        quotatManager.SelectQuotat(CurrentDifficulty);
    }
}
