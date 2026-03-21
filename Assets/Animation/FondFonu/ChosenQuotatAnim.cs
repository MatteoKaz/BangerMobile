using System.Collections;
using UnityEngine;

public class ChosenQuotatAnim : MonoBehaviour
{
    [SerializeField] GameObject Case;

    [Header("EffectValue")]
    [SerializeField] float animDuraton = 0.3f;
    [SerializeField] Vector3 sizeShow;

    [SerializeField] UiManager uiManager;

    public void OnEnable()
    {
        if (uiManager != null)
        {
            uiManager.DifficultyChosenAnim += LaunchAnim;
            uiManager.DifficultyShownAnim += ShowBouton;
        }
    }

    public void OnDisable()
    {
        if (uiManager != null)
        {
            uiManager.DifficultyChosenAnim -= LaunchAnim;
            uiManager.DifficultyShownAnim -= ShowBouton;
        }
    }

    public void LaunchAnim()
    {
        StartCoroutine(SelectAnim());
    }

    public void ShowBouton()
    {
        StartCoroutine(ShowContent());
    }

    public IEnumerator SelectAnim()
    {
        float t = 0f;
        Vector3 startSize = Case.transform.localScale;
        Vector3 targetSize = new Vector3(0f, 0f, 0f); // alpha = 0

        while (t < animDuraton)
        {
            t += Time.deltaTime;

            float normalizedTime = t / animDuraton;

            Case.transform.localScale = Vector3.Lerp(startSize, targetSize, normalizedTime);

            yield return null;
        }

        


    }

    public IEnumerator ShowContent()
    {
        float t = 0f;
        Vector3 startSize = Case.transform.localScale;
        Vector3 targetSize = sizeShow; // alpha = 0

        while (t < animDuraton)
        {
            t += Time.deltaTime;

            float normalizedTime = t / animDuraton;

            Case.transform.localScale = Vector3.Lerp(startSize, targetSize, normalizedTime);

            yield return null;
        }

    }
}
