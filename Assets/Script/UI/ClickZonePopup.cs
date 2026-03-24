using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Affiche un tampon au relâchement dont l'intensité dépend de la durée d'appui (3 niveaux).
/// Charge la scène cible après un délai.
/// </summary>
[RequireComponent(typeof(Image))]
public class ClickZonePopup : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
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

    [Header("Chargement de scène")]
    [Tooltip("Délai en secondes après relâchement avant le chargement.")]
    [SerializeField] private float delayBeforeLoad = 1.5f;

#if UNITY_EDITOR
    [SerializeField] private SceneAsset targetSceneAsset;
#endif
    [HideInInspector]
    [SerializeField] private string targetSceneName;

    private Canvas _canvas;
    private Coroutine _loadCoroutine;
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

        // Annule un chargement en cours si le joueur rappuie
        if (_loadCoroutine != null)
        {
            StopCoroutine(_loadCoroutine);
            _loadCoroutine = null;
            popup.gameObject.SetActive(false);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        float holdDuration = Time.time - _holdStartTime;

        Sprite selectedStamp = SelectStamp(holdDuration);

        MovePopupToClick(_pressScreenPosition);
        ApplyStamp(selectedStamp);
        popup.gameObject.SetActive(true);

        _loadCoroutine = StartCoroutine(LoadSceneAfterDelay());
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

    /// <summary>Attend le délai puis charge la scène cible.</summary>
    private IEnumerator LoadSceneAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeLoad);

        if (!string.IsNullOrEmpty(targetSceneName))
            UnityEngine.SceneManagement.SceneManager.LoadScene(targetSceneName);
        else
            Debug.LogWarning("ClickZonePopup : aucune scène cible assignée.", this);
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (targetSceneAsset != null)
            targetSceneName = targetSceneAsset.name;
    }
#endif
}
