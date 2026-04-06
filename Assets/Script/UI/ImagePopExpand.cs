using UnityEngine;

/// <summary>
/// 1er clic : passe devant et joue l'animation en avant.
/// 2e clic : joue en sens inverse et reprend sa place d'origine.
/// Si une autre instance est déjà ouverte, elle est automatiquement refermée.
/// </summary>
[RequireComponent(typeof(Animator))]
public class ImagePopExpand : MonoBehaviour
{
    private static readonly int SpeedParam = Animator.StringToHash("Speed");

    /// <summary>Instance actuellement ouverte, partagée entre toutes les instances.</summary>
    private static ImagePopExpand _currentOpen;

    [Tooltip("Nom exact du state d'animation à jouer dans l'Animator Controller.")]
    [SerializeField] private string _animationStateName = "JulieAnim";

    private Animator _animator;
    private RectTransform _rectTransform;
    private bool _isOpen;
    private int _originalIndex;

    // Valeurs originales du RectTransform avant toute animation
    private Vector2 _originalAnchoredPosition;
    private Vector2 _originalSizeDelta;
    private Vector3 _originalLocalScale;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rectTransform = GetComponent<RectTransform>();
        _originalIndex = transform.GetSiblingIndex();

        // Capture les valeurs initiales avant que l'Animator ne les touche
        _originalAnchoredPosition = _rectTransform.anchoredPosition;
        _originalSizeDelta = _rectTransform.sizeDelta;
        _originalLocalScale = _rectTransform.localScale;
    }

    private void OnEnable()
    {
        // Réinitialise l'état à chaque fois que le panel parent est réactivé
        ResetToOriginalState();
    }

    private void OnDisable()
    {
        // Remet immédiatement l'image à sa position d'origine (panel non visible)
        ResetToOriginalState();
    }

    /// <summary>Bascule la lecture avant/arrière et le rang dans la hiérarchie.</summary>
    public void Toggle()
    {
        // Si une autre image est ouverte, on la referme d'abord
        if (_currentOpen != null && _currentOpen != this && _currentOpen._isOpen)
            _currentOpen.Close();

        _isOpen = !_isOpen;

        if (_isOpen)
        {
            _currentOpen = this;
            transform.SetAsLastSibling();
        }
        else
        {
            _currentOpen = null;
            transform.SetSiblingIndex(_originalIndex);
        }

        PlayAnimation(_isOpen ? 1f : -1f, _isOpen ? 0f : 1f);
    }

    /// <summary>Referme cette image depuis l'extérieur (appelé par une autre instance).</summary>
    public void Close()
    {
        if (!_isOpen) return;

        _isOpen = false;
        _currentOpen = null;
        transform.SetSiblingIndex(_originalIndex);
        PlayAnimation(-1f, 1f);
    }

    private void PlayAnimation(float speed, float normalizedTime)
    {
        _animator.enabled = true;
        _animator.SetFloat(SpeedParam, speed);
        _animator.Play(_animationStateName, 0, normalizedTime);
    }

    private void ResetToOriginalState()
    {
        _animator.enabled = false;
        _isOpen = false;

        if (_currentOpen == this)
            _currentOpen = null;

        // SetSiblingIndex est interdit pendant l'activation/désactivation d'un parent
        // On vérifie que le parent est actif avant d'appeler cette méthode
        if (transform.parent != null && transform.parent.gameObject.activeInHierarchy)
            transform.SetSiblingIndex(_originalIndex);

        _rectTransform.anchoredPosition = _originalAnchoredPosition;
        _rectTransform.sizeDelta = _originalSizeDelta;
        _rectTransform.localScale = _originalLocalScale;
    }
}
