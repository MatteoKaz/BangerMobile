using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

/// <summary>
/// Slider vertical de sélection de difficulté avec feeling lourd et rebond spring.
/// 3 positions snappées : haut = Easy, milieu = Medium, bas = Hard.
/// Change le sprite et la lumière du LED selon la difficulté.
/// Le bouton GO est désactivé après un clic et se réarme à chaque DayBegin.
/// </summary>
public class DifficultySlider : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    // ── Snap Positions ────────────────────────────────────────────────────
    [Header("Snap Positions (anchoredPosition Y)")]
    [SerializeField] private float positionEasy   = -250f;
    [SerializeField] private float positionMedium =    0f;
    [SerializeField] private float positionHard   =  200f;

    // ── LED Sprites ───────────────────────────────────────────────────────
    [Header("LED Sprites")]
    [SerializeField] private Image  ledImage;
    [SerializeField] private Sprite spriteEasy;
    [SerializeField] private Sprite spriteMedium;
    [SerializeField] private Sprite spriteHard;

    // ── LED Glow ──────────────────────────────────────────────────────────
    [Header("LED Glow")]
    [SerializeField] private Light2D ledLightA;
    [SerializeField] private Light2D ledLightB;
    [SerializeField] private Color colorEasy   = new Color(0.2f, 1f,   0.2f);
    [SerializeField] private Color colorMedium = new Color(1f,   0.65f, 0f);
    [SerializeField] private Color colorHard   = new Color(1f,   0.1f,  0.1f);

    // ── Spring Feel ───────────────────────────────────────────────────────
    [Header("Spring Feel")]
    [SerializeField] private float heavyDragMult = 0.30f;
    [SerializeField] private float snapSpeed     = 28f;
    [SerializeField] private float dampingRatio  = 0.40f;

    // ── References ────────────────────────────────────────────────────────
    [Header("References")]
    [SerializeField] private QuotatManager quotatManager;
    [SerializeField] private DayManager    dayManager;
    [SerializeField] private Button        goButton;

    // ── Public State ──────────────────────────────────────────────────────
    public int CurrentDifficulty { get; private set; } = 0; // 0=Easy 1=Medium 2=Hard

    /// <summary>Fired every time the slider snaps to a new difficulty. Parameter is 0=Easy, 1=Medium, 2=Hard.</summary>
    public event Action<int> DifficultyChanged;

    // ── Private State ─────────────────────────────────────────────────────
    private RectTransform _rect;
    private bool          _isDragging;
    private float         _dragStartLocalY;
    private float         _rectStartY;
    private float         _velocity;
    private float         _targetY;
    private Coroutine     _springCoroutine;
    private bool          _goAlreadyPressed;

    private float[] _snapPositions;

    private const float RubberBandFactor  = 0.25f;
    private const float SnapSettlePos     = 0.5f;
    private const float SnapSettleVel     = 2f;
    private const float CarryVelocityMult = 0.4f;

    [SerializeField] AudioEventDispatcher audioEventDispatcher;
    // ─────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (audioEventDispatcher == null)
            Debug.LogError("[DifficultySlider] audioEventDispatcher is not assigned — sounds will not play.", this);
        _rect          = GetComponent<RectTransform>();
        _snapPositions = new float[] { positionEasy, positionMedium, positionHard };

        SetAnchoredY(positionEasy);
        _targetY = positionEasy;
        UpdateLED(0);
        DifficultyChanged?.Invoke(0);
    }

    private void OnEnable()
    {
        if (dayManager != null)
            dayManager.DayBegin += ResetGoButton;
    }

    private void OnDisable()
    {
        if (dayManager != null)
            dayManager.DayBegin -= ResetGoButton;
    }

    // ── Drag Handling ─────────────────────────────────────────────────────

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_goAlreadyPressed) return;
        
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

        if (newY > positionHard) newY = positionHard + (newY - positionHard) * RubberBandFactor;
        if (newY < positionEasy) newY = positionEasy + (newY - positionEasy) * RubberBandFactor;

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

    // ── Snap + Spring ─────────────────────────────────────────────────────

    private void SnapTo(int index)
    {
        CurrentDifficulty = index;
        _targetY          = _snapPositions[index];
        UpdateLED(index);
        DifficultyChanged?.Invoke(index);

        if (audioEventDispatcher != null)
            audioEventDispatcher.PlayAudio(AudioType.Levier); // one-shot immédiat

        StopCurrentSpring();
        _springCoroutine = StartCoroutine(SpringCoroutine());
    }
    private IEnumerator SpringCoroutine()
    {
        float omega      = snapSpeed;
        float zeta       = dampingRatio;
        float currentY   = _rect.anchoredPosition.y;
        float currentVel = _velocity * CarryVelocityMult;

        while (true)
        {
            float displacement = currentY - _targetY;
            float springForce  = -omega * omega * displacement;
            float dampForce    = -2f * zeta * omega * currentVel;
            float acceleration = springForce + dampForce;

            currentVel += acceleration * Time.deltaTime;
            currentY   += currentVel   * Time.deltaTime;

            SetAnchoredY(currentY);

            if (Mathf.Abs(displacement) < SnapSettlePos && Mathf.Abs(currentVel) < SnapSettleVel)
            {
                SetAnchoredY(_targetY);
                break;
            }

            yield return null;
        }
    }

    // ── ButtonGo ──────────────────────────────────────────────────────────

    /// <summary>
    /// Branche cette méthode sur ButtonGo.onClick.
    /// N'est actif qu'une seule fois par jour ; se réarme sur DayBegin.
    /// </summary>
    public void OnGoPressed()
    {
        if (_goAlreadyPressed) return;
        if (quotatManager == null) { Debug.LogError("DifficultySlider: QuotatManager reference is missing."); return; }

        _goAlreadyPressed = true;
        if (goButton != null) goButton.interactable = false;

        if (audioEventDispatcher != null)
            audioEventDispatcher.PlayAudio(AudioType.Bouton); // one-shot immédiat

        quotatManager.SelectQuotat(CurrentDifficulty);
    }


    private void ResetGoButton()
    {
        _goAlreadyPressed = false;

        if (goButton != null)
            goButton.interactable = true;
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
        if (ledImage != null)
        {
            switch (difficultyIndex)
            {
                case 0: ledImage.sprite = spriteEasy;   break;
                case 1: ledImage.sprite = spriteMedium; break;
                case 2: ledImage.sprite = spriteHard;   break;
            }
        }

        Color targetColor = difficultyIndex switch
        {
            0 => colorEasy,
            1 => colorMedium,
            _ => colorHard
        };

        if (ledLightA != null) ledLightA.color = targetColor;
        if (ledLightB != null) ledLightB.color = targetColor;
    }
}
