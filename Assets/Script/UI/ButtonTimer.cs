using System.Collections;
using UnityEngine;

public class ButtonTimer : MonoBehaviour
{
    [SerializeField] GameObject button;
    [SerializeField] AudioEventDispatcher audioEventManager;
    public AnimationCurve bounceCurve;
    [SerializeField] TimeManager timeManager;
    public bool OnGoing = false;
    public void OnClick()
    {
        if (OnGoing == true)
            return;
        StartCoroutine(ClickAnim());
    }
   
    public IEnumerator ClickAnim()
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
        timeManager.EndDay();
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
        OnGoing = false;
    }
}
