using System;
using System.Collections;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;

public class QuotatManager : MonoBehaviour
{
    public int WeekQuotat = 500;
    public int BaseQuotat = 250;
    public float exposant = 1.5f;
    public int DayQuotat ;
    [SerializeField] DayManager dayManager;
    public event Action QuotatChosen;
    public int quotatEasy;
    public int quotatMid;
    public int quotatHard;
    [SerializeField] PaperSpawner paperSpawner;
    [SerializeField] ScoreManager scoreManager;

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
        WeekQuotat = (int)Math.Round(BaseQuotat * Math.Pow(dayManager.currentWeek, exposant - 1));
    }

    //Temporaire
    public void QuotatPerDay()
    {
        //TemporairePourTest
        DayQuotat = WeekQuotat;
        quotatEasy = Mathf.RoundToInt(DayQuotat * 0.75f);
        quotatMid = Mathf.RoundToInt(DayQuotat * 1f);
        quotatHard = Mathf.RoundToInt(DayQuotat * 1.25f);
        StartCoroutine(WaitForActive());
        Debug.Log($"QuotatPerDay {DayQuotat}");
        
    }

    public IEnumerator WaitForActive()
    {
        yield return new WaitForSeconds(0.1f);
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
        QuotatIsSet?.Invoke();
        ChosenQuotat(DayQuotat);
    }
  
    public void ChosenQuotat(int quotatChosen)
    {
        DayQuotat = quotatChosen;
        scoreManager.quotatOfTheDay = quotatChosen;
        int valueToSpawn = DayQuotat / 10;
        paperSpawner.totalPapers = Mathf.RoundToInt(multToBalance * valueToSpawn) ;
        StartCoroutine(StartWave());
        
        Debug.Log($"Quotat manager {DayQuotat}");
    }
    public IEnumerator StartWave()
    {
        yield return new WaitForSeconds(waitTimeBeforLaunch);
        QuotatChosen?.Invoke();
    }

  

}
