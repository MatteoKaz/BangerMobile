using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Met le jeu en pause et affiche un panel quand on clique sur le bouton,
/// reprend le jeu et ferme le panel au second clic.
/// </summary>
public class PauseManager : MonoBehaviour
{
    [SerializeField] private Button pauseButton;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private AudioEventDispatcher audioEventDispatcher;
    private bool _isPaused = false;

    private void Start()
    {
        pausePanel.SetActive(false);
    }

    /// <summary>Bascule entre pause et reprise du jeu.</summary>
    public void TogglePause()
    {
        _isPaused = !_isPaused;
        Time.timeScale = _isPaused ? 0f : 1f;
        pausePanel.SetActive(_isPaused);
    }


    private void OnDisable()
    {
        Time.timeScale = 1f;
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }
}