using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostManager : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private PoleManager poleManager;
    [SerializeField] private DayManager dayManager;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private QuotatManager quotatManager;
    [SerializeField] private PaperSpawner paperSpawner;

    [Header("Paramètres de boost")]
    [SerializeField] private float minIntervalBetweenBoosts = 15f;
    [SerializeField] private float maxIntervalBetweenBoosts = 40f;
    [SerializeField] private float boostDuration = 10f;
    [SerializeField] private float boostSpeedBonus = 0.5f;

    private Coroutine _boostLoopCoroutine;
    private bool _dayRunning;
    private bool _difficultyChosen;
    private bool _allPapersSpawned;

    private readonly HashSet<Pole> _boostedPoles = new HashSet<Pole>();
    private readonly List<Coroutine> _activeBoostCoroutines = new List<Coroutine>();

    private static readonly float[] BoostMultipliers = { 1.5f, 2f, 2.5f, 3f };

    /// <summary>Événement déclenché quand un boost commence sur un pôle.</summary>
    public event Action<Pole> BoostStarted;
    /// <summary>Événement déclenché quand un boost se termine sur un pôle.</summary>
    public event Action<Pole> BoostEnded;

    private void OnEnable()
    {
        dayManager.DayBegin += OnDayBegin;
        dayManager.DayEnd += OnDayEnd;
        timeManager.TimerEnded += OnDayEnd;
        quotatManager.QuotatIsSet += OnDifficultyChosen;
        paperSpawner.AllPapersSpawned += OnAllPapersSpawned;
    }

    private void OnDisable()
    {
        dayManager.DayBegin -= OnDayBegin;
        dayManager.DayEnd -= OnDayEnd;
        timeManager.TimerEnded -= OnDayEnd;
        quotatManager.QuotatIsSet -= OnDifficultyChosen;
        paperSpawner.AllPapersSpawned -= OnAllPapersSpawned;
    }

    private void OnDayBegin()
    {
        Debug.Log("[BoostManager] OnDayBegin — en attente de la difficulté");
        _dayRunning = true;
        _difficultyChosen = false;
        _allPapersSpawned = false;
        _boostedPoles.Clear();
        _activeBoostCoroutines.Clear();
    }

    private void OnDifficultyChosen()
    {
        if (!_dayRunning) return;

        Debug.Log("[BoostManager] Difficulté choisie — démarrage du BoostLoop");
        _difficultyChosen = true;

        if (_boostLoopCoroutine != null)
        {
            StopCoroutine(_boostLoopCoroutine);
            _boostLoopCoroutine = null;
        }

        _boostLoopCoroutine = StartCoroutine(BoostLoop());
    }

    private void OnAllPapersSpawned()
    {
        Debug.Log("[BoostManager] Dernier papier spawné — arrêt immédiat de tous les boosts");
        _allPapersSpawned = true;
        StopAllBoosts();
    }

    private void OnDayEnd()
    {
        Debug.Log("[BoostManager] OnDayEnd — arrêt du loop");
        _dayRunning = false;
        StopAllBoosts();
    }

    /// <summary>Arrête le BoostLoop et force l'arrêt immédiat de tous les boosts actifs.</summary>
    private void StopAllBoosts()
    {
        if (_boostLoopCoroutine != null)
        {
            StopCoroutine(_boostLoopCoroutine);
            _boostLoopCoroutine = null;
        }

        foreach (Coroutine coroutine in _activeBoostCoroutines)
        {
            if (coroutine != null)
                StopCoroutine(coroutine);
        }

        foreach (Pole pole in _boostedPoles)
        {
            pole.CurrentBoostMultiplier = 1f;
            BoostEnded?.Invoke(pole);
        }

        _boostedPoles.Clear();
        _activeBoostCoroutines.Clear();
    }

    private IEnumerator BoostLoop()
    {
        Debug.Log($"[BoostManager] BoostLoop lancé — pôles disponibles : {poleManager?.poles?.Length}");

        while (_dayRunning && !_allPapersSpawned)
        {
            float delay = UnityEngine.Random.Range(minIntervalBetweenBoosts, maxIntervalBetweenBoosts);
            Debug.Log($"[BoostManager] Prochain boost dans {delay:F1}s");
            yield return new WaitForSeconds(delay);

            if (!_dayRunning || _allPapersSpawned) yield break;

            Pole[] poles = poleManager.poles;
            if (poles == null || poles.Length == 0)
            {
                Debug.LogWarning("[BoostManager] Aucun pôle trouvé dans poleManager.poles !");
                continue;
            }

            List<Pole> availablePoles = new List<Pole>();
            foreach (Pole pole in poles)
            {
                if (!_boostedPoles.Contains(pole))
                    availablePoles.Add(pole);
            }

            if (availablePoles.Count == 0)
            {
                Debug.Log("[BoostManager] Tous les pôles sont déjà boostés, skip.");
                continue;
            }

            Pole target = availablePoles[UnityEngine.Random.Range(0, availablePoles.Count)];
            Coroutine boostCoroutine = StartCoroutine(ApplyBoost(target));
            _activeBoostCoroutines.Add(boostCoroutine);
        }
    }

    private IEnumerator ApplyBoost(Pole pole)
    {
    _boostedPoles.Add(pole);
    float previousBoostSpeed = pole.BoostEmployeSpeed;
    float chosenMultiplier = BoostMultipliers[UnityEngine.Random.Range(0, BoostMultipliers.Length)];
    
    pole.CurrentBoostMultiplier = chosenMultiplier;
    pole.BoostEmployeSpeed = previousBoostSpeed + boostSpeedBonus;

    BoostStarted?.Invoke(pole);
    yield return new WaitForSeconds(boostDuration);

    if (_boostedPoles.Contains(pole))
    {
        pole.CurrentBoostMultiplier = 1f;
        pole.BoostEmployeSpeed = previousBoostSpeed;
        _boostedPoles.Remove(pole);
        BoostEnded?.Invoke(pole);
    }
    }


    /* old code
    private IEnumerator ApplyBoost(Pole pole)
    {
        _boostedPoles.Add(pole);

        float previousBonusRevenus = pole.BonusRevenus;
        float previousBoostSpeed = pole.BoostEmployeSpeed;

        float chosenMultiplier = BoostMultipliers[UnityEngine.Random.Range(0, BoostMultipliers.Length)];
        pole.CurrentBoostMultiplier = chosenMultiplier;
        pole.BonusRevenus = previousBonusRevenus * chosenMultiplier;
        pole.BoostEmployeSpeed = previousBoostSpeed + boostSpeedBonus;

        Debug.Log($"[BoostManager] Boost ON — pôle : {pole.name} | Multiplicateur : x{chosenMultiplier} | BonusRevenus : {previousBonusRevenus} → {pole.BonusRevenus} | BoostSpeed : {previousBoostSpeed} → {pole.BoostEmployeSpeed}");

        BoostStarted?.Invoke(pole);

        yield return new WaitForSeconds(boostDuration);

        // Restaure uniquement si StopAllBoosts n'a pas déjà géré ce pôle
        if (_boostedPoles.Contains(pole))
        {
            pole.BonusRevenus = previousBonusRevenus;
            pole.BoostEmployeSpeed = previousBoostSpeed;
            pole.CurrentBoostMultiplier = 1f;
            _boostedPoles.Remove(pole);

            Debug.Log($"[BoostManager] Boost OFF — pôle : {pole.name} | Valeurs restaurées.");

            BoostEnded?.Invoke(pole);
        }
    }*/
}
