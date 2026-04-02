using System.Collections;
using TMPro;
using UnityEngine;

public class FeedbackMoneyEmploye : MonoBehaviour
{
    [SerializeField] Employe employe;
    [SerializeField] Transform scoreTarget;
    [SerializeField] Canvas canvas;
    [SerializeField] GameObject popupTextPrefab;

    private void OnEnable()
    {
        employe.ScoreWinAnim += OnScoreWinAnim;
    }

    private void OnDisable()
    {
        employe.ScoreWinAnim -= OnScoreWinAnim;
    }

    private void OnScoreWinAnim()
    {
        Transform target = employe.mypole?.quotatTextTarget ?? scoreTarget;
        StartCoroutine(PopAndMoveToScore($"+{employe.mypole.paperValue}$", transform.position, target));
    }

    private IEnumerator PopAndMoveToScore(string text, Vector3 spawnPosition, Transform target)
    {
        GameObject popup = Instantiate(popupTextPrefab, spawnPosition, Quaternion.identity, transform);
        TextMeshProUGUI tmp = popup.GetComponent<TextMeshProUGUI>();
        tmp.text = text;

        float t = 0f;
        float popDuration = 0.3f;
        popup.transform.localScale = Vector3.zero;
        while (t < popDuration)
        {
            t += Time.deltaTime;
            float ratio = t / popDuration;
            float scale = ratio < 0.6f
                ? Mathf.Lerp(0f, 1.5f, ratio / 0.6f)
                : Mathf.Lerp(1.5f, 1f, (ratio - 0.6f) / 0.4f);
            popup.transform.localScale = Vector3.one * scale;
            yield return null;
        }

        yield return new WaitForSeconds(0.1f);

        t = 0f;
        float moveDuration = 0.5f;
        Vector3 startPos = popup.transform.position;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float ratio = Mathf.Clamp01(t / moveDuration);
            popup.transform.position = Vector3.Lerp(startPos, target.position, ratio);
            popup.transform.localScale = Vector3.one * Mathf.Lerp(1f, 0f, ratio * ratio);
            yield return null;
        }
        employe.mypole.UpdateUI();
        Destroy(popup);
    }
}