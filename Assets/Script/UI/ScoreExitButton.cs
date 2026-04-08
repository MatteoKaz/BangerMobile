using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gère la visibilité du tampon "Terminer" du menu score.
/// Le tampon est réinitialisé à chaque ouverture du score,
/// puis réactivé automatiquement à la fin de l'animation des chiffres.
/// </summary>
[RequireComponent(typeof(Button))]
public class ScoreExitButton : MonoBehaviour
{
    [SerializeField] private DayManager dayManager;
    [SerializeField] private UiManager uiManager;
    [SerializeField] private ClickZonePopup clickZonePopup;
    [SerializeField] private UIScore uiScore;

    private Coroutine _waitForAnimCoroutine;

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

    /// <summary>Réinitialise le tampon et attend la fin de l'animation du score pour le débloquer.</summary>
    private void OnScoreOpened()
    {
        if (uiScore == null)
            Debug.LogError("[ScoreExitButton] uiScore est NULL ! Assigne le champ dans l'Inspector.", this);

        if (clickZonePopup == null)
            Debug.LogError("[ScoreExitButton] clickZonePopup est NULL ! Assigne le champ dans l'Inspector.", this);

        clickZonePopup?.Block();
        clickZonePopup?.ResetStamp();

        if (_waitForAnimCoroutine != null)
            StopCoroutine(_waitForAnimCoroutine);

        _waitForAnimCoroutine = StartCoroutine(WaitForScoreAnimFinished());
    }

    /// <summary>Réinitialise le tampon au début de chaque journée.</summary>
    private void OnDayBegin()
    {
        if (_waitForAnimCoroutine != null)
        {
            StopCoroutine(_waitForAnimCoroutine);
            _waitForAnimCoroutine = null;
        }

        clickZonePopup?.Block();
        clickZonePopup?.ResetStamp();
    }

    /// <summary>Attend que l'animation des chiffres soit terminée, puis débloque et affiche le tampon.</summary>
    private IEnumerator WaitForScoreAnimFinished()
    {
        yield return new WaitUntil(() => uiScore != null && uiScore.hasFinish);
        clickZonePopup?.Unblock();
        clickZonePopup?.ShowStamp();
        _waitForAnimCoroutine = null;
    }
}
