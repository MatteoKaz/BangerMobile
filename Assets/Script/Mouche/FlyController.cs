using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class FlyController : MonoBehaviour
{
    private const string PrefKeySquashed   = "Fly_IsSquashed";
    private const string PrefKeyPosX       = "Fly_PosX";
    private const string PrefKeyPosY       = "Fly_PosY";
    private const string PrefKeyPanelIndex = "Fly_PanelIndex";

    // Remise à zéro à chaque lancement de l'application, survit aux rechargements de scène.
    private static bool _sessionInitialized = false;

    [Header("Sprites")]
    [SerializeField] private Sprite[] _flyingSprites;
    [SerializeField] private Sprite _idleSprite;
    [SerializeField] private Sprite[] _squashSprites;
    [SerializeField] private float _squashFrameDuration = 0.06f;

    [Header("Mouvement")]
    [SerializeField] private float _moveSpeed    = 2f;
    [SerializeField] private float _minMoveTime  = 0.5f;
    [SerializeField] private float _maxMoveTime  = 2f;
    [SerializeField] private float _minIdleTime  = 0.5f;
    [SerializeField] private float _maxIdleTime  = 2f;
    [SerializeField] private Vector2 _boundsMin  = new Vector2(-2f, -4.5f);
    [SerializeField] private Vector2 _boundsMax  = new Vector2(2f,   4.5f);

    [Header("Animation")]
    [SerializeField] private float _frameDuration = 0.08f;

    [Header("Fuite")]
    [SerializeField] private float _fleeSpeed    = 6f;
    [SerializeField] private float _fleeDistance = 3f;

    [Header("Zones de tampon")]
    [Tooltip("Glisser ici les RectTransform des zones que la mouche doit fréquenter.")]
    [SerializeField] private RectTransform[] _stampZones;
    [Range(0f, 1f)]
    [SerializeField] private float _stampZoneBias      = 0.6f;
    [SerializeField] private float _stampZoneVariation = 0.3f;

    [Header("Panels")]
    [Tooltip("Transform du panel principal (menu de base, toujours présent).")]
    [SerializeField] private Transform _mainPanel;
    [Tooltip("Glisser ici les FlyPanelTracker de chaque panel (PanelOptions, PanelCredits, PanelAide...).")]
    [SerializeField] private FlyPanelTracker[] _panelTrackers;

    private SpriteRenderer _spriteRenderer;
    private bool _isMoving  = false;
    private bool _isTapped  = false;
    private bool _isFleeing = false;
    private Vector2 _targetPosition;

    // Panel sur lequel la mouche a été écrasée (null = menu principal)
    private FlyPanelTracker _squashedOnPanel = null;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();

        if (!_sessionInitialized)
        {
            _sessionInitialized = true;
            ClearSavedState();
        }
    }

    private void Start()
    {
        if (LoadSquashedState())
            RestoreSquashedFly();
        else
            StartLiveFly();
    }

    // -------------------------------------------------------------------------
    // Persistence
    // -------------------------------------------------------------------------

    /// <summary>Retourne true si une mouche écrasée est sauvegardée pour cette session.</summary>
    private bool LoadSquashedState()
    {
        return PlayerPrefs.GetInt(PrefKeySquashed, 0) == 1;
    }

    /// <summary>
    /// Restaure la mouche dans son état écrasé :
    /// position sauvegardée, sprite final, panel d'origine, et reparenting.
    /// </summary>
    private void RestoreSquashedFly()
    {
        _isTapped = true;

        float x = PlayerPrefs.GetFloat(PrefKeyPosX, transform.position.x);
        float y = PlayerPrefs.GetFloat(PrefKeyPosY, transform.position.y);
        transform.position = new Vector3(x, y, transform.position.z);

        int panelIndex = PlayerPrefs.GetInt(PrefKeyPanelIndex, -1);
        _squashedOnPanel = ResolvePanel(panelIndex);

        if (_squashSprites != null && _squashSprites.Length > 0)
            _spriteRenderer.sprite = _squashSprites[_squashSprites.Length - 1];

        ReparentToActivePanel();
        RefreshVisibility();
    }

    /// <summary>Sauvegarde l'état écrasé dans PlayerPrefs.</summary>
    private void SaveSquashedState()
    {
        PlayerPrefs.SetInt(PrefKeySquashed, 1);
        PlayerPrefs.SetFloat(PrefKeyPosX, transform.position.x);
        PlayerPrefs.SetFloat(PrefKeyPosY, transform.position.y);
        PlayerPrefs.SetInt(PrefKeyPanelIndex, ResolveIndex(_squashedOnPanel));
        PlayerPrefs.Save();
    }

    /// <summary>Efface l'état sauvegardé — appelé au premier lancement de chaque session.</summary>
    private void ClearSavedState()
    {
        PlayerPrefs.DeleteKey(PrefKeySquashed);
        PlayerPrefs.DeleteKey(PrefKeyPosX);
        PlayerPrefs.DeleteKey(PrefKeyPosY);
        PlayerPrefs.DeleteKey(PrefKeyPanelIndex);
        PlayerPrefs.Save();
    }

    /// <summary>Convertit un index sauvegardé en FlyPanelTracker (-1 = menu principal).</summary>
    private FlyPanelTracker ResolvePanel(int index)
    {
        if (index < 0 || _panelTrackers == null || index >= _panelTrackers.Length)
            return null;

        return _panelTrackers[index];
    }

    /// <summary>Convertit un FlyPanelTracker en index à sauvegarder (-1 = menu principal).</summary>
    private int ResolveIndex(FlyPanelTracker tracker)
    {
        if (tracker == null || _panelTrackers == null) return -1;

        for (int i = 0; i < _panelTrackers.Length; i++)
        {
            if (_panelTrackers[i] == tracker)
                return i;
        }

        return -1;
    }

    // -------------------------------------------------------------------------
    // Cycle de vie (mouche vivante)
    // -------------------------------------------------------------------------

    /// <summary>Lance les coroutines de mouvement et d'animation pour la mouche vivante.</summary>
    private void StartLiveFly()
    {
        StartCoroutine(MovementCycle());
        StartCoroutine(AnimationCycle());
    }

    /// <summary>Cycle principal : alterne entre déplacement et pause.</summary>
    private IEnumerator MovementCycle()
    {
        while (!_isTapped)
        {
            while (_isFleeing) yield return null;

            _isMoving = true;
            _targetPosition = PickNextTarget();

            float moveDuration = Random.Range(_minMoveTime, _maxMoveTime);
            float moveTimer    = 0f;

            while (moveTimer < moveDuration && !_isTapped && !_isFleeing)
            {
                transform.position = Vector2.MoveTowards(
                    transform.position,
                    _targetPosition,
                    _moveSpeed * Time.deltaTime
                );
                moveTimer += Time.deltaTime;
                yield return null;
            }

            if (_isFleeing) continue;

            _isMoving = false;
            float idleTimer    = 0f;
            float idleDuration = Random.Range(_minIdleTime, _maxIdleTime);

            while (idleTimer < idleDuration && !_isTapped && !_isFleeing)
            {
                idleTimer += Time.deltaTime;
                yield return null;
            }
        }
    }

    /// <summary>Choisit la prochaine destination avec biais vers les zones de tampon.</summary>
    private Vector2 PickNextTarget()
    {
        if (_stampZones != null && _stampZones.Length > 0 && Random.value < _stampZoneBias)
        {
            RectTransform zone = _stampZones[Random.Range(0, _stampZones.Length)];
            if (zone != null)
                return (Vector2)zone.position + Random.insideUnitCircle * _stampZoneVariation;
        }

        return new Vector2(
            Random.Range(_boundsMin.x, _boundsMax.x),
            Random.Range(_boundsMin.y, _boundsMax.y)
        );
    }

    /// <summary>Cycle d'animation : alterne les sprites selon l'état.</summary>
    private IEnumerator AnimationCycle()
    {
        int frameIndex = 0;
        while (!_isTapped)
        {
            if (_isMoving && _flyingSprites.Length > 0)
            {
                _spriteRenderer.sprite = _flyingSprites[frameIndex % _flyingSprites.Length];
                frameIndex++;
                yield return new WaitForSeconds(_frameDuration);
            }
            else
            {
                _spriteRenderer.sprite = _idleSprite != null ? _idleSprite : _spriteRenderer.sprite;
                frameIndex = 0;
                yield return null;
            }
        }
    }

    // -------------------------------------------------------------------------
    // Interactions
    // -------------------------------------------------------------------------

    /// <summary>Appelé quand le joueur tape directement la mouche.</summary>
    public void OnTapped()
    {
        if (_isTapped) return;

        _isTapped  = true;
        _isMoving  = false;
        _isFleeing = false;
        StopAllCoroutines();

        _squashedOnPanel = GetActivePanel();
        SaveSquashedState();
        ReparentToActivePanel();

        StartCoroutine(SquashAnimation());
    }

    /// <summary>Déclenche la fuite de la mouche à l'opposé de la position de menace.</summary>
    public void Flee(Vector2 threatPosition)
    {
        if (_isTapped || _isFleeing) return;

        _isFleeing = true;
        StartCoroutine(FleeCycle(threatPosition));
    }

    /// <summary>Déplace la mouche à l'opposé du tap, puis reprend le cycle normal.</summary>
    private IEnumerator FleeCycle(Vector2 threatPosition)
    {
        _isMoving = true;

        Vector2 fleeDirection = ((Vector2)transform.position - threatPosition).normalized;
        if (fleeDirection == Vector2.zero)
            fleeDirection = Random.insideUnitCircle.normalized;

        Vector2 fleeTarget = (Vector2)transform.position + fleeDirection * _fleeDistance;
        fleeTarget.x = Mathf.Clamp(fleeTarget.x, _boundsMin.x, _boundsMax.x);
        fleeTarget.y = Mathf.Clamp(fleeTarget.y, _boundsMin.y, _boundsMax.y);

        while (!_isTapped && Vector2.Distance(transform.position, fleeTarget) > 0.05f)
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                fleeTarget,
                _fleeSpeed * Time.deltaTime
            );
            yield return null;
        }

        _isMoving  = false;
        _isFleeing = false;
    }

    /// <summary>Joue la séquence de sprites d'écrasement puis reste sur le dernier frame.</summary>
    private IEnumerator SquashAnimation()
    {
        if (_squashSprites == null || _squashSprites.Length == 0) yield break;

        foreach (Sprite frame in _squashSprites)
        {
            _spriteRenderer.sprite = frame;
            yield return new WaitForSeconds(_squashFrameDuration);
        }

        _spriteRenderer.sprite = _squashSprites[_squashSprites.Length - 1];
    }

    // -------------------------------------------------------------------------
    // Reparenting
    // -------------------------------------------------------------------------

    /// <summary>
    /// Réattache la mouche comme enfant du panel actif au moment de l'écrasement.
    /// Si aucun panel n'est actif, elle devient enfant de _mainPanel.
    /// worldPositionStays: true préserve la position mondiale.
    /// </summary>
    private void ReparentToActivePanel()
    {
        Transform target = _squashedOnPanel != null
            ? _squashedOnPanel.PanelTransform
            : _mainPanel;

        if (target != null)
            transform.SetParent(target, worldPositionStays: true);
    }

    // -------------------------------------------------------------------------
    // Visibilité selon panels
    // -------------------------------------------------------------------------

    /// <summary>Retourne le FlyPanelTracker du panel actuellement actif, ou null si menu principal.</summary>
    private FlyPanelTracker GetActivePanel()
    {
        if (_panelTrackers == null) return null;

        foreach (FlyPanelTracker tracker in _panelTrackers)
        {
            if (tracker != null && tracker.gameObject.activeInHierarchy)
                return tracker;
        }

        return null;
    }

    /// <summary>Retourne true si au moins un panel tracké est actuellement actif.</summary>
    private bool IsAnyPanelActive()
    {
        if (_panelTrackers == null) return false;

        foreach (FlyPanelTracker tracker in _panelTrackers)
        {
            if (tracker != null && tracker.gameObject.activeInHierarchy)
                return true;
        }

        return false;
    }

    /// <summary>Recalcule et applique la visibilité selon l'état courant des panels.</summary>
    private void RefreshVisibility()
    {
        if (_squashedOnPanel != null)
            _spriteRenderer.enabled = _squashedOnPanel.gameObject.activeInHierarchy;
        else
            _spriteRenderer.enabled = !IsAnyPanelActive();
    }

    /// <summary>
    /// Appelé par FlyPanelTracker quand un panel devient visible.
    /// Si c'est le panel d'origine → affiche la mouche.
    /// Si c'est un autre panel    → cache la mouche.
    /// </summary>
    public void OnPanelShown(FlyPanelTracker panel)
    {
        if (!_isTapped) return;

        _spriteRenderer.enabled = (panel == _squashedOnPanel);
    }

    /// <summary>
    /// Appelé par FlyPanelTracker quand un panel est masqué.
    /// Réévalue la visibilité après chaque fermeture de panel.
    /// </summary>
    public void OnPanelHidden(FlyPanelTracker panel)
    {
        if (!_isTapped) return;

        RefreshVisibility();
    }
}
