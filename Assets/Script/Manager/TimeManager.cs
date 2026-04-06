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
    [SerializeField] PoleManager poleManager;
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

            // Vérifie si plus rien dans la scène
            bool noPapers = FindObjectsByType<PaperMove>(FindObjectsSortMode.None).Length == 0;
            bool noTasks = true;
            foreach (Pole pole in poleManager.poles)
            {
                if (pole.totalPaper > 0) { noTasks = false; break; }
            }

            if (noPapers && noTasks)
                break; 

            yield return null;
        }
        yield return new WaitForSeconds(0.5f);
        currentTimer = null;
        TimerEnded?.Invoke();
        yield return new WaitForSeconds(0.5f);
        timer.SetActive(false);
    }

}
