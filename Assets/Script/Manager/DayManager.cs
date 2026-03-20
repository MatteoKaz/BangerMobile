using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class DayManager : MonoBehaviour
{
    private int currentDay = 0;
    public string DayName;
    public int currentWeek = 1;

    public float transitionDuration = 1f;

    public event Action DayBegin;
    public event Action DayEnd;
    public event Action FirstDayInitialization;
    public event Action DayTransition;

    [SerializeField] private TimeManager timeManager; 

    public void OnEnable()
    {
        timeManager.TimerEnded += DayOver;
    }
    void Start()
    {
        LaunchNewDay();
        IsFirstGame();
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
    

    public void DayOver()
    {
        DayEnd?.Invoke();
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
            FirstDayInitialization?.Invoke();
            Debug.Log("Initialisation premier jour");
        }
       

    }



    public IEnumerator EndDayFade()
    {
        yield return new WaitForSeconds(transitionDuration);
        DayTransition?.Invoke();
    }
}
