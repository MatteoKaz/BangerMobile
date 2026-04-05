using System.Collections;
using TMPro;
using UnityEngine;

public class FeedbackMoneyEmploye : MonoBehaviour
{
    [SerializeField] private Employe employe;
    [SerializeField] private Transform scoreTarget;
    [SerializeField] private Canvas canvas;
    [SerializeField] private GameObject popupTextPrefab;
    [SerializeField] private AudioEventDispatcher audioEventDispatcher;

    private void OnEnable()
    {
        employe.ScoreWinAnim += OnScoreWinAnim;
    }

    private void OnDisable()
    {
        employe.ScoreWinAnim -= OnScoreWinAnim;
    }

    /// <summary>Déclenché quand un employé termine un papier. Affiche la vraie valeur gagnée (base × BonusRevenus).</summary>
    private void OnScoreWinAnim()
    {
        if (employe.mypole == null) return;

        Transform target = employe.mypole.quotatTextTarget != null ? employe.mypole.quotatTextTarget : scoreTarget;
        int realValue = employe.mypole.paperValue
        * Mathf.RoundToInt(employe.mypole.BonusRevenus)
        * Mathf.RoundToInt(employe.mypole.CurrentBoostMultiplier);

        StartCoroutine(PopAndMoveToScore($"+{realValue}$", transform.position, target));
        audioEventDispatcher.PlayAudio(AudioType.Gain);
    }

    private IEnumerator PopAndMoveToScore(string text, Vector3 spawnPosition, Transform target)
    {
        GameObject popup = Instantiate(popupTextPrefab, spawnPosition, Quaternion.identity, transform);
        TextMeshProUGUI tmp = popup.GetComponent<TextMeshProUGUI>();
        tmp.text = text;

        // Animation d'apparition avec overshoot
        float t = 0f;
        const float PopDuration = 0.3f;
        popup.transform.localScale = Vector3.zero;

        while (t < PopDuration)
        {
            t += Time.deltaTime;
            float ratio = t / PopDuration;
            float scale = ratio < 0.6f
                ? Mathf.Lerp(0f, 1.5f, ratio / 0.6f)
                : Mathf.Lerp(1.5f, 1f, (ratio - 0.6f) / 0.4f);
            popup.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        // Déplacement vers le quota
        t = 0f;
        const float MoveDuration = 0.5f;
        Vector3 startPos = popup.transform.position;

        while (t < MoveDuration)
        {
            t += Time.deltaTime;
            float ratio = Mathf.Clamp01(t / MoveDuration);
            popup.transform.position = Vector3.Lerp(startPos, target.position, ratio);
            popup.transform.localScale = Vector3.one * Mathf.Lerp(1f, 0f, ratio * ratio);
            yield return null;
        }

        employe.mypole.UpdateUI();
        Destroy(popup);
    }
}
