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
/// Selon le mode choisi dans l'Inspector, charge une scène ou toggle un panel après un délai.
/// </summary>
[RequireComponent(typeof(Image))]
public class ClickZonePopup : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public enum ActionMode
    {
        LoadScene,
        OpenPanel
    }

    [Header("Action")]
    [Tooltip("LoadScene : charge une scène. OpenPanel : toggle un panel UI.")]
    [SerializeField] private ActionMode actionMode = ActionMode.LoadScene;

    [Header("Popup")]
    [SerializeField] private RectTransform popup;
    [SerializeField] private Image popupImage;

    [Header("Sprites du tampon (du plus clair au plus foncé)")]
    [SerializeField] private Sprite stampLight;
    [SerializeField] private Sprite stampMedium;
    [SerializeField] private Sprite stampDark;

    [Header("Seuils de durée (secondes)")]
    [Tooltip("Durée minimale pour le tampon moyen.")]
    [SerializeField] private float thresholdMedium = 0.4f;
    [Tooltip("Durée minimale pour le tampon foncé.")]
    [SerializeField] private float thresholdDark = 0.9f;

    [Header("Délai avant action (secondes)")]
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
    [Tooltip("(Optionnel) Conteneur dont les enfants frères seront désactivés avant d'ouvrir targetPanel. " +
             "Ne pas assigner le même objet que targetPanel.")]
    [SerializeField] private GameObject parentPanel;

    private Canvas _canvas;
    private Coroutine _actionCoroutine;
    private float _holdStartTime;
    private Vector2 _pressScreenPosition;

    private void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();

        if (popup != null)
            popup.gameObject.SetActive(false);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _holdStartTime = Time.time;
        _pressScreenPosition = eventData.position;

        if (_actionCoroutine != null)
        {
            StopCoroutine(_actionCoroutine);
            _actionCoroutine = null;
            popup.gameObject.SetActive(false);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        float holdDuration = Time.time - _holdStartTime;

        MovePopupToClick(_pressScreenPosition);
        ApplyStamp(SelectStamp(holdDuration));
        popup.gameObject.SetActive(true);

        _actionCoroutine = StartCoroutine(ExecuteActionAfterDelay());
    }

    /// <summary>Retourne le sprite correspondant à la durée d'appui.</summary>
    private Sprite SelectStamp(float duration)
    {
        if (duration >= thresholdDark)   return stampDark;
        if (duration >= thresholdMedium) return stampMedium;
        return stampLight;
    }

    /// <summary>Déplace le popup pour que son centre soit à la position du clic.</summary>
    private void MovePopupToClick(Vector2 screenPosition)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.GetComponent<RectTransform>(),
                screenPosition,
                _canvas.worldCamera,
                out Vector2 localPoint))
        {
            popup.anchoredPosition = localPoint;
        }
    }

    private void ApplyStamp(Sprite sprite)
    {
        if (popupImage != null)
            popupImage.sprite = sprite;
    }

    /// <summary>Attend le délai puis exécute l'action configurée.</summary>
    private IEnumerator ExecuteActionAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeAction);

        switch (actionMode)
        {
            case ActionMode.LoadScene: ExecuteLoadScene(); break;
            case ActionMode.OpenPanel: ExecuteOpenPanel(); break;
        }
    }

    private void ExecuteLoadScene()
    {
        if (!string.IsNullOrEmpty(targetSceneName))
            SceneManager.LoadScene(targetSceneName);
        else
            Debug.LogWarning("ClickZonePopup : aucune scène cible assignée.", this);
    }

    /// <summary>Toggle le panel cible. Si un parentPanel différent est assigné,
    /// désactive ses enfants frères avant d'ouvrir targetPanel.</summary>
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
            // Ne désactive les frères que si parentPanel est un conteneur distinct de targetPanel
            if (parentPanel != null && parentPanel != targetPanel)
            {
                foreach (Transform child in parentPanel.transform)
                    child.gameObject.SetActive(false);
            }

            targetPanel.SetActive(true);
        }

        popup.gameObject.SetActive(false);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (targetSceneAsset != null)
            targetSceneName = targetSceneAsset.name;
    }
#endif
}
