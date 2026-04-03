using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// Change le sprite et la taille du SpriteRenderer de la pile selon le nombre de papiers.
/// Anime un punch de scale à chaque changement de count.
/// Contre-scale le Canvas enfant pour que le compteur garde toujours la même taille.
/// </summary>

public class PileBackEmploye : MonoBehaviour
{
    [System.Serializable]
    public struct PileThreshold
    {
        [Tooltip("Nombre minimum de papiers pour afficher ce sprite.")]
        public int minCount;
        public Sprite sprite;
        [Tooltip("Scale appliqué au GameObject pour ce seuil.")]
        public float scale;
    }

    [System.Serializable]
    public struct PoleThresholdSet
    {
        public PoleType poleType;
        public PileThreshold[] thresholds;
    }
    private const float PunchScaleMultiplier = 1.2f;
    private const float PunchGrowDuration = 0.12f;
    private const float PunchShrinkDuration = 0.2f;

    [SerializeField] private Tuyaux tuyauxGreen;
    [SerializeField] private Tuyaux tuyauxRed;
    [SerializeField] private Tuyaux tuyauxBlue;

    private Tuyaux _activeTuyaux;
    [SerializeField] private Employe employe;
    [SerializeField] private PoleThresholdSet[] thresholdsByPole;

    [Tooltip("Canvas enfant contenant le compteur — son scale sera compensé automatiquement.")]
    [SerializeField] private Transform counterCanvas;

    private Image _spriteRenderer;
    private Vector3 _originalCanvasLocalScale;
    private Vector3 _targetScale;
    private Coroutine _punchCoroutine;

    private void Awake()
    {
        _spriteRenderer = GetComponent<Image>();

        if (counterCanvas != null)
            _originalCanvasLocalScale = counterCanvas.localScale;
        

    }

    private void Start()
    {
        StartCoroutine(InitAfterFrame());
    }

    private IEnumerator InitAfterFrame()
    {
        yield return new WaitForSeconds(1f);
       
        OnPoleChanged();
    }
    private void OnDestroy()
    {
        if (_activeTuyaux  != null)
        _activeTuyaux.AddPaperUi -= OnCountUpdated;
    }

    public void OnDisable()
    {
        if (_activeTuyaux != null)
        _activeTuyaux.AddPaperUi -= OnCountUpdated;
        if (employe?.mypole != null)
            employe.mypole.UpdatePaperFond -= UpdateVisual;
    }
    private Tuyaux GetTuyauxForPole()
    {
        if (employe?.mypole == null)
        {
            Debug.LogWarning("[PileBackEmploye] employe ou mypole est null");
            return null;
        }

        return employe.mypole.type switch
        {
            PoleType.GreenPole => tuyauxGreen,
            PoleType.RedPole => tuyauxRed,
            PoleType.BluePole => tuyauxBlue,
            _ => null
        };
    }
    public void OnCountUpdated()
    {
        
        PlayPunch();
        UpdateVisual();
    }

    /// <summary>Sélectionne le sprite et le scale correspondant au paperCount actuel.</summary>
    private void UpdateVisual()
    {
        if (employe?.mypole == null) return;
        GetTuyauxForPole();
        PileThreshold[] thresholds = GetThresholdsForCurrentPole();
        if (thresholds == null || thresholds.Length == 0) return;

        int count = employe.mypole.waitingPaper;
        Debug.LogWarning($"papier attente : {count}");
        Sprite selected = thresholds[0].sprite;
        float selectedScale = thresholds[0].scale;

        foreach (PileThreshold threshold in thresholds)
        {
            if (count >= threshold.minCount)
            {
                selected = threshold.sprite;
                selectedScale = threshold.scale;
            }
        }

        _spriteRenderer.sprite = selected;
        _targetScale = Vector3.one * selectedScale;
        ApplyScale(_targetScale);
    }

    /// <summary>Lance l'animation de punch : grossit puis revient à _targetScale.</summary>
    private void PlayPunch()
    {
        if (_punchCoroutine != null)
            StopCoroutine(_punchCoroutine);

        _punchCoroutine = StartCoroutine(PunchRoutine());
    }

    private IEnumerator PunchRoutine()
    {
        Vector3 punchTarget = _targetScale * PunchScaleMultiplier;

        // Grossissement
        yield return StartCoroutine(ScaleRoutine(punchTarget, PunchGrowDuration));

        // Retour à la taille normale
        yield return StartCoroutine(ScaleRoutine(_targetScale, PunchShrinkDuration));

        _punchCoroutine = null;
    }

    private IEnumerator ScaleRoutine(Vector3 target, float duration)
    {
        Vector3 start = transform.localScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            t = t * t * (3f - 2f * t);
            ApplyScale(Vector3.Lerp(start, target, t));
            yield return null;
        }

        ApplyScale(target);
    }

    /// <summary>Applique le scale à la pile et compense sur le Canvas pour garder le texte constant.</summary>
    private void ApplyScale(Vector3 scale)
    {
        transform.localScale = scale;

        if (counterCanvas != null && scale.x != 0f)
            counterCanvas.localScale = _originalCanvasLocalScale / scale.x;
    }
    

    // Remplace l'ancien tableau 'thresholds'
    private PileThreshold[] GetThresholdsForCurrentPole()
    {
        if (employe?.mypole == null) return null;

        PoleType currentType = employe.mypole.type;

        foreach (PoleThresholdSet set in thresholdsByPole)
        {
            if (set.poleType == currentType)
                return set.thresholds;
        }

        Debug.LogWarning($"[PileBackEmploye] Aucun threshold trouvé pour le pôle {currentType}");
        return null;
    }

    public void OnPoleChanged()
    {
        // Désabonne l'ancien
        if (_activeTuyaux != null)
            _activeTuyaux.AddPaperUi -= OnCountUpdated;
        if (employe?.mypole != null)
            employe.mypole.UpdatePaperFond -= UpdateVisual;

        // Assigne toujours le nouveau tuyau
        _activeTuyaux = GetTuyauxForPole();

        // Abonne au nouveau
        if (_activeTuyaux != null)
            _activeTuyaux.AddPaperUi += OnCountUpdated;
        if (employe?.mypole != null)
            employe.mypole.UpdatePaperFond += UpdateVisual;

        UpdateVisual();
    }
}
