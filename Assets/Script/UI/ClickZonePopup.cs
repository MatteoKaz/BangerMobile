using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Affiche un tampon au relâchement dont l'intensité dépend de la durée d'appui (3 niveaux).
/// Trois modes : LoadScene, OpenPanel, ToggleStamp.
/// En mode ToggleStamp, le tampon reste jusqu'au prochain clic sur une autre zone,
/// ou disparaît automatiquement si autoHideDelay est supérieur à 0.
/// </summary>
[RequireComponent(typeof(Image))]
public class ClickZonePopup : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum ActionMode
    {
        LoadScene,
        OpenPanel,
        /// <summary>
        /// Colle le tampon sur la carte. Il reste affiché jusqu'à ce qu'une autre zone
        /// ToggleStamp soit activée, ou se cache automatiquement si autoHideDelay > 0.
        /// </summary>
        ToggleStamp
    }

    // Zone actuellement stampée — partagée entre toutes les instances.
    private static ClickZonePopup _activeToggleZone;

    [Header("Action")]
    [Tooltip("LoadScene : charge une scène.\nOpenPanel : toggle un panel UI.\nToggleStamp : colle le tampon sur la carte.")]
    [SerializeField] private ActionMode actionMode = ActionMode.LoadScene;

    [Header("Popup — doit être enfant de la carte en mode ToggleStamp")]
    [SerializeField] private RectTransform popup;
    [SerializeField] private Image popupImage;
    [Tooltip("Durée en secondes avant que le tampon disparaisse automatiquement (0 = reste indéfiniment).")]
    [SerializeField] private float autoHideDelay = 0f;

    [Header("Sprites du tampon (du plus clair au plus foncé)")]
    [SerializeField] private Sprite stampLight;
    [SerializeField] private Sprite stampMedium;
    [SerializeField] private Sprite stampDark;

    [Header("Seuils de durée (secondes)")]
    [Tooltip("Durée minimale pour le tampon moyen.")]
    [SerializeField] private float thresholdMedium = 0.4f;
    [Tooltip("Durée minimale pour le tampon foncé.")]
    [SerializeField] private float thresholdDark = 0.9f;

    [Header("Délai avant action (secondes) — ignoré en mode ToggleStamp")]
    [SerializeField] private float delayBeforeAction = 1.5f;

    [Header("Mode : Load Scene")]
#if UNITY_EDITOR
    [SerializeField] private SceneAsset targetSceneAsset;
#endif
    [HideInInspector]
    [SerializeField] private string targetSceneName;

    [Header("Mode : Open Panel")]
    [Tooltip("Panel à activer/désactiver.")]
    [SerializeField] private GameObject targetPanel;
    [Tooltip("(Optionnel) Conteneur dont les enfants seront désactivés avant d'ouvrir targetPanel.")]
    [SerializeField] private GameObject parentPanel;

    private Canvas _canvas;
    private Coroutine _actionCoroutine;
    private Coroutine _autoHideCoroutine;
    private float _holdStartTime;
    private Vector2 _pressScreenPosition;

    [SerializeField] private AudioEventDispatcher audioEventDispatcher;
    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();

        if (popup != null)
            popup.gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        audioEventDispatcher.PlayAudio(AudioType.Tampon);
        _holdStartTime       = Time.realtimeSinceStartup;
        _pressScreenPosition = eventData.position;

        if (_actionCoroutine != null)
        {
            StopCoroutine(_actionCoroutine);
            _actionCoroutine = null;

            if (actionMode != ActionMode.ToggleStamp)
                popup.gameObject.SetActive(false);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        float holdDuration = Time.realtimeSinceStartup - _holdStartTime;
        ApplyStamp(SelectStamp(holdDuration));

        if (actionMode == ActionMode.ToggleStamp)
        {
            MovePopupToClick(_pressScreenPosition);
            ExecuteToggleStamp();
        }
        else
        {
            MovePopupToClick(_pressScreenPosition);
            popup.gameObject.SetActive(true);
            _actionCoroutine = StartCoroutine(ExecuteActionAfterDelay());
        }
    }

    /// <summary>Retourne le sprite correspondant à la durée d'appui.</summary>
    private Sprite SelectStamp(float duration)
    {
        if (duration >= thresholdDark)   return stampDark;
        if (duration >= thresholdMedium) return stampMedium;
        return stampLight;
    }

    /// <summary>Déplace le popup à la position écran du clic en coordonnées monde Canvas.</summary>
    private void MovePopupToClick(Vector2 screenPosition)
    {
        Camera cam = _canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : _canvas.worldCamera;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.GetComponent<RectTransform>(),
                screenPosition,
                cam,
                out Vector2 localPoint))
        {
            popup.position = _canvas.transform.TransformPoint(localPoint);
        }
    }

    private void ApplyStamp(Sprite sprite)
    {
        if (popupImage != null)
            popupImage.sprite = sprite;
    }

    /// <summary>
    /// Mode ToggleStamp : retire le tampon de l'ancienne zone active,
    /// pose le tampon sur cette zone, puis lance l'auto-hide si configuré.
    /// </summary>
    private void ExecuteToggleStamp()
    {
        // Annule l'auto-hide en cours sur l'ancienne zone active
        if (_activeToggleZone != null && _activeToggleZone != this)
        {
            if (_activeToggleZone._autoHideCoroutine != null)
            {
                _activeToggleZone.StopCoroutine(_activeToggleZone._autoHideCoroutine);
                _activeToggleZone._autoHideCoroutine = null;
            }
            _activeToggleZone.popup.gameObject.SetActive(false);
        }

        // Annule l'auto-hide précédent sur cette zone si on re-clique dessus
        if (_autoHideCoroutine != null)
        {
            StopCoroutine(_autoHideCoroutine);
            _autoHideCoroutine = null;
        }

        popup.gameObject.SetActive(true);
        _activeToggleZone = this;

        // Lance l'auto-hide uniquement si un délai est configuré
        if (autoHideDelay > 0f)
            _autoHideCoroutine = StartCoroutine(AutoHideStamp());
    }

    /// <summary>
    /// Cache automatiquement le tampon après autoHideDelay secondes.
    /// Utilisé pour les actions comme virer un employé.
    /// </summary>
    private IEnumerator AutoHideStamp()
    {
        yield return new WaitForSecondsRealtime(autoHideDelay);

        if (popup != null)
            popup.gameObject.SetActive(false);

        if (_activeToggleZone == this)
            _activeToggleZone = null;

        _autoHideCoroutine = null;
    }

    /// <summary>Attend le délai en temps réel puis exécute l'action configurée.</summary>
    private IEnumerator ExecuteActionAfterDelay()
    {
        yield return new WaitForSecondsRealtime(delayBeforeAction);

        switch (actionMode)
        {
            case ActionMode.LoadScene: ExecuteLoadScene(); break;
            case ActionMode.OpenPanel: ExecuteOpenPanel(); break;
        }
    }

    private void ExecuteLoadScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(targetSceneName);
        }
        else
        {
            Debug.LogWarning("ClickZonePopup : aucune scène cible assignée.", this);
        }
    }

    /// <summary>
    /// Toggle le panel cible. Si un parentPanel est assigné,
    /// désactive ses enfants avant d'ouvrir targetPanel.
    /// </summary>
    private void ExecuteOpenPanel()
    {
        if (targetPanel == null)
        {
            Debug.LogWarning("ClickZonePopup : aucun panel cible assigné.", this);
            return;
        }

        bool isOpen = targetPanel.activeSelf;

        if (isOpen)
        {
            targetPanel.SetActive(false);
        }
        else
        {
            if (parentPanel != null && parentPanel != targetPanel)
            {
                foreach (Transform child in parentPanel.transform)
                    child.gameObject.SetActive(false);
            }

            targetPanel.SetActive(true);
        }

        popup.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_activeToggleZone == this)
            _activeToggleZone = null;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (targetSceneAsset != null)
            targetSceneName = targetSceneAsset.name;
    }
#endif
}
