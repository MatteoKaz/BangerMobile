using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SwatButton : MonoBehaviour
{
    [SerializeField] GameObject button;
   
    public AnimationCurve bounceCurve;

    [SerializeField] SwatManager swatManager;

    public bool OnGoing = false;
    public void OnClick()
    {
        StartCoroutine(ClickAnim());
        swatManager.StartPulse();
    }

    public IEnumerator ClickAnim()
    {
        if (OnGoing == true)
            yield return null;
        OnGoing = true;
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
        swatManager.lightGO();
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
