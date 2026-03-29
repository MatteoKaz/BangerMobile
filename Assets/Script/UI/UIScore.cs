using System.Collections;
using TMPro;
using UnityEngine;

public class UIScore : MonoBehaviour
{
    [SerializeField] UiManager uiManager;
    [SerializeField] TextMeshProUGUI UiScore;
    [SerializeField] TextMeshProUGUI Uimoney;
    [SerializeField] ScoreManager scoreManager;

    [Header("Texte de bonus pôle")]
    [Tooltip("TextMeshPro qui affiche le statut de chaque pôle.")]
    [SerializeField] TextMeshProUGUI bonusLabel;

    [Header("Textes personnalisables")]
    [SerializeField] string texteQuotaAtteint = "Quota atteint";
    [SerializeField] string texteQuotaPasAtteint = "Quota pas atteint";
    [SerializeField] string prefixeBonus = "+";
    [SerializeField] string suffixeBonus = "%";

    [Header("Timings")]
    [SerializeField] float pauseBeforeEachPole = 0.6f;
    [SerializeField] float statusDisplayDuration = 0.8f;
    [SerializeField] float bonusDisplayDuration = 0.8f;
    [SerializeField] float bonusAnimDuration = 0.6f;

    [Header("Vibration du label")]
    [SerializeField] float shakeMagnitude = 5f;
    [SerializeField] float shakeDuration = 0.3f;

    private void OnEnable()
    {
        uiManager.ScoreAnim += LaunchAnim;
        uiManager.ScoreReset += ResetScore;
    }

    private void OnDisable()
    {
        uiManager.ScoreAnim -= LaunchAnim;
        uiManager.ScoreReset -= ResetScore;
    }

    public void ResetScore()
    {
        UiScore.text = null;
        Uimoney.text = "Argent:";
        if (bonusLabel != null)
            bonusLabel.gameObject.SetActive(false);
    }

    public void LaunchAnim()
    {
        StartCoroutine(ScoreAnim());
    }

    /// <summary>Fait vibrer le RectTransform du bonusLabel pendant une courte durée.</summary>
    private IEnumerator ShakeText()
    {
        RectTransform rect = bonusLabel.rectTransform;
        Vector2 originalPos = rect.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float offsetX = Random.Range(-shakeMagnitude, shakeMagnitude);
            float offsetY = Random.Range(-shakeMagnitude, shakeMagnitude);
            rect.anchoredPosition = originalPos + new Vector2(offsetX, offsetY);
            elapsed += Time.deltaTime;
            yield return null;
        }

        rect.anchoredPosition = originalPos;
    }

    public IEnumerator ScoreAnim()
    {
        yield return new WaitForSeconds(1.5f);

        // --- Animation du score (quotat) ---
        float duration = 1f;
        float t = 0f;
        int score = scoreManager.playerQuotat;

        while (t < duration)
        {
            t += Time.deltaTime;
            int display = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(0, score, t / duration)), 0, score);
            UiScore.text = $"{display}/{scoreManager.quotatOfTheDay}";
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);

        // --- Animation de l'argent de BASE ---
        t = 0f;
        int baseMoney = scoreManager.baseMoney;
        int currentDisplayMoney = 0;

        while (t < duration)
        {
            t += Time.deltaTime;
            currentDisplayMoney = Mathf.Clamp(Mathf.RoundToInt(Mathf.Lerp(0, baseMoney, t / duration)), 0, baseMoney);
            Uimoney.text = $"Argent: {currentDisplayMoney}";
            yield return null;
        }
        currentDisplayMoney = baseMoney;
        Uimoney.text = $"Argent: {currentDisplayMoney}";

        // --- Résultats par pôle ---
        if (bonusLabel != null)
        {
            foreach (ScoreManager.PoleResult result in scoreManager.poleResults)
            {
                yield return new WaitForSeconds(pauseBeforeEachPole);

                if (result.quotaReached)
                {
                    // Étape 1 : "Pôle X - Quota atteint"
                    bonusLabel.text = $"{result.poleName} - {texteQuotaAtteint}";
                    bonusLabel.gameObject.SetActive(true);
                    StartCoroutine(ShakeText());

                    yield return new WaitForSeconds(statusDisplayDuration);

                    // Étape 2 : "+X%"
                    int bonusPct = Mathf.RoundToInt(result.bonusPercent * 100f);
                    bonusLabel.text = $"{prefixeBonus}{bonusPct}{suffixeBonus}";
                    StartCoroutine(ShakeText());

                    yield return new WaitForSeconds(bonusDisplayDuration);

                    // Étape 3 : Animer l'argent qui monte
                    int bonusAmount = Mathf.RoundToInt(baseMoney * result.bonusPercent);
                    int targetMoney = currentDisplayMoney + bonusAmount;
                    t = 0f;
                    int startMoney = currentDisplayMoney;

                    while (t < bonusAnimDuration)
                    {
                        t += Time.deltaTime;
                        currentDisplayMoney = Mathf.RoundToInt(Mathf.Lerp(startMoney, targetMoney, t / bonusAnimDuration));
                        Uimoney.text = $"Argent: {currentDisplayMoney}";
                        yield return null;
                    }

                    currentDisplayMoney = targetMoney;
                    Uimoney.text = $"Argent: {currentDisplayMoney}";
                }
                else
                {
                    // Quota pas atteint : affiche le message sans bonus
                    bonusLabel.text = $"{result.poleName} - {texteQuotaPasAtteint}";
                    bonusLabel.gameObject.SetActive(true);
                    StartCoroutine(ShakeText());

                    yield return new WaitForSeconds(statusDisplayDuration);
                }

                bonusLabel.gameObject.SetActive(false);
            }
        }
    }
}
