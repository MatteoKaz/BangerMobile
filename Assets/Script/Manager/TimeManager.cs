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
    [SerializeField] PaperSpawner spawnerpaper;

    public void OnEnable()
    {
        //QuotatManager.QuotatChosen += LaunchTimer;
        spawnerpaper.AllPapersSpawned += OnAllPapersSpawned;
    }

    void OnDisable()
    {
        //QuotatManager.QuotatChosen -= LaunchTimer;
        spawnerpaper.AllPapersSpawned -= OnAllPapersSpawned;
    }

    private void OnAllPapersSpawned()
    {
        TimerEnded?.Invoke();
    }

 
    /*public void LaunchTimer()
    {
        if (currentTimer != null)
            StopCoroutine(currentTimer);
        currentTimer = StartCoroutine(DayTimer());
    }
    /*public IEnumerator DayTimer()
    {
        yield return new WaitForSeconds(2f);
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

    }*/

}
