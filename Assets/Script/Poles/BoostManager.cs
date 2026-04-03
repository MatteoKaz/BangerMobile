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

    [Header("Paramètres de boost")]
    [SerializeField] private float minIntervalBetweenBoosts = 15f;
    [SerializeField] private float maxIntervalBetweenBoosts = 40f;
    [SerializeField] private float boostDuration = 10f;
    [SerializeField] private float boostSpeedBonus = 0.5f;

    private Coroutine _boostLoopCoroutine;
    private bool _dayRunning;

    private readonly HashSet<Pole> _boostedPoles = new HashSet<Pole>();

    /// <summary>Événement déclenché quand un boost commence sur un pôle.</summary>
    public event Action<Pole> BoostStarted;
    /// <summary>Événement déclenché quand un boost se termine sur un pôle.</summary>
    public event Action<Pole> BoostEnded;

    private void OnEnable()
    {
        dayManager.DayBegin    += OnDayBegin;
        dayManager.DayEnd      += OnDayEnd;
        timeManager.TimerEnded += OnDayEnd;
    }

    private void OnDisable()
    {
        dayManager.DayBegin    -= OnDayBegin;
        dayManager.DayEnd      -= OnDayEnd;
        timeManager.TimerEnded -= OnDayEnd;
    }

    private void OnDayBegin()
    {
        Debug.Log("[BoostManager] OnDayBegin — démarrage du loop");
        _dayRunning = true;
        _boostedPoles.Clear();
        _boostLoopCoroutine = StartCoroutine(BoostLoop());
    }

    private void OnDayEnd()
    {
        Debug.Log("[BoostManager] OnDayEnd — arrêt du loop");
        _dayRunning = false;
        if (_boostLoopCoroutine != null)
        {
            StopCoroutine(_boostLoopCoroutine);
            _boostLoopCoroutine = null;
        }
    }

    private IEnumerator BoostLoop()
    {
        Debug.Log($"[BoostManager] BoostLoop lancé — pôles disponibles : {poleManager?.poles?.Length}");

        while (_dayRunning)
        {
            float delay = UnityEngine.Random.Range(minIntervalBetweenBoosts, maxIntervalBetweenBoosts);
            Debug.Log($"[BoostManager] Prochain boost dans {delay:F1}s");
            yield return new WaitForSeconds(delay);

            if (!_dayRunning) yield break;

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
            StartCoroutine(ApplyBoost(target));
        }
    }

    private IEnumerator ApplyBoost(Pole pole)
    {
        _boostedPoles.Add(pole);

        float previousBonusRevenus = pole.BonusRevenus;
        float previousBoostSpeed   = pole.BoostEmployeSpeed;

        pole.BonusRevenus      = previousBonusRevenus * 2f;
        pole.BoostEmployeSpeed = previousBoostSpeed + boostSpeedBonus;

        Debug.Log($"[BoostManager] Boost ON — pôle : {pole.name} | BonusRevenus : {previousBonusRevenus} → {pole.BonusRevenus} | BoostSpeed : {previousBoostSpeed} → {pole.BoostEmployeSpeed}");

        BoostStarted?.Invoke(pole);

        yield return new WaitForSeconds(boostDuration);

        pole.BonusRevenus      = previousBonusRevenus;
        pole.BoostEmployeSpeed = previousBoostSpeed;

        _boostedPoles.Remove(pole);

        Debug.Log($"[BoostManager] Boost OFF — pôle : {pole.name} | Valeurs restaurées.");

        BoostEnded?.Invoke(pole);
    }
}
