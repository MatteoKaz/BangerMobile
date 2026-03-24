using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class DayManager : MonoBehaviour
{
    public int currentDay = 0;
    public string DayName;
    public int currentWeek = 1;
    
    [HideInInspector] public bool skipFirstLaunch = false;

    public float transitionDuration = 1f;

    public event Action ResetValueBeforeNextDay;
    public event Action DayBegin;
    public event Action DayEnd;
    public event Action FirstDayInitialization;
    public event Action DayTransition;
    public event Action RankingDay;
    public event Action NewWeekReset;
    [SerializeField] private TimeManager timeManager;
    [SerializeField] RankingManager rankingManager;
    public void OnEnable()
    {
        timeManager.TimerEnded += DayOver;
    }
    void Start()
    {
      
       if (!skipFirstLaunch)
        {
            LaunchNewDay();
            IsFirstGame();
        }
    }
    public void LaunchNewDay()
    {
        
        if (currentDay <5)
        {
            currentDay += 1;
            DayNameChange();
        }
        else
        {
            currentWeek += 1;
            currentDay = 1;
            DayNameChange();
        }
        DayBegin?.Invoke();


    }
    public void RestoreDay(int day, int week)
    {
        currentDay  = day;
        currentWeek = week;
        DayNameChange();
        DayBegin?.Invoke();
    }
    public void NewDaySetUp()
    {
        if (currentDay < 5)
        {

            StartCoroutine(NextDaySetup());
        }
        else
        {
            ResetValueBeforeNextDay?.Invoke();
            rankingManager.rankingFolder.SetActive(true);
            RankingDay?.Invoke();
        }

    }
    public void NewWeek()
    {
        rankingManager.rankingFolder.SetActive(false);
        NewWeekReset?.Invoke();
        StartCoroutine(NextDaySetup());
    }
    public IEnumerator NextDaySetup()
    {
        
        ResetValueBeforeNextDay?.Invoke();
        yield return new WaitForSeconds(1f);
        LaunchNewDay();
    }

    public void DayOver()
    {
        DayEnd?.Invoke();
        StartCoroutine(EndDayFade());
    }

    public void DayNameChange()
    {
        switch(currentDay)
        {
            case 1:
                DayName = "Lundi";
                break;
            case 2:
                DayName = "Mardi";
                break;
            case 3:
                DayName = "Mercredi";
                break;
            case 4:
                DayName = "Jeudi";
                break;
            case 5:
                DayName = "Vendredi";
                break;

                 
        }
    }

    public void IsFirstGame()
    {
        if ( currentDay == 1 && currentWeek == 1)
        {
            StartCoroutine(LaunchFirstDayInit());
            Debug.Log("Initialisation premier jour");
        }
       

    }

    public IEnumerator LaunchFirstDayInit()
    {
        yield return new WaitForSeconds(0.1f);
        FirstDayInitialization?.Invoke();
    }

    public IEnumerator EndDayFade()
    {
        //Lancer IciFadeIn
        yield return new WaitForSeconds(transitionDuration);
        DayTransition?.Invoke();
    }
}
