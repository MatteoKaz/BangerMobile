using System.Collections;
using UnityEngine;

/// <summary>
/// Affiche un panel d'accueil uniquement lors du tout premier lancement de la map
/// (jour 1, semaine 1). Se ferme automatiquement ou sur action du joueur.
/// </summary>
public class FirstLaunchPanel : MonoBehaviour
{
    [SerializeField] private DayManager dayManager;
    [SerializeField] private GameObject panel;

    [Tooltip("Durée d'affichage avant fermeture automatique. 0 = fermeture manuelle uniquement.")]
    [SerializeField] private float autoCloseDuration = 0f;

    private void OnEnable()
    {
        dayManager.FirstDayInitialization += OnFirstDay;
    }

    private void OnDisable()
    {
        dayManager.FirstDayInitialization -= OnFirstDay;
    }

    private void Start()
    {
        panel.SetActive(false);
    }

    private void OnFirstDay()
    {
        panel.SetActive(true);

        if (autoCloseDuration > 0f)
            StartCoroutine(AutoClose());
    }

    /// <summary>Ferme le panel. À brancher sur le bouton OK/Fermer du panel.</summary>
    public void ClosePanel()
    {
        StopAllCoroutines();
        panel.SetActive(false);
    }

    private IEnumerator AutoClose()
    {
        yield return new WaitForSecondsRealtime(autoCloseDuration);
        ClosePanel();
    }
}
