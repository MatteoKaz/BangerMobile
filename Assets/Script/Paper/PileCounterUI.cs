using UnityEngine;
using TMPro;
using System.Collections;

public class PileCounterUI : MonoBehaviour
{
    public Pile pile;
    public TextMeshProUGUI counterText;

    private float baseSize;
    private Coroutine animRoutine;

    private void Start()
    {
        baseSize = counterText.fontSize;

        UpdateCounter();
        pile.UpdateCount += UpdateCounter;
    }

    private void OnDestroy()
    {
        pile.UpdateCount -= UpdateCounter;
    }

    private void UpdateCounter()
    {
        counterText.text = pile.paperCount.ToString();

        if (animRoutine != null)
            StopCoroutine(animRoutine);

        animRoutine = StartCoroutine(AnimateFontSize());
    }

    private IEnumerator AnimateFontSize()
    {
        float duration = 0.35f;
        float t = 0f;

        float targetSize = baseSize * 1.35f;

        // Agrandissement progressif
        while (t < 1f)
        {
            t += Time.deltaTime / (duration * 0.5f);
            float smooth = Mathf.SmoothStep(0f, 1f, t);
            counterText.fontSize = Mathf.Lerp(baseSize, targetSize, smooth);
            yield return null;
        }

        // Retour progressif
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / (duration * 0.5f);
            float smooth = Mathf.SmoothStep(0f, 1f, t);
            counterText.fontSize = Mathf.Lerp(targetSize, baseSize, smooth);
            yield return null;
        }

        counterText.fontSize = baseSize;
    }
}