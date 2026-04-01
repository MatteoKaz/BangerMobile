using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class SwatManager : MonoBehaviour
{
    public int numberOfUtilisation = 0;
    
    public float SwatDuration = 14f;
    public Light2D light;
    [SerializeField] DayManager dayManager;
    [SerializeField] Light2D lightblue;
    [SerializeField] Light2D lightred;

    public event Action SwatModeStart;
    public event Action SwatModeEnd;

    public void OnEnable()
    {
        dayManager.DayEnd += DeactivateButton;
    }

    public void OnDisable()
    {
        dayManager.DayEnd -= DeactivateButton;
    }

    public void OnBuyActivation()
    {
        
        lightGO();
    }
    public void ActivateButton()
    {
        if (numberOfUtilisation <= 0) return;
        SwatModeStart?.Invoke();
    }
    public void DeactivateButton()
    {

        SwatModeEnd?.Invoke();
    }

    public void OnPoleSelected(Pole pole)
    {
        if (numberOfUtilisation <= 0) return;
        numberOfUtilisation--;


        foreach (Employe emp in pole.employeList)
        {
            emp.OnSwat(); // ou via event
        }

        DeactivateButton();
        StartCoroutine(SwatLight());
        // désactive les boutons
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
        StartCoroutine(Light());
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
}
