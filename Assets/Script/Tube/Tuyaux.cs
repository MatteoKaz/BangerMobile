using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.U2D;

public class Tuyaux : MonoBehaviour
{
  
    public PaperType tuyauxType;
    public event Action AddPaper;
    [SerializeField] Light2D lightcomp;
    public Color baseColor;
[SerializeField] private AudioEventDispatcher audioEventDispatcher;
    public void Start()
    {
        baseColor = lightcomp.color;
    }
    public void GoodPaper()
    {
        audioEventDispatcher.PlayAudio(AudioType.Point);
        Debug.Log("+1");
        AddPaper?.Invoke();
        lightcomp.color = baseColor;
        StartCoroutine(LightCoroutine());
    }

    public void WrongPaper()
    {
        audioEventDispatcher.PlayAudio(AudioType.Wrong);
        lightcomp.color = Color.indianRed;
        StartCoroutine(LightCoroutine());
        
    }
   public IEnumerator LightCoroutine()
    {
            
        lightcomp.intensity = 1f;
        yield return new WaitForSeconds(0.25f);
        lightcomp.intensity = 0f;
    }
}
