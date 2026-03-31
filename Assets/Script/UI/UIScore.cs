using System.Collections;
using TMPro;
using UnityEngine;

public class UIScore : MonoBehaviour
{
    [SerializeField] UiManager uiManager;
    [SerializeField] ScoreManager scoreManager;

    [Header("Quota par pôle (un label par pôle, dans l'ordre)")]
    [SerializeField] TextMeshProUGUI[] poleQuotaLabels;

    [Header("Bénéfice de la partie")]
    [SerializeField] TextMeshProUGUI beneficeLabel;

    [Header("Total argent joueur")]
    [SerializeField] TextMeshProUGUI totalMoneyLabel;

    [Header("Timings")]
    [SerializeField] float delayBeforeStart      = 1.5f;
    [SerializeField] float quotaCountDuration    = 1.0f;
    [SerializeField] float pauseAfterQuota       = 0.4f;
    [SerializeField] float baseMoneyCountDuration = 1.0f;
    [SerializeField] float pauseBetweenBonuses   = 0.4f;
    [SerializeField] float bonusCountDuration    = 0.6f;
    [SerializeField] float pauseBeforeTotal      = 0.4f;
    [SerializeField] float totalCountDuration    = 1.0f;

    private void OnEnable()
    {
        uiManager.ScoreAnim  += LaunchAnim;
        uiManager.ScoreReset += ResetScore;
    }

    private void OnDisable()
    {
        uiManager.ScoreAnim  -= LaunchAnim;
        uiManager.ScoreReset -= ResetScore;
    }

    /// <summary>Réinitialise tous les textes du menu de score.</summary>
    public void ResetScore()
    {
        foreach (TextMeshProUGUI label in poleQuotaLabels)
            if (label != null) label.text = string.Empty;

        if (beneficeLabel  != null) beneficeLabel.text  = string.Empty;
        if (totalMoneyLabel != null) totalMoneyLabel.text = string.Empty;
    }

    /// <summary>Lance l'animation d'affichage du score.</summary>
    public void LaunchAnim()
    {
        StartCoroutine(ScoreAnim());
    }

    /// <summary>Anime séquentiellement les quotas de pôle, le bénéfice, puis le total joueur.</summary>
    public IEnumerator ScoreAnim()
    {
        yield return new WaitForSeconds(delayBeforeStart);

        // ── Quotas par pôle ─────────────────────────────────────────────
        float t = 0f;
        while (t < quotaCountDuration)
        {
            t += Time.deltaTime;
            float ratio = Mathf.Clamp01(t / quotaCountDuration);

            for (int i = 0; i < poleQuotaLabels.Length; i++)
            {
                if (poleQuotaLabels[i] == null || i >= scoreManager.poleResults.Count) continue;

                ScoreManager.PoleResult r = scoreManager.poleResults[i];
                int displayed = Mathf.RoundToInt(Mathf.Lerp(0, r.advancement, ratio));
                poleQuotaLabels[i].text = $"{r.poleName} : {displayed} / {r.quota}";
            }

            yield return null;
        }

        // Valeurs finales exactes
        for (int i = 0; i < poleQuotaLabels.Length; i++)
        {
            if (poleQuotaLabels[i] == null || i >= scoreManager.poleResults.Count) continue;

            ScoreManager.PoleResult r = scoreManager.poleResults[i];
            poleQuotaLabels[i].text = $"{r.poleName} : {r.advancement} / {r.quota}";
        }

        yield return new WaitForSeconds(pauseAfterQuota);

        // ── Bénéfice de base ─────────────────────────────────────────────
        int baseMoney       = scoreManager.baseMoney;
        int displayBenefice = 0;

        t = 0f;
        while (t < baseMoneyCountDuration)
        {
            t += Time.deltaTime;
            displayBenefice = Mathf.RoundToInt(Mathf.Lerp(0, baseMoney, Mathf.Clamp01(t / baseMoneyCountDuration)));
            if (beneficeLabel != null) beneficeLabel.text = $"Bénéfice : {displayBenefice}";
            yield return null;
        }
        displayBenefice = baseMoney;
        if (beneficeLabel != null) beneficeLabel.text = $"Bénéfice : {displayBenefice}";

        // ── Bonus des pôles ───────────────────────────────────────────────
        foreach (ScoreManager.PoleResult result in scoreManager.poleResults)
        {
            if (!result.quotaReached) continue;

            yield return new WaitForSeconds(pauseBetweenBonuses);

            int bonusAmount    = Mathf.RoundToInt(baseMoney * result.bonusPercent);
            int startBenefice  = displayBenefice;
            int targetBenefice = displayBenefice + bonusAmount;
            int bonusPct       = Mathf.RoundToInt(result.bonusPercent * 100f);

            t = 0f;
            while (t < bonusCountDuration)
            {
                t += Time.deltaTime;
                displayBenefice = Mathf.RoundToInt(Mathf.Lerp(startBenefice, targetBenefice, Mathf.Clamp01(t / bonusCountDuration)));
                if (beneficeLabel != null)
                    beneficeLabel.text = $"Bénéfice : {displayBenefice}  (+{bonusPct}% {result.poleName})";
                yield return null;
            }
            displayBenefice = targetBenefice;
            if (beneficeLabel != null) beneficeLabel.text = $"Bénéfice : {displayBenefice}";
        }

        yield return new WaitForSeconds(pauseBeforeTotal);

        // ── Total argent joueur ───────────────────────────────────────────
        int previousMoney = scoreManager.playerMoney - displayBenefice;
        int finalMoney    = scoreManager.playerMoney;

        t = 0f;
        while (t < totalCountDuration)
        {
            t += Time.deltaTime;
            int displayed = Mathf.RoundToInt(Mathf.Lerp(previousMoney, finalMoney, Mathf.Clamp01(t / totalCountDuration)));
            if (totalMoneyLabel != null) totalMoneyLabel.text = $"Total : {displayed}";
            yield return null;
        }
        if (totalMoneyLabel != null) totalMoneyLabel.text = $"Total : {finalMoney}";
    }
}
