using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Pole))]
public class PoleBoostOverlay : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private BoostManager boostManager;
    [SerializeField] private UnityEngine.UI.Image highlightImage;
    [SerializeField] private TextMeshProUGUI boostText;
    [SerializeField] private ParticleSystem confettiParticles;

    [Header("Paramètres visuels")]
    [SerializeField] private float minAlpha = 0f;
    [SerializeField] private float maxAlpha = 0.25f;
    [SerializeField] private float pulseSpeed = 7f;

    private Pole _pole;
    private Coroutine _visualCoroutine;
    private Color _baseColor;

    private void Awake()
    {
        _pole = GetComponent<Pole>();

        if (highlightImage != null)
            _baseColor = highlightImage.color;
    }

    private void OnEnable()
    {
        boostManager.BoostStarted += OnBoostStarted;
        boostManager.BoostEnded   += OnBoostEnded;
    }

    private void OnDisable()
    {
        boostManager.BoostStarted -= OnBoostStarted;
        boostManager.BoostEnded   -= OnBoostEnded;
    }

    private void OnBoostStarted(Pole pole)
    {
        if (pole != _pole) return;

        if (confettiParticles != null)
        {
            confettiParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            confettiParticles.Play();
        }

        if (_visualCoroutine != null) StopCoroutine(_visualCoroutine);
        _visualCoroutine = StartCoroutine(ShowOverlay());
    }

    private void OnBoostEnded(Pole pole)
    {
        if (pole != _pole) return;

        if (confettiParticles != null)
            confettiParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (_visualCoroutine != null) StopCoroutine(_visualCoroutine);
        _visualCoroutine = StartCoroutine(HideOverlay());
    }


    private IEnumerator ShowOverlay()
    {
        if (boostText != null)
        {
            boostText.text = "x2";
            boostText.gameObject.SetActive(true);
        }

        while (true)
        {
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;

            if (highlightImage != null)
                highlightImage.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, Mathf.Lerp(minAlpha, maxAlpha, t));

            yield return null;
        }
    }

    private IEnumerator HideOverlay()
    {
        float startAlpha = highlightImage != null ? highlightImage.color.a : 0f;
        float t = 0f;

        while (t < 0.4f)
        {
            t += Time.deltaTime;
            float ratio = 1f - Mathf.Clamp01(t / 0.4f);

            if (highlightImage != null)
                highlightImage.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, startAlpha * ratio);

            yield return null;
        }

        if (highlightImage != null)
            highlightImage.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, 0f);

        if (boostText != null)
            boostText.gameObject.SetActive(false);
    }
}
