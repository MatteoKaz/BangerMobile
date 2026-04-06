using System;
using System.Collections;

using UnityEngine;

public class QuotatManager : MonoBehaviour
{
     private int WeekQuotat = 250;
     private int BaseQuotat = 250;
    private float exposant = 3f;
    public int DayQuotat ;
    [SerializeField] DayManager dayManager;
    public event Action QuotatChosen;
    public int quotatEasy;
    public int quotatMid;
    public int quotatHard;
    [SerializeField] PaperSpawner paperSpawner;
    [SerializeField] ScoreManager scoreManager;
    public int currentDifficulty { get; private set; }
    [Header("BalanceSpawnValue")]
    public float multToBalance = 2f;


    [Header("TimeBeforeLaunchSpawner")]
    public float waitTimeBeforLaunch = 1f;


    public event Action QuotatIsSet;
    public event Action CalculQuotat;
    
    public void OnEnable()
    {
        dayManager.DayBegin += QuotatPerDay;
        dayManager.NewWeekReset += QuotatCroissance;
    }
    void OnDisable()
    {
        dayManager.DayBegin -= QuotatPerDay;
        dayManager.NewWeekReset -= QuotatCroissance;
    }

    public void QuotatCroissance()
    {
        Debug.LogWarning(dayManager.currentWeek);
        //WeekQuotat = (int)Math.Round(BaseQuotat * Math.Pow(dayManager.currentWeek, exposant - 1));
    }

   

     public void QuotatPerDay()
    {
        // Progression semaine linéaire
        WeekQuotat = Mathf.RoundToInt(BaseQuotat * (1f + (dayManager.currentWeek - 1f) * 0.15f));

        // Variation par jour
        float dayMult = 1f;
        switch (dayManager.currentDay)
        {
            case 1: dayMult = 0.75f; break;
            case 2: dayMult = 1.00f; break;
            case 3: dayMult = 1.20f; break;
            case 4: dayMult = 0.80f; break;
            case 5: dayMult = 1.25f; break;
        }
        DayQuotat = Mathf.RoundToInt(WeekQuotat * dayMult);

        // Quotas difficulté avec convergence par semaine
        quotatEasy = Mathf.RoundToInt(DayQuotat * GetDifficultyMultiplier(0, dayManager.currentWeek));
        quotatMid = Mathf.RoundToInt(DayQuotat * GetDifficultyMultiplier(1, dayManager.currentWeek));
        quotatHard = Mathf.RoundToInt(DayQuotat * GetDifficultyMultiplier(2, dayManager.currentWeek));

        StartCoroutine(WaitForActive());
        Debug.Log($"QuotatPerDay — Day:{dayManager.currentDay} DayQuotat:{DayQuotat} | Easy:{quotatEasy} Mid:{quotatMid} Hard:{quotatHard}");
    }

    public IEnumerator WaitForActive()
    {
        yield return new WaitForEndOfFrame();
        CalculQuotat?.Invoke();
    }
    public void SelectQuotat(int difficultySelect)
    {
        switch (difficultySelect)
        {
            case 0:
                DayQuotat = quotatEasy;
                break;
            case 1:
                DayQuotat = quotatMid;
                break;
            case 2:
                DayQuotat = quotatHard;
                break;
        }

        scoreManager.SetDifficulty(difficultySelect);
        currentDifficulty = difficultySelect;
        QuotatIsSet?.Invoke();
        ChosenQuotat(DayQuotat);
    }

    private float GetDifficultyMultiplier(int difficulty, int week)
    {
        // Valeurs de départ (semaine 1)
        float startMult;
        // Valeurs cibles (semaine finale ~10)
        float targetMult;

        switch (difficulty)
        {
            case 0: // Easy
                startMult = 1.0f;
                targetMult = 2f;
                break;
            case 1: // Mid
                startMult = 1.3f;
                targetMult = 3f;
                break;
            case 2: // Hard
                startMult = 1.5f;
                targetMult = 4f;
                break;
            default:
                startMult = 1.5f;
                targetMult = 1.0f;
                break;
        }

        // t = 0 en semaine 1, t = 1 en semaine maxWeek
        float maxWeek = 10f;
        float t = Mathf.Clamp01((week - 1f) / (maxWeek - 1f));

        return Mathf.Lerp(startMult, targetMult, t);
    }

    private float GetSpawnMultiplier(int difficulty, int week)
    {
        float t = Mathf.Clamp01((week - 1f) / 9f);
        switch (difficulty)
        {
            case 0: return Mathf.Lerp(2f, 0.9f, t); // Easy S12x, S10 1.5x
            case 1: return Mathf.Lerp(1.5f, 0.75f, t); // Mid  S11.5x, S10 1x
            case 2: return Mathf.Lerp(1.1f, 0.5f, t); // Hard S11.1x, S100.6x
            default: return 1f;
        }
    }
    public void ChosenQuotat(int quotatChosen)
    {
        DayQuotat = quotatChosen;
        scoreManager.quotatOfTheDay = quotatChosen;
        int valueToSpawn = DayQuotat / 10;
        float spawnMult = GetSpawnMultiplier(currentDifficulty, dayManager.currentWeek);
        paperSpawner.totalPapers = Mathf.RoundToInt(spawnMult * valueToSpawn) / 3;
        StartCoroutine(StartWave());
    }
    public IEnumerator StartWave()
    {
        yield return new WaitForSeconds(waitTimeBeforLaunch);
        QuotatChosen?.Invoke();
    }

  

}
