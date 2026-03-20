using System;
using System.Collections;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    [SerializeField] public int DayDuration = 60;
    public int DayDurationToShow = 180;
    public event Action TimerEnded;
    [SerializeField] QuotatManager QuotatManager;
    Coroutine currentTimer;


    public void OnEnable()
    {
        QuotatManager.QuotatChosen += LaunchTimer;
    }

    public void LaunchTimer()
    {
        if (currentTimer != null)
            StopCoroutine(currentTimer);
        currentTimer = StartCoroutine(DayTimer());
    }
    public IEnumerator DayTimer()
    {
        DayDurationToShow = DayDuration;
        float T = 0;
        while (T < DayDuration)
        {
            T += Time.deltaTime;
            DayDurationToShow = Mathf.CeilToInt(DayDuration - T);
            
            yield return null;  
        }
        currentTimer = null;
        TimerEnded?.Invoke();

    }
}
