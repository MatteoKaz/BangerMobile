using System;
using System.Collections;
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
    [SerializeField] float waitTimeBeforeButtonActive = 1f;

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
        quotatManager.QuotatIsSet    += DisableDifficultyUI;
        dayManager.DayBegin          += EnableDay;
        dayScript.EndShowing         += DisableDay;
        scoreManager.LaunchScoreAnim += EnableScore;
        dayManager.DayBegin          += DisableScoreDay;
    }

    private void OnDisable()
    {
        quotatManager.QuotatIsSet    -= DisableDifficultyUI;
        dayManager.DayBegin          -= EnableDay;
        dayScript.EndShowing         -= DisableDay;
        scoreManager.LaunchScoreAnim -= EnableScore;
        dayManager.DayBegin          -= DisableScoreDay;
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

    /// <summary>Active le panel de score, désactive le bouton, et lance les animations associées.</summary>
    public void EnableScore()
    {
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

    /// <summary>Attend le délai configuré puis rend le bouton interactable.</summary>
    private const float DelayBeforeButtonOnClickActive = 1f;

    /// <summary>
    /// Attend le délai configuré puis rend le bouton interactable,
    /// puis attend 1 seconde supplémentaire avant de réactiver ses événements onClick.
    /// </summary>
    public IEnumerator EnableScoreButton()
    {
        if (scoreButton != null)
            scoreButton.onClick.RemoveAllListeners();

        yield return new WaitForSeconds(waitTimeBeforeButtonActive);

        if (scoreButton != null)
            scoreButton.interactable = true;

        yield return new WaitForSeconds(DelayBeforeButtonOnClickActive);

        if (scoreButton != null)
        {
            scoreButton.onClick.AddListener(DisableScore);
            scoreButton.onClick.AddListener(EnableShop);
        }
    }


    /// <summary>Anime l'entrée du panel de score.</summary>
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

    /// <summary>Masque immédiatement le score au début d'une nouvelle journée.</summary>
    public void DisableScoreDay()
    {
        ScoreReset?.Invoke();
        Score.SetActive(false);
    }

    /// <summary>Lance la coroutine de fermeture du score depuis un bouton UI.</summary>
    public void DisableScore()
    {
        StartCoroutine(WaitAndDisableScore());
    }

    /// <summary>Attend que l'animation du score soit terminée, puis masque le panel de score.</summary>
    public IEnumerator WaitAndDisableScore()
    {
        yield return new WaitUntil(() => uiscore.hasFinish == true);
        ScoreReset?.Invoke();
        Score.SetActive(false);
    }

    /// <summary>Lance la coroutine d'ouverture du shop depuis un bouton UI.</summary>
    public void EnableShop()
    {
        StartCoroutine(WaitAndEnableShop());
    }

    /// <summary>Attend que l'animation du score soit terminée, puis affiche le shop et notifie le tutoriel.</summary>
    public IEnumerator WaitAndEnableShop()
    {
        yield return new WaitUntil(() => uiscore.hasFinish == true);
        shop.SetActive(true);
        StartCoroutine(AnimShop());

        if (_shopOpenedNotified) yield break;
        _shopOpenedNotified = true;
        TutorialManager.NotifyShopOpened();

        if (dayManager.currentDay == 5 && dayManager.currentWeek == 1)
            TutorialManager.NotifyFirstFridayShop();
    }

    /// <summary>Anime l'entrée du panel de shop.</summary>
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
