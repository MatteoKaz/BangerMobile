using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Fade : MonoBehaviour
{
    [Header("EffectValue")]
    [SerializeField] float fadeDuration = 1.0f;
    [SerializeField] float fadeTextDelay = 1.25f;
    [SerializeField] float fadeImageDelay = 0.5f;

    [Header("ref")]
    [SerializeField] DayManager dayManager;
    [SerializeField] UIDay uIDay;
    [SerializeField] Image image;
    [SerializeField] TextMeshProUGUI textMeshPro;
    private void OnEnable()
    {
        dayManager.DayBegin += LaunchFadeIn;
        if(uIDay !=null)
        {
            uIDay.LaunchFade += LaunchFade;
            uIDay.LaunchFadeIN += LaunchFadeIn;
            
        }
    }
    private void OnDisable()
    {
        dayManager.DayBegin -= LaunchFadeIn;
        if (uIDay != null)
        {
            uIDay.LaunchFade -= LaunchFade;
            uIDay.LaunchFadeIN -= LaunchFadeIn;
           
        }
    }
    public void LaunchFade()
    {
        if (image != null)
        {
            StartCoroutine(FadeImage());
            
        }
        if (textMeshPro != null)
        {
            
            StartCoroutine(Fadetext());
        }
        
    }


    public void LaunchFadeIn()
    {
        if (image != null)
        {
            StartCoroutine(FadeInImage());
            

        }
        if (textMeshPro != null)
        {
            Debug.Log("FADE");
            StartCoroutine(FadeInText());
        }

    }
    public IEnumerator FadeImage()
    {
        yield return new WaitForSeconds(fadeImageDelay);
        float t = 0f;
        Color startColor = image.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f); // alpha = 0
        float colorMatEnd = 0f;
        float colorMatStart = 1f;
        float ColorToUse = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;

                float normalizedTime = t / fadeDuration;

                image.color = Color.Lerp(startColor, targetColor, normalizedTime);
                if(image.material!= null)
                ColorToUse = Mathf.Lerp(colorMatStart, colorMatEnd, normalizedTime);
                image.material.SetFloat("Opacity",ColorToUse);

            yield return null;
            }
        
    }
    public IEnumerator FadeInImage()
    {
        
        float t = 0f;
        Color startColor = image.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 1f);
        float colorMatEnd = 1f;
        float colorMatStart = 0f;
        float ColorToUse = 0f;// alpha = 0
        if (startColor == image.color)
            yield break;
        while (t < fadeDuration)
            {
                t += Time.deltaTime;

                float normalizedTime = t / fadeDuration;

                image.color = Color.Lerp(startColor, targetColor, normalizedTime);
            if (image.material != null)
                ColorToUse = Mathf.Lerp(colorMatStart, colorMatEnd, normalizedTime);
            image.material.SetFloat("Opacity", ColorToUse);
            yield return null;
            }
        
    }
    public IEnumerator Fadetext()
    {
        float t = 0f;
        Color startColor = textMeshPro.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 0f); // alpha = 0
        
            while (t < fadeDuration)
            {
                t += Time.deltaTime;

                float normalizedTime = t / fadeDuration;

                textMeshPro.color = Color.Lerp(startColor, targetColor, normalizedTime);

                yield return null;
            }
        
    }

    public IEnumerator FadeInText()
    {
        yield return new WaitForSeconds(fadeTextDelay);
        float t = 0f;
        Color startColor = textMeshPro.color;
        Color targetColor = new Color(startColor.r, startColor.g, startColor.b, 1f); // alpha = 0
       
            while (t < fadeDuration)
            {
                t += Time.deltaTime;

                float normalizedTime = t / fadeDuration;

                textMeshPro.color = Color.Lerp(startColor, targetColor, normalizedTime);

                yield return null;
            }
        
    }
}
