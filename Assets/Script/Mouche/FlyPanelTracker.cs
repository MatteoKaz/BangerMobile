using UnityEngine;

/// <summary>
/// À placer sur chaque panel du menu. Notifie FlyController quand ce panel
/// est activé ou désactivé, afin que la mouche écrasée se montre/cache
/// selon le panel sur lequel elle a été tuée.
/// </summary>
public class FlyPanelTracker : MonoBehaviour
{
    [SerializeField] private FlyController _flyController;

    /// <summary>Retourne le Transform du panel géré par ce tracker.</summary>
    public Transform PanelTransform => transform;

    private void OnEnable()
    {
        _flyController.OnPanelShown(this);
    }

    private void OnDisable()
    {
        _flyController.OnPanelHidden(this);
    }
}