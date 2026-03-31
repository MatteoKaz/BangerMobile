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
    [SerializeField] GameObject shop;
    [SerializeField] UIDay dayScript;
    [SerializeField] GameObject ScoreScene;

    public float animDuration = 1.0f;

    [Header("DifficultyUI")]
    public float waitTimeBeforeCloseDifficulty = 1f;
    [SerializeField] GameObject DifficultyChoice;


    public event Action DifficultyChosenAnim;
    public event Action DifficultyShownAnim;
    public event Action ScoreAnim;
    public event Action LaunchDayAnim;
    public event Action dayResetOpacity;
    public event Action ScoreReset;

    [SerializeField] public float yendPose = 2890f;
    [SerializeField] public float ybasePose = 960f;
    [SerializeField] GameObject ScoreUi;
    [SerializeField] AnimationCurve curveAnim;
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
        Difficulty.SetActive(true);
        dayResetOpacity?.Invoke();
        LaunchDayAnim?.Invoke();
        StartCoroutine(waitToShowDifficulty());
        

    }
    public IEnumerator waitToShowDifficulty()
    {

        yield return new WaitForSeconds(0.5f);
        
        DifficultyShownAnim?.Invoke();
    }
    public void DisableDay()
    {
        Day.SetActive(false);
    }

    public void EnableScore()
    {
        Score.SetActive(true);
        DifficultyChoice.SetActive(true);
        Day.SetActive(true);
        StartCoroutine(AnimScore());
        ScoreAnim?.Invoke();
        dayResetOpacity?.Invoke();
    }

    public IEnumerator AnimScore()
    {
        RectTransform rect = ScoreScene.GetComponent<RectTransform>();

        Vector2 startpos = new Vector2(rect.anchoredPosition.x, ybasePose);
        Vector2 targetPos = new Vector2(rect.anchoredPosition.x, yendPose);
        float t = 0;
        while (t < animDuration)
        {
            t += Time.deltaTime;
            float normalized = t / animDuration;

            float curve = curveAnim.Evaluate(normalized);
            rect.anchoredPosition = Vector2.Lerp(targetPos, startpos, curve);

            yield return null;
        }
    }
    public void DisableScore()
    {
        ScoreReset?.Invoke();
        Score.SetActive(false);
    }

    public void EnableShop()
    {
        shop.SetActive(true);
    }

    
}
