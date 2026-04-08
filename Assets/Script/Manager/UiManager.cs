using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UiManager : MonoBehaviour
{
    [SerializeField] DayManager dayManager;
    [SerializeField] QuotatManager quotatManager;
    [SerializeField] ScoreManager scoreManager;
    [SerializeField] UIScore uiscore;
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

    [Header("Score Button")]
    [SerializeField] Button scoreButton;

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

    bool doOnce = false;
    private bool _shopOpenedNotified = false;

    private bool scoreAnimationFinished = false;

    private const float DelayBeforeButtonOnClickActive = 1f;

    private void OnEnable()
    {
        quotatManager.QuotatIsSet += DisableDifficultyUI;
        dayManager.DayBegin += EnableDay;
        dayScript.EndShowing += DisableDay;
        scoreManager.LaunchScoreAnim += EnableScore;
        dayManager.DayBegin += DisableScoreDay;
    }

    private void OnDisable()
    {
        quotatManager.QuotatIsSet -= DisableDifficultyUI;
        dayManager.DayBegin -= EnableDay;
        dayScript.EndShowing -= DisableDay;
        scoreManager.LaunchScoreAnim -= EnableScore;
        dayManager.DayBegin -= DisableScoreDay;
    }

    /// <summary>Déclenche la fermeture de l'UI de difficulté.</summary>
    public void DisableDifficultyUI()
    {
        StartCoroutine(CloseDifficultyUI());
    }

    /// <summary>Joue l'animation puis masque le panel de difficulté.</summary>
    public IEnumerator CloseDifficultyUI()
    {
        DifficultyChosenAnim?.Invoke();
        yield return new WaitForSeconds(waitTimeBeforeCloseDifficulty);
        DifficultyChoice.SetActive(false);
    }

    /// <summary>Active le panel de jour et lance les animations associées.</summary>
    public void EnableDay()
    {
        Day.SetActive(true);
        Difficulty.SetActive(true);
        dayResetOpacity?.Invoke();
        LaunchDayAnim?.Invoke();
        StartCoroutine(waitToShowDifficulty());
    }

    /// <summary>Attend avant d'afficher l'animation de difficulté.</summary>
    public IEnumerator waitToShowDifficulty()
    {
        yield return new WaitForSeconds(0.5f);
        DifficultyShownAnim?.Invoke();
    }

    /// <summary>Masque le panel de jour.</summary>
    public void DisableDay()
    {
        Day.SetActive(false);
    }

    /// <summary>Active le panel de score et lance l'animation.</summary>
    public void EnableScore()
    {
        doOnce = false;
        scoreAnimationFinished = false;

        Score.SetActive(true);
        DifficultyChoice.SetActive(true);
        Day.SetActive(true);

        if (scoreButton != null)
            scoreButton.interactable = false;

        StartCoroutine(AnimScore());
        StartCoroutine(EnableScoreButton());

        ScoreAnim?.Invoke();
        dayResetOpacity?.Invoke();

        if (dayManager.currentDay == 5 && dayManager.currentWeek == 1)
            TutorialManager.NotifyFirstFridayScore();
    }

    /// <summary>Attend la fin de l'animation du score pour activer le bouton.</summary>
    public IEnumerator EnableScoreButton()
    {
        if (scoreButton != null)
            scoreButton.onClick.RemoveAllListeners();

        yield return new WaitUntil(() => scoreAnimationFinished && uiscore.hasFinish);

        if (scoreButton != null)
            scoreButton.interactable = true;

        yield return new WaitForSeconds(DelayBeforeButtonOnClickActive);

        if (scoreButton != null)
        {
            scoreButton.onClick.AddListener(DisableScore);
            scoreButton.onClick.AddListener(EnableShop);
        }
    }

    /// <summary>Animation d'entrée du score.</summary>
    public IEnumerator AnimScore()
    {
        RectTransform rect = ScoreUi.GetComponent<RectTransform>();

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

        scoreAnimationFinished = true;
    }

    /// <summary>Masque immédiatement le score au début d'une nouvelle journée.</summary>
    public void DisableScoreDay()
    {
        if (!doOnce)
        {
            doOnce = true;
            StartCoroutine(DisableScoreDayCoroutine());
        }
    }

    public IEnumerator DisableScoreDayCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        ScoreReset?.Invoke();
        Score.SetActive(false);
    }

    /// <summary>Lance la fermeture du score.</summary>
    public void DisableScore()
    {
        if (!doOnce)
        {
            doOnce = true;
            StartCoroutine(WaitAndDisableScore());
        }
    }

    public IEnumerator WaitAndDisableScore()
    {
        yield return new WaitForSeconds(0.2f);
        ScoreReset?.Invoke();
        Score.SetActive(false);
    }

    /// <summary>Ouvre le shop.</summary>
    public void EnableShop()
    {
        if (!doOnce)
        {
            StartCoroutine(WaitAndEnableShop());
        }
    }

    public IEnumerator WaitAndEnableShop()
    {
        yield return new WaitForSeconds(0.2f);
        shop.SetActive(true);
        StartCoroutine(AnimShop());

        if (_shopOpenedNotified) yield break;
        _shopOpenedNotified = true;

        TutorialManager.NotifyShopOpened();

        if (dayManager.currentDay == 5 && dayManager.currentWeek == 1)
            TutorialManager.NotifyFirstFridayShop();
    }

    /// <summary>Animation d'ouverture du shop.</summary>
    public IEnumerator AnimShop()
    {
        RectTransform rect = ShopScene.GetComponent<RectTransform>();

        Vector2 startpos = new Vector2(rect.anchoredPosition.x, 2500f);
        Vector2 targetPos = new Vector2(rect.anchoredPosition.x, 0f);

        float t = 0;

        while (t < 2f)
        {
            t += Time.deltaTime;

            float normalized = t / animDuration;
            float curve = curveAnim.Evaluate(normalized);

            rect.anchoredPosition = Vector2.Lerp(startpos, targetPos, curve);

            yield return null;
        }
    }
}