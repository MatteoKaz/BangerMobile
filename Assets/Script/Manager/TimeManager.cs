using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TimeManager : MonoBehaviour
{
    [SerializeField] public int DayDuration = 25;
    public int DayDurationToShow = 25;
    public event Action TimerEnded;
    [SerializeField] QuotatManager QuotatManager;
    Coroutine currentTimer;
    [SerializeField] PaperSpawner spawnerpaper;
    [SerializeField] Button button;
    [SerializeField] GameObject timer;
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
        //button.interactable = true;
        if (currentTimer != null)
            StopCoroutine(currentTimer);
        timer.SetActive(true);
        currentTimer = StartCoroutine(DayTimer());
    }
    public void EndDay()
    {
        //button.interactable = false;
        TimerEnded?.Invoke();
    }
 
    /*public void LaunchTimer()
    {
        if (currentTimer != null)
            StopCoroutine(currentTimer);
        currentTimer = StartCoroutine(DayTimer());
    }
    */
    public IEnumerator DayTimer()
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
        yield return new WaitForSeconds(0.5f);
        timer.SetActive(false);
    }

}
