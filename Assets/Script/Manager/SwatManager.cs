using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class SwatManager : MonoBehaviour
{
    public int numberOfUtilisation = 0;
   
    public float SwatDuration = 14f;
    public Light2D light;
    [SerializeField] DayManager dayManager;
    [SerializeField] Light2D lightblue;
    [SerializeField] Light2D lightred;
    [SerializeField] Image highglightsRouge;
    [SerializeField] Image highglightsBleu;
    [SerializeField] Image highglightsVert;
    [SerializeField] Button[] buttons;
    [SerializeField] Image[] images;
    [SerializeField] Color redcolor;
    [SerializeField] Color bleucolor;
    [SerializeField] Color vertcolor;
    [SerializeField] GameObject PostIt;
    [SerializeField] GameObject parent;
    private Coroutine HighlightCoroutine;
    private Coroutine SwatLightLittle;
    
    [SerializeField] private AudioEventDispatcher audioEventManager;
    public event Action SwatModeStart;
    public event Action SwatModeEnd;
    private bool swatModeActive = false;

    public void OnEnable()
    {
        dayManager.DayBegin += activate;
        dayManager.DayEnd += DeactivateButton;
    }

    public void OnDisable()
    {
        dayManager.DayBegin -= activate;
        dayManager.DayEnd -= DeactivateButton;
    }

    public void OnBuyActivation()
    {
        
        lightGO();

    }
    public void activate()
    {
        StartCoroutine(Initialization());
    }
    public IEnumerator Initialization()
    {
        yield return new WaitForSeconds(0.75f);
        if (numberOfUtilisation > 0)
        {
            parent.SetActive(true);

            lightGO();
        }
    }
    public void ActivateButton()
    {
        if (numberOfUtilisation <= 0) return;
        SwatModeStart?.Invoke();
        swatModeActive = true;
        PostIt.SetActive(true);
    }
    public void DeactivateButton()
    {
        
        if (swatModeActive) 
        {
            numberOfUtilisation++; 
            swatModeActive = false;
            PostIt.SetActive(false);
        }
        if (numberOfUtilisation == 0)
        {
            parent.SetActive(false);
        }
        SwatModeEnd?.Invoke();
    }

    public void OnPoleSelected(Pole pole)
    {
        if (numberOfUtilisation <= 0) return;
        numberOfUtilisation--;
        swatModeActive = false;
        PostIt.SetActive(false);
        foreach (Button buton in buttons)
        {
            buton.enabled = false;
        }
        foreach (Image image in images)
        {
            image.enabled = false;
        }
        StopCoroutine(HighlightCoroutine);
        StartCoroutine(StopPulse());
        foreach (Employe emp in pole.employeList)
        {
            emp.OnSwat(); // ou via event
        }
        audioEventManager.PlayAudio(AudioType.Swat);
        DeactivateButton();
        StartCoroutine(SwatLight());
        // d�sactive les boutons
    }

    public IEnumerator SwatLight()
    {
        
        light.enabled = true;
        light.intensity = 0f;
        float t = 0f;
        while (t <0.25f)
        {
            t += Time.deltaTime;
            light.intensity = Mathf.Lerp(0, 3.2f, t);
            float speed = 3f;
            float ping = Mathf.PingPong(Time.time * speed, 1f);
            light.color = Color.Lerp(Color.red, Color.blue, ping);
            yield return null;
        }
        t = 0f;
        while (t < SwatDuration)
        {
            float speed = 3f;
            float ping = Mathf.PingPong(Time.time * speed, 1f);
            light.color = Color.Lerp(Color.red, Color.blue, ping);
            t += Time.deltaTime;
            yield return null;
        }
        t = 0f;
        while (t < 0.5f)
        {
            t += Time.deltaTime;
            light.intensity = Mathf.Lerp(3.2f, 0, t);
            float speed = 3f;
            float ping = Mathf.PingPong(Time.time * speed, 1f);
            light.color = Color.Lerp(Color.red, Color.blue, ping);
            yield return null;
        }
        light.enabled = false;
       // if (numberOfUtilisation <= 0) 
    }

    public void lightGO()
    {
        if (numberOfUtilisation <= 0)
            return;
        SwatLightLittle = StartCoroutine(Light());
    }

    public IEnumerator Light()
    {

        while (true)
        {
            float t = Mathf.PingPong(Time.time / 0.4f, 0.7f);
            lightblue.intensity = Mathf.Lerp(0f, 2.12f, t);
            lightred.intensity = Mathf.Lerp(2.12f, 0f, t);
            yield return null;
        }
    }

    public void StartPulse()
    {
        if (numberOfUtilisation <= 0) return;
        HighlightCoroutine = StartCoroutine(PulseHighlight());
        foreach (Button buton in buttons)
        {
            buton.enabled = true;
        }
        foreach (Image image in images)
        {
            image.enabled = true;
        }
    }
    public IEnumerator PulseHighlight()
    {
        
   
            while (true)
            {

                float t = (Mathf.Sin(Time.time * 5f) + 1f) / 2f;
                highglightsBleu.color = new Color(bleucolor.r, bleucolor.g, bleucolor.b, Mathf.Lerp(0f, 0.3f, t));
                highglightsRouge.color = new Color(redcolor.r, redcolor.g,redcolor.b, Mathf.Lerp(0f, 0.3f, t));
                highglightsVert.color = new Color(vertcolor.r,vertcolor.g, vertcolor.b  , Mathf.Lerp(0f, 0.3f, t));
                yield return null;
            }
        
    }

    public IEnumerator StopPulse()
    {
        float t = 0f;
        while (t<1f)
        {
            t += Time.deltaTime/0.1f;
            float normalized = Mathf.Clamp01(t);
            highglightsBleu.color = new Color(bleucolor.r, bleucolor.g, bleucolor.b, Mathf.Lerp(0.3f, 0f, t));
            highglightsRouge.color = new Color(redcolor.r, redcolor.g, redcolor.b, Mathf.Lerp(0.3f, 0.0f, t));
            highglightsVert.color = new Color(vertcolor.r, vertcolor.g, vertcolor.b, Mathf.Lerp(0.3f, 0.0f, t));
            yield return null;
        }
        if (SwatLightLittle != null)
        {
            if (numberOfUtilisation <= 0)
            {
                StopCoroutine(SwatLightLittle);
                SwatLightLittle = null;
                lightblue.intensity = 0f;
                lightred.intensity = 0f;
            }
        }
            
    }
}
