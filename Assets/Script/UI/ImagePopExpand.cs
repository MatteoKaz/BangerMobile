using UnityEngine;

/// <summary>
/// 1er clic : passe devant et joue l'animation en avant.
/// 2e clic : joue en sens inverse et reprend sa place d'origine.
/// </summary>
[RequireComponent(typeof(Animator))]
public class ImagePopExpand : MonoBehaviour
{
    private static readonly int SpeedParam = Animator.StringToHash("Speed");

    private Animator _animator;
    private bool     _isOpen        = false;
    private int      _originalIndex;

    private void Awake()
    {
        _animator         = GetComponent<Animator>();
        _animator.enabled = false;
        _originalIndex    = transform.GetSiblingIndex();
    }

    /// <summary>Bascule la lecture avant/arrière et le rang dans la hiérarchie.</summary>
    public void Toggle()
    {
        _isOpen = !_isOpen;

        if (_isOpen)
            transform.SetAsLastSibling();
        else
            transform.SetSiblingIndex(_originalIndex);

        _animator.enabled = true;
        _animator.SetFloat(SpeedParam, _isOpen ? 1f : -1f);
        _animator.Play("JulieAnim", 0, _isOpen ? 0f : 1f);
    }
}