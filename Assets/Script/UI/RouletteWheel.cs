using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Random = UnityEngine.Random;

/// <summary>
/// Roulette verticale avec recyclage infini des slots.
/// Les items descendent, sortent en bas et réapparaissent en haut.
/// La roulette ne peut être lancée qu'une seule fois par jour.
/// </summary>
public class RouletteWheel : MonoBehaviour
{
    [Header("Données")]
    [SerializeField] private DataEmploye employeData;
    [SerializeField] private PoleManager poleManager;
    [SerializeField] private DayManager dayManager;

    [Header("UI")]
    [SerializeField] private RectTransform maskRect;
    [SerializeField] private RectTransform itemsContainer;
    [SerializeField] private RouletteSlot slotPrefab;
    [SerializeField] private Button spinButton;

    [Tooltip("Parent hors du Mask où le slot gagnant sera reparenté pour s'afficher par-dessus.")]
    [SerializeField] private RectTransform overlayContainer;

    [Tooltip("Texte affiché après la pause de révélation avec le nom de l'employé sélectionné.")]
    [SerializeField] private TextMeshProUGUI resultLabel;

    [Tooltip("RectTransform du CenterMarker — définit la position Y d'arrêt du slot gagnant.")]
    [SerializeField] private RectTransform winnerTargetMarker;

    [Header("Paramètres")]
    [Tooltip("Hauteur d'un slot en pixels (sizeDelta Y appliqué au RectTransform du slot).")]
    [SerializeField] private float slotHeight = 120f;

    [Tooltip("Vitesse maximale en pixels/seconde.")]
    [SerializeField] private float maxSpeed = 1500f;

    [Tooltip("Durée de la phase à vitesse constante en secondes.")]
    [SerializeField] private float constantDuration = 2.5f;

    [Tooltip("Durée de la décélération en secondes.")]
    [SerializeField] private float decelerationDuration = 2f;

    [Tooltip("Durée de la pause après l'arrêt, avant d'afficher le résultat.")]
    [SerializeField] private float revealDelay = 1.5f;

    [Header("Highlight")]
    [Tooltip("Couleur appliquée au slot gagnant à l'arrêt.")]
    [SerializeField] private Color highlightColor = new Color(1f, 0.85f, 0f, 1f);

    [Header("Tremblement")]
    [Tooltip("RectTransform à faire trembler — doit avoir des anchors fixes (pas stretch). Si vide, aucun tremblement.")]
    [SerializeField] private RectTransform shakeTarget;

    [Tooltip("Amplitude maximale du tremblement en pixels, atteinte à vitesse maximale.")]
    [SerializeField] private float maxShakeAmplitude = 8f;

    [Tooltip("Fréquence du tremblement en Hz (oscillations par seconde).")]
    [SerializeField] private float shakeFrequency = 30f;

    [Tooltip("Angle maximal de rotation (degrés) du CenterMarker pendant le spin.")]
    [SerializeField] private float maxShakeRotation = 15f;

    [Tooltip("Fréquence de la rotation oscillante en Hz.")]
    [SerializeField] private float shakeRotationFrequency = 8f;

    [Header("Lumières")]
    [Tooltip("Lumières 2D qui alternent en séquence pendant le spin.")]
    [SerializeField] private List<Light2D> spinLights = new();

    [Tooltip("Fréquence d'alternance à vitesse maximale (changements de lumière par seconde).")]
    [SerializeField] private float maxLightFrequency = 12f;

    [Tooltip("Fréquence d'alternance à vitesse minimale (début de décélération).")]
    [SerializeField] private float minLightFrequency = 2f;

    [Header("Pulse du gagnant")]
    [Tooltip("Scale maximal atteint lors du grossissement du slot gagnant.")]
    [SerializeField] private float pulseMaxScale = 1.15f;

    [Tooltip("Scale minimal atteint lors du dégrossissement du slot gagnant.")]
    [SerializeField] private float pulseMinScale = 0.95f;

    [Tooltip("Durée d'un demi-cycle de pulse (grossir → dégrossir) en secondes.")]
    [SerializeField] private float pulseHalfDuration = 0.35f;

    // ── État interne ──────────────────────────────────────────────────────────

    private readonly List<EmployeDataz> _employees = new();
    private readonly List<int> _employeeGlobalIndices = new();
    private readonly HashSet<int> _rouletteWinnerIndices = new();
    private readonly List<RouletteSlot> _slots = new();
    private readonly List<RectTransform> _slotRects = new();
    private readonly List<float> _slotPositions = new();
    private readonly List<int> _slotEmployeeIndices = new();

    private float _maskHeight;
    private float _cycleHeight;
    private float _stepHeight;

    private bool _isSpinning;
    private bool _hasSpunToday;
    private int _selectedIndex;

    private float _shakeTime;
    private Vector2 _shakeBasePosition;
    private Coroutine _pulseCoroutine;
    private RouletteSlot _promotedSlot;

    private int _currentLightIndex;
    private float _lightTimer;

    [Header("Audio")]
    [SerializeField] private AudioEventDispatcher audioEventDispatcher;
    [SerializeField] private AudioSource spinLoopSource;

    /// <summary>Invoqué après la pause de révélation avec l'employé sélectionné.</summary>
    public event Action<EmployeDataz> OnEmployeSelected;
    public event Action EmployeSelected;

    // ── Unity ─────────────────────────────────────────────────────────────────

    private void Awake()
    {
        spinButton.onClick.AddListener(Spin);

        if (resultLabel != null)
            resultLabel.text = string.Empty;

        if (shakeTarget != null)
            _shakeBasePosition = shakeTarget.anchoredPosition;

        if (dayManager != null)
            dayManager.ResetValueBeforeNextDay += ResetDailySpin;
    }

    /// <summary>
    /// À chaque activation du panneau, remet à zéro les gagnants précédents
    /// et recalcule la liste depuis TakenEmployeIndex en temps réel.
    /// </summary>
    private void OnEnable()
    {
        _rouletteWinnerIndices.Clear();
        BuildAvailableList();
    }

    private void OnDisable()
    {
        StopSpinLoop();
        TurnOffAllLights();
    }

    private void OnDestroy()
    {
        if (dayManager != null)
            dayManager.ResetValueBeforeNextDay -= ResetDailySpin;
    }

    // ── API publique ──────────────────────────────────────────────────────────

    /// <summary>
    /// Réinitialise le droit de lancer la roulette pour le nouveau jour
    /// et met à jour la liste des employés disponibles en excluant ceux dans les pôles.
    /// </summary>
    public void ResetDailySpin()
    {
        _hasSpunToday = false;
        spinButton.interactable = true;

        _rouletteWinnerIndices.Clear();
        BuildAvailableList();

        Debug.Log($"[RouletteWheel] Nouveau jour — {_employees.Count} employés disponibles.");
    }

    /// <summary>Lance la roulette. Ignoré si un spin est en cours ou déjà fait aujourd'hui.</summary>
    public void Spin()
    {
        audioEventDispatcher.PlayAudio(AudioType.Spin);
        if (_isSpinning) return;

        if (_hasSpunToday)
        {
            Debug.LogWarning("[RouletteWheel] La roulette a déjà été lancée aujourd'hui.");
            return;
        }

        if (_employees.Count == 0)
        {
            Debug.LogWarning("[RouletteWheel] Aucun employé disponible pour la roulette.");
            return;
        }

        _selectedIndex = Random.Range(0, _employees.Count);

        if (resultLabel != null)
            resultLabel.text = string.Empty;

        StopPulse();
        DemoteWinnerSlot();
        BuildSlots();
        StartCoroutine(SpinCoroutine());
    }

    // ── Construction ──────────────────────────────────────────────────────────

    /// <summary>
    /// Construit la liste des employés disponibles pour la roulette.
    /// Exclut les employés assignés à un pôle (TakenEmployeIndex)
    /// et ceux déjà gagnés à la roulette ce cycle (_rouletteWinnerIndices).
    /// </summary>
    private void BuildAvailableList()
    {
        _employees.Clear();
        _employeeGlobalIndices.Clear();

        HashSet<int> takenByPoles = (poleManager != null && poleManager.TakenEmployeIndex != null)
            ? new HashSet<int>(poleManager.TakenEmployeIndex)
            : new HashSet<int>();

        for (int i = 0; i < employeData.allEmploye.Count; i++)
        {
            if (!takenByPoles.Contains(i) && !_rouletteWinnerIndices.Contains(i))
            {
                _employees.Add(employeData.allEmploye[i]);
                _employeeGlobalIndices.Add(i);
            }
        }

        Debug.Log($"[RouletteWheel] {_employees.Count} disponibles " +
                  $"({takenByPoles.Count} dans pôles, {_rouletteWinnerIndices.Count} gagnés roulette).");
    }

    /// <summary>
    /// Enregistre le gagnant dans la liste interne de la roulette.
    /// N'affecte pas TakenEmployeIndex — seuls les pôles gèrent cette liste.
    /// </summary>
    private void RegisterWinnerAsTaken(int localSelectedIndex)
    {
        int globalIndex = _employeeGlobalIndices[localSelectedIndex];
        _rouletteWinnerIndices.Add(globalIndex);
        Debug.Log($"[RouletteWheel] Gagnant enregistré : index {globalIndex} " +
                  $"({_employees[localSelectedIndex].EmployeName}).");
    }

    /// <summary>Crée le pool de slots qui couvre le masque + buffers haut et bas.</summary>
    private void BuildSlots()
    {
        foreach (RouletteSlot s in _slots) Destroy(s.gameObject);
        _slots.Clear();
        _slotRects.Clear();
        _slotPositions.Clear();
        _slotEmployeeIndices.Clear();

        _maskHeight = maskRect.rect.height;
        _stepHeight = slotHeight;
        _cycleHeight = _employees.Count * _stepHeight;

        float prefabScaleY = Mathf.Abs(slotPrefab.transform.localScale.y);
        float localSlotHeight = (prefabScaleY > 0f) ? slotHeight / prefabScaleY : slotHeight;
        int slotCount = Mathf.CeilToInt(_maskHeight / _stepHeight) + 2;

        for (int i = 0; i < slotCount; i++)
        {
            float posY = (i - 1) * _stepHeight + _stepHeight * 0.5f;
            int empIdx = ((i - 1) % _employees.Count + _employees.Count) % _employees.Count;

            RouletteSlot slot = Instantiate(slotPrefab, itemsContainer);
            RectTransform rt = slot.GetComponent<RectTransform>();

            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.sizeDelta = new Vector2(0f, localSlotHeight);
            rt.anchoredPosition = new Vector2(0f, -posY);

            slot.Setup(_employees[empIdx]);
            slot.SetHighlight(false);

            _slots.Add(slot);
            _slotRects.Add(rt);
            _slotPositions.Add(posY);
            _slotEmployeeIndices.Add(empIdx);
        }

        Debug.Log($"[RouletteWheel] {slotCount} slots créés, maskHeight={_maskHeight}px.");
    }

    // ── Tremblement ───────────────────────────────────────────────────────────

    /// <summary>
    /// Applique un tremblement horizontal, vertical et une rotation oscillante
    /// proportionnels à normalizedSpeed [0..1].
    /// </summary>
    private void ApplyShake(float normalizedSpeed)
    {
        if (shakeTarget == null) return;

        _shakeTime += Time.unscaledDeltaTime * shakeFrequency;

        float offsetX = Mathf.Sin(_shakeTime * Mathf.PI * 2f) * (normalizedSpeed * maxShakeAmplitude);
        float offsetY = Mathf.Cos(_shakeTime * Mathf.PI * 2.7f) * (normalizedSpeed * maxShakeAmplitude * 0.6f);
        shakeTarget.anchoredPosition = new Vector2(
            _shakeBasePosition.x + offsetX,
            _shakeBasePosition.y + offsetY
        );

        float rotationAngle = Mathf.Sin(_shakeTime * Mathf.PI * 2f * shakeRotationFrequency / shakeFrequency)
                              * (normalizedSpeed * maxShakeRotation);
        shakeTarget.localRotation = Quaternion.Euler(0f, 0f, rotationAngle);
    }

    /// <summary>Remet le shakeTarget à sa position et rotation de base.</summary>
    private void ResetShake()
    {
        if (shakeTarget == null) return;
        _shakeTime = 0f;
        shakeTarget.anchoredPosition = _shakeBasePosition;
        shakeTarget.localRotation = Quaternion.identity;
    }

    // ── Lumières ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Avance le clignotement des lumières en séquence.
    /// normalizedSpeed [0..1] pilote la fréquence entre minLightFrequency et maxLightFrequency.
    /// </summary>
    private void StepLights(float normalizedSpeed)
    {
        if (spinLights == null || spinLights.Count == 0) return;

        float frequency = Mathf.Lerp(minLightFrequency, maxLightFrequency, normalizedSpeed);
        float interval = 1f / frequency;

        _lightTimer += Time.unscaledDeltaTime;
        if (_lightTimer < interval) return;

        _lightTimer -= interval;

        // Éteint la lumière courante, allume la suivante
        if (spinLights[_currentLightIndex] != null)
            spinLights[_currentLightIndex].enabled = false;

        _currentLightIndex = (_currentLightIndex + 1) % spinLights.Count;

        if (spinLights[_currentLightIndex] != null)
            spinLights[_currentLightIndex].enabled = true;
    }

    /// <summary>Éteint toutes les lumières et réinitialise l'index.</summary>
    private void TurnOffAllLights()
    {
        if (spinLights == null) return;
        foreach (Light2D light in spinLights)
        {
            if (light != null)
                light.enabled = false;
        }
        _currentLightIndex = 0;
        _lightTimer = 0f;
    }

    // ── Pulse du gagnant ──────────────────────────────────────────────────────

    /// <summary>Lance la pulse infinie sur le slot gagnant.</summary>
    private void StartPulse(Transform slotTransform)
    {
        StopPulse();
        _pulseCoroutine = StartCoroutine(PulseCoroutine(slotTransform));
    }

    /// <summary>Arrête la pulse.</summary>
    private void StopPulse()
    {
        if (_pulseCoroutine == null) return;
        StopCoroutine(_pulseCoroutine);
        _pulseCoroutine = null;
    }

    /// <summary>Coroutine de pulse infinie : grossit et dégrossit en boucle (smoothstep).</summary>
    private IEnumerator PulseCoroutine(Transform slotTransform)
    {
        Vector3 baseScale = slotTransform.localScale;

        while (true)
        {
            float elapsed = 0f;
            while (elapsed < pulseHalfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / pulseHalfDuration);
                float s = t * t * (3f - 2f * t);
                slotTransform.localScale = baseScale * Mathf.Lerp(pulseMinScale, pulseMaxScale, s);
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < pulseHalfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / pulseHalfDuration);
                float s = t * t * (3f - 2f * t);
                slotTransform.localScale = baseScale * Mathf.Lerp(pulseMaxScale, pulseMinScale, s);
                yield return null;
            }
        }
    }

    // ── Promotion / démot hors du Mask ────────────────────────────────────────

    /// <summary>
    /// Reparente le slot gagnant dans overlayContainer (hors du Mask) en conservant
    /// sa position mondiale, pour qu'il s'affiche par-dessus tous les éléments.
    /// </summary>
    private void PromoteWinnerSlot(RouletteSlot slot)
    {
        if (overlayContainer == null) return;

        _promotedSlot = slot;
        RectTransform rt = slot.GetComponent<RectTransform>();
        Vector3 worldPos = rt.position;

        slot.transform.SetParent(overlayContainer, worldPositionStays: true);
        rt.position = worldPos;
        slot.transform.SetAsLastSibling();
    }

    /// <summary>Remet le slot promu dans itemsContainer avant le prochain BuildSlots.</summary>
    private void DemoteWinnerSlot()
    {
        if (_promotedSlot == null) return;
        _promotedSlot.transform.SetParent(itemsContainer, worldPositionStays: false);
        _promotedSlot = null;
    }

    // ── Défilement ────────────────────────────────────────────────────────────

    /// <summary>Déplace tous les slots de deltaScroll pixels vers le bas avec recyclage.</summary>
    private void ScrollSlots(float deltaScroll)
    {
        float bottomBound = _maskHeight + _stepHeight * 0.5f;

        for (int i = 0; i < _slots.Count; i++)
        {
            _slotPositions[i] += deltaScroll;

            if (_slotPositions[i] > bottomBound)
            {
                int topIdx = GetTopmostSlotIndex(excludeIdx: i);
                int topEmpIdx = _slotEmployeeIndices[topIdx];
                int newEmpIdx = (topEmpIdx - 1 + _employees.Count) % _employees.Count;

                _slotPositions[i] = _slotPositions[topIdx] - _stepHeight;
                _slotEmployeeIndices[i] = newEmpIdx;
                _slots[i].Setup(_employees[newEmpIdx]);
                _slots[i].SetHighlight(false);
            }

            _slotRects[i].anchoredPosition = new Vector2(0f, -_slotPositions[i]);
        }
    }

    // ── Animation principale ──────────────────────────────────────────────────

    private IEnumerator SpinCoroutine()
    {
        _isSpinning = true;
        _hasSpunToday = true;
        spinButton.interactable = false;
        _shakeTime = 0f;

        TurnOffAllLights();

        // Phase 1 — Son rapide en boucle, synchronisé avec le début du défilement
        

        float elapsed = 0f;
        while (elapsed < constantDuration)
        {
            float dt = Time.unscaledDeltaTime;
            elapsed += dt;
            ScrollSlots(maxSpeed * dt);
            ApplyShake(1f);
            StepLights(1f);
            yield return null;
        }

        // Phase 2 — Transition son : Spin s'arrête, SpinSlow démarre en même temps
        // que la décélération visuelle commence
       // StopSpinLoop();
        //PlaySpinLoop(AudioType.SpinSlow);

        float centerY = GetWinnerTargetY();
        int topIdx = GetTopmostSlotIndex();
        int bottomIdx = GetBottommostSlotIndex();
        float winnerStartY = _slotPositions[topIdx] - _stepHeight;

        _slotPositions[bottomIdx] = winnerStartY;
        _slotEmployeeIndices[bottomIdx] = _selectedIndex;
        _slots[bottomIdx].Setup(_employees[_selectedIndex]);
        _slots[bottomIdx].SetHighlight(false);
        _slotRects[bottomIdx].anchoredPosition = new Vector2(0f, -winnerStartY);

        float totalDecelerationDist = centerY - winnerStartY;
        float adaptedDuration = Mathf.Max(decelerationDuration, 3f * totalDecelerationDist / maxSpeed);

        float winnerTrackedY = winnerStartY;
        float scrolledSoFar = 0f;
        elapsed = 0f;

        while (elapsed < adaptedDuration)
        {
            float dt = Time.unscaledDeltaTime;
            elapsed += dt;
            float t = Mathf.Clamp01(elapsed / adaptedDuration);
            float tInv = 1f - t;
            float progress = 1f - (tInv * tInv * tInv);
            float delta = progress * totalDecelerationDist - scrolledSoFar;
            scrolledSoFar = progress * totalDecelerationDist;

            float normalizedSpeed = tInv * tInv;
            ApplyShake(normalizedSpeed);
            StepLights(normalizedSpeed);
            winnerTrackedY += delta;
            ScrollSlots(delta);
            yield return null;
        }

        // Snap pixel-perfect — la roulette est visuellement arrêtée
        float snapDelta = centerY - winnerTrackedY;
        for (int i = 0; i < _slots.Count; i++)
        {
            _slotPositions[i] += snapDelta;
            _slotRects[i].anchoredPosition = new Vector2(0f, -_slotPositions[i]);
        }

        ResetShake();
        TurnOffAllLights();

        int finalWinnerSlot = 0;
        float closestDist = float.MaxValue;
        for (int i = 0; i < _slotPositions.Count; i++)
        {
            float dist = Mathf.Abs(_slotPositions[i] - centerY);
            if (dist < closestDist) { closestDist = dist; finalWinnerSlot = i; }
        }

        _slots[finalWinnerSlot].Setup(_employees[_selectedIndex]);
        _slots[finalWinnerSlot].SetHighlight(true, highlightColor);

        RegisterWinnerAsTaken(_selectedIndex);

        // Phase 3 — Arrêt total : SpinSlow coupe, SpinEnd joue en one-shot
       // StopSpinLoop();
        audioEventDispatcher?.PlayAudio(AudioType.SpinEnd);

        PromoteWinnerSlot(_slots[finalWinnerSlot]);
        StartPulse(_slots[finalWinnerSlot].transform);

        yield return new WaitForSeconds(revealDelay);

        if (resultLabel != null)
            resultLabel.text = _employees[_selectedIndex].EmployeName;

        _isSpinning = false;

        OnEmployeSelected?.Invoke(_employees[_selectedIndex]);
        EmployeSelected?.Invoke();
        Debug.Log($"[RouletteWheel] Sélectionné : {_employees[_selectedIndex].EmployeName}");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>Index du slot le plus haut visuellement (position Y minimale).</summary>
    private int GetTopmostSlotIndex(int excludeIdx = -1)
    {
        int idx = -1;
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
        int idx = 0;
        float max = float.MinValue;
        for (int i = 0; i < _slotPositions.Count; i++)
        {
            if (_slotPositions[i] > max) { max = _slotPositions[i]; idx = i; }
        }
        return idx;
    }

    /// <summary>
    /// Retourne la position Y cible dans l'espace local du Mask (positif vers le bas depuis le haut)
    /// correspondant au winnerTargetMarker. Fallback sur le centre du masque si non assigné.
    /// </summary>
    private float GetWinnerTargetY()
    {
        if (winnerTargetMarker == null)
            return _maskHeight * 0.5f;

        Vector3 localPos = maskRect.InverseTransformPoint(winnerTargetMarker.position);
        return _maskHeight * 0.5f - localPos.y;
    }

    /// <summary>Démarre le son de boucle spin sur la source dédiée.</summary>
    private void PlaySpinLoop(AudioType audioType)
    {
        if (audioEventDispatcher == null || spinLoopSource == null) return;
        AudioClip clip = audioEventDispatcher.GetClip(audioType);
        if (clip == null) return;
        spinLoopSource.clip = clip;
        spinLoopSource.loop = true;
        spinLoopSource.Play();
    }

    /// <summary>Stoppe la source de boucle spin.</summary>
    private void StopSpinLoop()
    {
        if (spinLoopSource == null) return;
        spinLoopSource.Stop();
        spinLoopSource.clip = null;
    }
}
