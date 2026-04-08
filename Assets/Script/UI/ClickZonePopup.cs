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
        ToggleStamp
    }

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

    [Header("Groupe (mode ToggleStamp)")]
    [Tooltip("Même string sur les boutons MVP et Virer du même employé. Ex: 'Employe_01'")]
    [SerializeField] private string _stampGroup = "";

    [Header("Audio")]
    [SerializeField] private AudioEventDispatcher audioEventDispatcher;

    private static System.Collections.Generic.Dictionary<string, ClickZonePopup> _activeByGroup
        = new System.Collections.Generic.Dictionary<string, ClickZonePopup>();

    private Canvas _canvas;
    private Coroutine _actionCoroutine;
    private Coroutine _autoHideCoroutine;
    private Coroutine _autoHideCoroutine1;
    private float _holdStartTime;
    private Vector2 _pressScreenPosition;
    private bool _isBlocked = false;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();

        if (popup != null)
            popup.gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
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

    /// <summary>Bloque l'affichage du tampon jusqu'au prochain Unblock.</summary>
    public void Block()
    {
        Debug.Log($"[ClickZonePopup] Block() appelé sur {gameObject.name}");
        _isBlocked = true;
    }

    /// <summary>Réautorise l'affichage du tampon.</summary>
    public void Unblock()
    {
        Debug.Log($"[ClickZonePopup] Unblock() appelé sur {gameObject.name}");
        _isBlocked = false;
    }

    /// <summary>Rend le tampon visible immédiatement avec le sprite léger par défaut.</summary>
    public void ShowStamp()
    {
        Debug.Log($"[ClickZonePopup] ShowStamp() — popup null: {popup == null} | popupImage null: {popupImage == null} | stampLight null: {stampLight == null}");

        if (_autoHideCoroutine1 != null)
        {
            StopCoroutine(_autoHideCoroutine1);
            _autoHideCoroutine1 = null;
            Debug.Log("[ClickZonePopup] ShowStamp() → _autoHideCoroutine1 annulée");
        }

        if (popupImage != null && stampLight != null)
            popupImage.sprite = stampLight;

        if (popup != null)
        {
            popup.gameObject.SetActive(true);
            Debug.Log($"[ClickZonePopup] ShowStamp() → popup activé: {popup.gameObject.activeSelf}");
        }
        else
        {
            Debug.LogWarning("[ClickZonePopup] ShowStamp() → popup est null !");
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log($"[ClickZonePopup] OnPointerUp — _isBlocked: {_isBlocked} | popup actif: {(popup != null ? popup.gameObject.activeSelf.ToString() : "null")}");

        if (_isBlocked)
        {
            Debug.LogWarning("[ClickZonePopup] Clic bloqué (_isBlocked = true)");
            audioEventDispatcher?.PlayAudio(AudioType.Wrong);
            return;
        }

        if (actionMode == ActionMode.ToggleStamp)
        {
            if (!string.IsNullOrEmpty(_stampGroup)
                && _activeByGroup.TryGetValue(_stampGroup, out ClickZonePopup existing)
                && existing != null
                && existing != this)
            {
                audioEventDispatcher?.PlayAudio(AudioType.Wrong);
                return;
            }
        }

        audioEventDispatcher?.PlayAudio(AudioType.Tampon);

        float holdDuration = Time.realtimeSinceStartup - _holdStartTime;
        Sprite selectedSprite = SelectStamp(holdDuration);
        Debug.Log($"[ClickZonePopup] Durée appui: {holdDuration:F2}s → sprite: {(selectedSprite != null ? selectedSprite.name : "null")}");
        ApplyStamp(selectedSprite);

        if (actionMode == ActionMode.ToggleStamp)
        {
            MovePopupToClick(_pressScreenPosition);
            ExecuteToggleStamp();
        }
        else
        {
            MovePopupToClick(_pressScreenPosition);
            popup.gameObject.SetActive(true);
            Debug.Log($"[ClickZonePopup] popup SetActive(true) après clic. Mode: {actionMode}");
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
        if (!string.IsNullOrEmpty(_stampGroup)
            && _activeByGroup.TryGetValue(_stampGroup, out ClickZonePopup previous)
            && previous != null && previous != this)
        {
            if (previous._autoHideCoroutine != null)
            {
                previous.StopCoroutine(previous._autoHideCoroutine);
                previous._autoHideCoroutine = null;
            }
            previous.popup.gameObject.SetActive(false);
        }

        if (_autoHideCoroutine != null)
        {
            StopCoroutine(_autoHideCoroutine);
            _autoHideCoroutine = null;
        }

        popup.gameObject.SetActive(true);

        if (!string.IsNullOrEmpty(_stampGroup))
            _activeByGroup[_stampGroup] = this;
        else
            _activeToggleZone = this;

        if (autoHideDelay > 0f)
            _autoHideCoroutine = StartCoroutine(AutoHideStamp());
    }

    private IEnumerator AutoHideStamp()
    {
        yield return new WaitForSecondsRealtime(autoHideDelay);
        if (popup != null) popup.gameObject.SetActive(false);

        if (!string.IsNullOrEmpty(_stampGroup)
            && _activeByGroup.TryGetValue(_stampGroup, out var current) && current == this)
            _activeByGroup.Remove(_stampGroup);

        _autoHideCoroutine = null;
    }

    /// <summary>Cache le tampon MVP avec un court délai (usage MVPButton uniquement).</summary>
    public void HideMVP()
    {
        Debug.Log($"[ClickZonePopup] HideMVP() appelé sur {gameObject.name}");

        if (_autoHideCoroutine1 != null)
        {
            StopCoroutine(_autoHideCoroutine1);
            _autoHideCoroutine1 = null;
        }
        gameObject.SetActive(true);
        _autoHideCoroutine1 = StartCoroutine(AutoHideStampMvp());
    }

    private IEnumerator AutoHideStampMvp()
    {
        yield return new WaitForSecondsRealtime(0.25f);

        Debug.Log("[ClickZonePopup] AutoHideStampMvp → popup caché");

        if (popup != null)
            popup.gameObject.SetActive(false);

        if (!string.IsNullOrEmpty(_stampGroup)
            && _activeByGroup.TryGetValue(_stampGroup, out var current) && current == this)
            _activeByGroup.Remove(_stampGroup);

        _autoHideCoroutine = null;
    }
    /// <summary>Arrête toutes les coroutines en cours et cache le popup immédiatement.</summary>
    public void ResetStamp()
    {
        if (_actionCoroutine != null)
        {
            StopCoroutine(_actionCoroutine);
            _actionCoroutine = null;
        }

        if (_autoHideCoroutine != null)
        {
            StopCoroutine(_autoHideCoroutine);
            _autoHideCoroutine = null;
        }

        if (_autoHideCoroutine1 != null)
        {
            StopCoroutine(_autoHideCoroutine1);
            _autoHideCoroutine1 = null;
        }

        if (popup != null)
            popup.gameObject.SetActive(false);
    }


    /// <summary>Cache le tampon immédiatement sans animation ni délai.</summary>
    public void HideStamp()
    {
        if (_autoHideCoroutine != null)
        {
            StopCoroutine(_autoHideCoroutine);
            _autoHideCoroutine = null;
        }

        if (popup != null)
            popup.gameObject.SetActive(false);

        if (!string.IsNullOrEmpty(_stampGroup)
            && _activeByGroup.TryGetValue(_stampGroup, out var current) && current == this)
            _activeByGroup.Remove(_stampGroup);

        if (_activeToggleZone == this)
            _activeToggleZone = null;
    }

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
            MusicManager.Instance?.StopAll();
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
                //foreach (Transform child in parentPanel.transform)
                //    child.gameObject.SetActive(false);
            }

            targetPanel.SetActive(true);
        }

        popup.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (_activeToggleZone == this) _activeToggleZone = null;

        if (!string.IsNullOrEmpty(_stampGroup)
            && _activeByGroup.TryGetValue(_stampGroup, out var current) && current == this)
            _activeByGroup.Remove(_stampGroup);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (targetSceneAsset != null)
            targetSceneName = targetSceneAsset.name;
    }
#endif
}
