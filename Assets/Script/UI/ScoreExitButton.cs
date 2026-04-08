using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gère la visibilité du tampon "Terminer" du menu score.
/// Le tampon est caché (et le GO réactivé) à chaque ouverture du score,
/// puis devient visible quand le joueur clique dessus.
/// </summary>
[RequireComponent(typeof(Button))]
public class ScoreExitButton : MonoBehaviour
{
    [SerializeField] private DayManager dayManager;
    [SerializeField] private UiManager uiManager;
    [SerializeField] private ClickZonePopup clickZonePopup;

    private void OnEnable()
    {
        dayManager.DayBegin += OnDayBegin;
        uiManager.ScoreAnim += OnScoreOpened;
    }

    private void OnDisable()
    {
        dayManager.DayBegin -= OnDayBegin;
        uiManager.ScoreAnim -= OnScoreOpened;
    }

    /// <summary>Réactive le GO du tampon et cache le popup à chaque ouverture du score.</summary>
    private void OnScoreOpened()
    {
        clickZonePopup?.HideMVP();
    }

    /// <summary>Réactive le GO du tampon et cache le popup à chaque début de journée.</summary>
    private void OnDayBegin()
    {
        clickZonePopup?.HideMVP();
    }
}