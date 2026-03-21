using System;
using System.Collections;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    [SerializeField] DayManager dayManager;
    [SerializeField] QuotatManager quotatManager;
    [ SerializeField] ScoreManager scoreManager;
    

    [SerializeField] GameObject Day;
    [SerializeField] GameObject Score;
    [SerializeField] GameObject Difficulty;

    [SerializeField] UIDay dayScript;

    [Header("DifficultyUI")]
    public float waitTimeBeforeCloseDifficulty = 1f;
    [SerializeField] GameObject DifficultyChoice;


    public event Action DifficultyChosenAnim;
    public event Action DifficultyShownAnim;
    public event Action ScoreAnim;
    public event Action LaunchDayAnim;

    private void OnEnable()
    {
        quotatManager.QuotatIsSet += DisableDifficultyUI;
        dayManager.DayBegin += EnableDay;
        dayScript.EndShowing += DisableDay;
        scoreManager.LaunchScoreAnim += EnableScore;
        dayManager.DayBegin += DisableScore;
    }
    private void OnDisable()
    {
        quotatManager.QuotatIsSet -= DisableDifficultyUI;
        dayManager.DayBegin -= EnableDay;
        dayScript.EndShowing -= DisableDay;
        scoreManager.LaunchScoreAnim -= EnableScore;
        dayManager.DayBegin -= DisableScore;
    }


    public void DisableDifficultyUI()
    {
        
        StartCoroutine(CloseDifficultyUI());
    }

    public IEnumerator CloseDifficultyUI()
    {
        DifficultyChosenAnim?.Invoke();
        yield return new WaitForSeconds(waitTimeBeforeCloseDifficulty);
        DifficultyChoice.SetActive(false);
    }


    public void EnableDay()
    {
       
        Day.SetActive(true);
        LaunchDayAnim?.Invoke();
        Difficulty.SetActive(true);
        DifficultyShownAnim?.Invoke();

    }

    public void DisableDay()
    {
        Day.SetActive(false);
    }

    public void EnableScore()
    {
        Score.SetActive(true);
        ScoreAnim?.Invoke();
    }
    public void DisableScore()
    {
        Score.SetActive(false);
    }
}
