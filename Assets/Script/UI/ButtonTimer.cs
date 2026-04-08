using System.Collections;
using UnityEngine;

public class ButtonTimer : MonoBehaviour
{
    [SerializeField] private GameObject button;
    [SerializeField] private AudioEventDispatcher audioEventManager;
    [SerializeField] private TimeManager timeManager;

    [Tooltip("Délai en secondes avant de quitter le menu score après le clic.")]
    [SerializeField] private float exitDelay = 1f;

    public AnimationCurve bounceCurve;
    public bool OnGoing = false;

    private void OnDisable()
    {
        // Sécurité : si le panel est désactivé pendant l'animation, on remet OnGoing à false
        OnGoing = false;
    }

    /// <summary>Appelé par le bouton Terminer. Lance l'animation puis attend le délai avant de quitter.</summary>
    public void OnClick()
    {
        Debug.Log($"[ButtonTimer] OnClick — OnGoing={OnGoing}");

        if (OnGoing)
            return;

        OnGoing = true;
        StartCoroutine(ClickAnim());
    }

    private IEnumerator ClickAnim()
    {
        Vector2 originalPos = button.transform.localPosition;
        float t = 0f;

        // Descend
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / 0.08f;
            float normalized = Mathf.Clamp01(t);
            button.transform.localPosition = Vector2.Lerp(originalPos, originalPos + Vector2.down * 24f, normalized);
            yield return null;
        }

        t = 0f;

        // Bounce up
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / 0.35f;
            float normalized = Mathf.Clamp01(t);
            button.transform.localPosition = Vector2.Lerp(originalPos + Vector2.down * 25f, originalPos, bounceCurve.Evaluate(normalized));
            yield return null;
        }

        button.transform.localPosition = originalPos;

        yield return new WaitForSecondsRealtime(exitDelay);

        Debug.Log("[ButtonTimer] EndDay déclenché");
        timeManager.EndDay();
        OnGoing = false;
    }
}