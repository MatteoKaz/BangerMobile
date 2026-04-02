using System;
using System.Collections;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    [SerializeField] DayManager dayManager;
    [SerializeField] QuotatManager quotatManager;
    [SerializeField] ScoreManager scoreManager;

    [SerializeField] GameObject Day;
    [SerializeField] GameObject Score;
    [SerializeField] GameObject Difficulty;
    [SerializeField] GameObject shop;
    [SerializeField] UIDay dayScript;
    [SerializeField] GameObject ScoreScene;
    [SerializeField] GameObject ShopScene;

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

    private bool _shopOpenedNotified = false;

    private void OnEnable()
    {
        quotatManager.QuotatIsSet   += DisableDifficultyUI;
        dayManager.DayBegin         += EnableDay;
        dayScript.EndShowing        += DisableDay;
        scoreManager.LaunchScoreAnim += EnableScore;
        dayManager.DayBegin         += DisableScore;
    }

    private void OnDisable()
    {
        quotatManager.QuotatIsSet   -= DisableDifficultyUI;
        dayManager.DayBegin         -= EnableDay;
        dayScript.EndShowing        -= DisableDay;
        scoreManager.LaunchScoreAnim -= EnableScore;
        dayManager.DayBegin         -= DisableScore;
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
        Debug.LogWarning("je lance");
        RectTransform rect = ScoreUi.GetComponent<RectTransform>();

        Vector2 startpos  = new Vector2(rect.anchoredPosition.x, ybasePose);
        Vector2 targetPos = new Vector2(rect.anchoredPosition.x, yendPose);
        float t = 0;

        while (t < animDuration)
        {
            t += Time.deltaTime;
            float normalized = t / animDuration;
            float curve      = curveAnim.Evaluate(normalized);
            rect.anchoredPosition = Vector2.Lerp(targetPos, startpos, curve);
            yield return null;
        }
    }

    public void DisableScore()
    {
        ScoreReset?.Invoke();
        Score.SetActive(false);
    }

    /// <summary>Ouvre la boutique et notifie le TutorialManager lors de la première ouverture.</summary>
    public void EnableShop()
    {
        shop.SetActive(true);
        StartCoroutine(AnimShop());

        if (_shopOpenedNotified) return;
        _shopOpenedNotified = true;
        TutorialManager.NotifyShopOpened();
    }

    public IEnumerator AnimShop()
    {
        Debug.LogWarning("je lance");
        RectTransform rect = ShopScene.GetComponent<RectTransform>();

        Vector2 startpos  = new Vector2(rect.anchoredPosition.x, 2500f);
        Vector2 targetPos = new Vector2(rect.anchoredPosition.x, 0f);
        float t = 0;

        while (t < 2f)
        {
            t += Time.deltaTime;
            float normalized = t / animDuration;
            float curve      = curveAnim.Evaluate(normalized);
            rect.anchoredPosition = Vector2.Lerp(startpos, targetPos, curve);
            yield return null;
        }
    }
}
