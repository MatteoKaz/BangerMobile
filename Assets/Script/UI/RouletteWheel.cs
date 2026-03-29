using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Roulette verticale avec recyclage infini des slots.
/// Les items descendent, sortent en bas et réapparaissent en haut.
/// Temps total configurable via totalSpinDuration et decelerationRatio.
/// </summary>
public class RouletteWheel : MonoBehaviour
{
    [Header("Données")]
    [SerializeField] private DataEmploye employeData;
    [SerializeField] private PoleManager poleManager;

    [Header("UI")]
    [SerializeField] private RectTransform    maskRect;
    [SerializeField] private RectTransform    itemsContainer;
    [SerializeField] private RouletteSlot     slotPrefab;
    [SerializeField] private Button           spinButton;

    [Tooltip("Texte affiché après le spin avec le nom de l'employé sélectionné.")]
    [SerializeField] private TextMeshProUGUI  resultLabel;

    [Header("Paramètres")]
    [Tooltip("Hauteur d'un slot en pixels — doit correspondre au Preferred Height du LayoutElement du prefab.")]
    [SerializeField] private float slotHeight = 120f;

    [Tooltip("Vitesse maximale en pixels/seconde.")]
    [SerializeField] private float maxSpeed = 1500f;

    [Tooltip("Durée totale du spin (phase constante + décélération) en secondes.")]
    [SerializeField] private float totalSpinDuration = 4f;

    [Tooltip("Proportion de la durée totale consacrée à la décélération (0 à 1).")]
    [Range(0.1f, 0.9f)]
    [SerializeField] private float decelerationRatio = 0.4f;

    [Header("Highlight")]
    [Tooltip("Couleur appliquée au slot gagnant à l'arrêt.")]
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0f, 1f);

    // ── État interne ──────────────────────────────────────────────────────────

    private readonly List<EmployeDataz>  _employees           = new();
    private readonly List<RouletteSlot>  _slots               = new();
    private readonly List<RectTransform> _slotRects           = new();
    private readonly List<float>         _slotPositions       = new();
    private readonly List<int>           _slotEmployeeIndices = new();

    private float _maskHeight;
    private float _cycleHeight;
    private bool  _isSpinning;
    private int   _selectedIndex;
    private int   _winnerSlotIndex = -1;

    /// <summary>Invoqué à la fin du spin avec l'employé sélectionné.</summary>
    public event System.Action<EmployeDataz> OnEmployeSelected;
    public event System.Action EmployeSelected;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        spinButton.onClick.AddListener(Spin);

        if (resultLabel != null)
            resultLabel.text = string.Empty;
    }

    // ── API publique ──────────────────────────────────────────────────────────

    /// <summary>Lance la roulette. Ignoré si un spin est déjà en cours.</summary>
    public void Spin()
    {
        if (_isSpinning) return;

        BuildAvailableList();

        if (_employees.Count == 0)
        {
            Debug.LogWarning("[RouletteWheel] Aucun employé disponible.");
            return;
        }

        _selectedIndex = Random.Range(0, _employees.Count);

        if (resultLabel != null)
            resultLabel.text = string.Empty;

        BuildSlots();
        StartCoroutine(SpinCoroutine());
    }

    // ── Construction ──────────────────────────────────────────────────────────

    /// <summary>
    /// Charge les employés non encore assignés à un pôle.
    /// Utilise TakenEmployeIndex du PoleManager pour filtrer.
    /// </summary>
    private void BuildAvailableList()
    {
        _employees.Clear();

        HashSet<int> takenIndices = new(poleManager.TakenEmployeIndex);

        for (int i = 0; i < employeData.allEmploye.Count; i++)
        {
            if (!takenIndices.Contains(i))
                _employees.Add(employeData.allEmploye[i]);
        }

        Debug.Log($"[RouletteWheel] {_employees.Count} employés disponibles " +
                  $"({poleManager.TakenEmployeIndex.Count} déjà assignés à des pôles).");

        if (_employees.Count == 0)
            Debug.LogWarning("[RouletteWheel] Tous les employés sont déjà assignés à des pôles.");
    }

    /// <summary>
    /// Crée un pool minimal de slots pour couvrir le masque + 1 buffer haut + 1 buffer bas.
    /// Positions gérées manuellement — aucun VerticalLayoutGroup requis.
    /// </summary>
    private void BuildSlots()
    {
        foreach (RouletteSlot s in _slots) Destroy(s.gameObject);
        _slots.Clear();
        _slotRects.Clear();
        _slotPositions.Clear();
        _slotEmployeeIndices.Clear();

        _maskHeight  = maskRect.rect.height;
        _cycleHeight = _employees.Count * slotHeight;

        int slotCount = Mathf.CeilToInt(_maskHeight / slotHeight) + 2;

        for (int i = 0; i < slotCount; i++)
        {
            // Slot 0 démarre une hauteur au-dessus du masque (caché)
            float posY   = (i - 1) * slotHeight;
            int   empIdx = ((i - 1) % _employees.Count + _employees.Count) % _employees.Count;

            RouletteSlot  slot = Instantiate(slotPrefab, itemsContainer);
            RectTransform rt   = slot.GetComponent<RectTransform>();

            // Stretch horizontal, hauteur fixe
            rt.anchorMin        = new Vector2(0f, 1f);
            rt.anchorMax        = new Vector2(1f, 1f);
            rt.sizeDelta        = new Vector2(0f, slotHeight);
            rt.anchoredPosition = new Vector2(0f, -posY);

            slot.Setup(_employees[empIdx]);
            slot.SetHighlight(false);

            _slots.Add(slot);
            _slotRects.Add(rt);
            _slotPositions.Add(posY);
            _slotEmployeeIndices.Add(empIdx);
        }

        _winnerSlotIndex = -1;
    }

    // ── Défilement ────────────────────────────────────────────────────────────

    /// <summary>
    /// Déplace tous les slots vers le bas de deltaScroll pixels.
    /// Les slots sortant par le bas sont recyclés en haut automatiquement.
    /// </summary>
    private void ScrollSlots(float deltaScroll)
    {
        float bottomBound = _maskHeight + slotHeight * 0.5f;

        for (int i = 0; i < _slots.Count; i++)
        {
            _slotPositions[i] += deltaScroll;

            // Slot sorti par le bas → recycler au-dessus du slot le plus haut
            if (_slotPositions[i] > bottomBound)
            {
                int   topIdx    = GetTopmostSlotIndex(excludeIdx: i);
                float topY      = _slotPositions[topIdx];
                int   topEmpIdx = _slotEmployeeIndices[topIdx];

                _slotPositions[i] = topY - slotHeight;

                if (i == _winnerSlotIndex)
                {
                    // Slot gagnant : conserver l'employé sélectionné
                    _slots[i].Setup(_employees[_selectedIndex]);
                }
                else
                {
                    // Employé précédant celui du slot le plus haut
                    int newEmpIdx = (topEmpIdx - 1 + _employees.Count) % _employees.Count;
                    _slotEmployeeIndices[i] = newEmpIdx;
                    _slots[i].Setup(_employees[newEmpIdx]);
                }

                _slots[i].SetHighlight(false);
            }

            _slotRects[i].anchoredPosition = new Vector2(0f, -_slotPositions[i]);
        }
    }

    // ── Animation principale ──────────────────────────────────────────────────

    private IEnumerator SpinCoroutine()
    {
        _isSpinning             = true;
        spinButton.interactable = false;

        float constantDuration     = totalSpinDuration * (1f - decelerationRatio);
        float decelerationDuration = totalSpinDuration * decelerationRatio;

        // ── Phase 1 : vitesse constante ───────────────────────────────────────
        float elapsed = 0f;
        while (elapsed < constantDuration)
        {
            float dt = Time.unscaledDeltaTime;
            elapsed += dt;
            ScrollSlots(maxSpeed * dt);
            yield return null;
        }

        // ── Préparation décélération ──────────────────────────────────────────
        // Injecter le slot gagnant juste au-dessus du slot le plus haut (hors écran)
        int topIdx    = GetTopmostSlotIndex();
        int bottomIdx = GetBottommostSlotIndex();

        _slotPositions[bottomIdx]       = _slotPositions[topIdx] - slotHeight;
        _slotEmployeeIndices[bottomIdx] = _selectedIndex;
        _slots[bottomIdx].Setup(_employees[_selectedIndex]);
        _slots[bottomIdx].SetHighlight(false);
        _slotRects[bottomIdx].anchoredPosition = new Vector2(0f, -_slotPositions[bottomIdx]);
        _winnerSlotIndex = bottomIdx;

        // Distance pour que le gagnant atteigne exactement le centre du masque
        float centerY      = _maskHeight * 0.5f - slotHeight * 0.5f;
        float winnerY      = _slotPositions[_winnerSlotIndex];
        float distToCenter = centerY - winnerY;

        // Garantir assez de distance pour une décélération visible et fluide
        float minDist = maxSpeed * decelerationDuration * 0.5f;
        while (distToCenter < minDist)
            distToCenter += _cycleHeight;

        // ── Phase 2 : décélération ease-out quadratique ───────────────────────
        float totalDist     = distToCenter;
        float scrolledSoFar = 0f;
        elapsed = 0f;

        while (elapsed < decelerationDuration)
        {
            float dt = Time.unscaledDeltaTime;
            elapsed += dt;
            float t        = Mathf.Clamp01(elapsed / decelerationDuration);
            float progress = 1f - (1f - t) * (1f - t); // ease-out quadratique
            float delta    = progress * totalDist - scrolledSoFar;
            scrolledSoFar  = progress * totalDist;
            ScrollSlots(delta);
            yield return null;
        }

        // Snap précis : aligner le gagnant exactement au centre
        float snapDelta = centerY - _slotPositions[_winnerSlotIndex];
        for (int i = 0; i < _slots.Count; i++)
        {
            _slotPositions[i] += snapDelta;
            _slotRects[i].anchoredPosition = new Vector2(0f, -_slotPositions[i]);
        }

        // ── Résultat ──────────────────────────────────────────────────────────
        _slots[_winnerSlotIndex].SetHighlight(true, highlightColor);

        if (resultLabel != null)
            resultLabel.text = _employees[_selectedIndex].EmployeName;

        _isSpinning             = false;
        spinButton.interactable = true;

        OnEmployeSelected?.Invoke(_employees[_selectedIndex]);
        EmployeSelected?.Invoke();
        Debug.Log($"[RouletteWheel] Sélectionné : {_employees[_selectedIndex].EmployeName}");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Index du slot le plus haut visuellement (position Y minimale).</summary>
    private int GetTopmostSlotIndex(int excludeIdx = -1)
    {
        int   idx = -1;
        float min = float.MaxValue;

        for (int i = 0; i < _slotPositions.Count; i++)
        {
            if (i == excludeIdx) continue;
            if (_slotPositions[i] < min) { min = _slotPositions[i]; idx = i; }
        }

        return idx;
    }

    /// <summary>Index du slot le plus bas visuellement (position Y maximale).</summary>
    private int GetBottommostSlotIndex()
    {
        int   idx = 0;
        float max = float.MinValue;

        for (int i = 0; i < _slotPositions.Count; i++)
        {
            if (_slotPositions[i] > max) { max = _slotPositions[i]; idx = i; }
        }

        return idx;
    }
}
