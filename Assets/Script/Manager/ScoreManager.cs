using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int playerMoney = 0;
    public int playerQuotat = 0;
    public int quotatOfTheDay = 0;
    public int WeekMedianQuotat = 0;

    [Header("Références")]
    [SerializeField] DayManager dayManager;
    [SerializeField] PoleManager poleManager;

    [Header("Bonus par quota de pôle atteint (par difficulté)")]
    [Tooltip("Bonus appliqué par pôle ayant atteint son quota en difficulté Facile (ex: 0.05 = +5%).")]
    public float bonusPoleEasy = 0.05f;
    [Tooltip("Bonus appliqué par pôle ayant atteint son quota en difficulté Normale (ex: 0.10 = +10%).")]
    public float bonusPoleMid = 0.10f;
    [Tooltip("Bonus appliqué par pôle ayant atteint son quota en difficulté Difficile (ex: 0.20 = +20%).")]
    public float bonusPoleHard = 0.20f;

    /// <summary>Difficulté choisie par le joueur ce jour (0 = Easy, 1 = Mid, 2 = Hard).</summary>
    [HideInInspector] public int currentDifficulty = 1;

    /// <summary>Argent de base avant application des bonus de pôles.</summary>
    [HideInInspector] public int baseMoney;

    /// <summary>Résultat de chaque pôle pour l'animation UI.</summary>
    public struct PoleResult
    {
        public string poleName;
        public bool quotaReached;
        public float bonusPercent;
        public int advancement;
        public int quota;
    }

    [HideInInspector] public List<PoleResult> poleResults = new List<PoleResult>();

    public event Action LaunchScoreAnim;

    public void OnEnable()
    {
        dayManager.DayTransition += CalculateMoney;
        dayManager.DayBegin += ResetDay;
    }

    public void OnDisable()
    {
        dayManager.DayTransition -= CalculateMoney;
        dayManager.DayBegin -= ResetDay;
    }

    /// <summary>Appelé par QuotatManager.SelectQuotat pour stocker la difficulté choisie.</summary>
    public void SetDifficulty(int difficulty)
    {
        currentDifficulty = difficulty;
    }

    /// <summary>Retourne le bonus par pôle selon la difficulté actuelle.</summary>
    private float GetBonusForCurrentDifficulty()
    {
        return currentDifficulty switch
        {
            0 => bonusPoleEasy,
            2 => bonusPoleHard,
            _ => bonusPoleMid
        };
    }

    /// <summary>Calcule l'argent de base puis les bonus par pôle ayant atteint son quota.</summary>
    public void CalculateMoney()
    {
        int earned = Mathf.Max(0, playerQuotat - quotatOfTheDay);
        baseMoney = earned;

        poleResults.Clear();

        float bonusPercent = GetBonusForCurrentDifficulty();
        int totalBonus = 0;

        if (poleManager != null)
        {
            for (int i = 0; i < poleManager.poles.Length; i++)
            {
                Pole pole = poleManager.poles[i];
                bool reached = pole.localAdvencement >= pole.localQuotat;

                string name = !string.IsNullOrEmpty(pole.PoleName) ? pole.PoleName : $"Pôle {i + 1}";

                poleResults.Add(new PoleResult
                {
                    poleName     = name,
                    quotaReached = reached,
                    bonusPercent = reached ? bonusPercent : 0f,
                    advancement  = pole.localAdvencement,
                    quota        = pole.localQuotat
                });

                if (reached)
                    totalBonus += Mathf.RoundToInt(baseMoney * bonusPercent);
            }
        }

        playerMoney += baseMoney + totalBonus;
        playerMoney = Mathf.Max(0, playerMoney);

        StartCoroutine(AnimLauncher());
    }

    public IEnumerator AnimLauncher()
    {
        yield return new WaitForSeconds(1f);
        LaunchScoreAnim?.Invoke();
    }

    public void ResetDay()
    {
        playerQuotat = 0;
    }
}
