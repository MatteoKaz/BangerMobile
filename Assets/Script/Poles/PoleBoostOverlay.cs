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

    [Header("Flottement du texte multiplicateur")]
    [SerializeField] private float floatAmplitude = 8f;
    [SerializeField] private float floatSpeed = 1.8f;

    [Header("Ombre du texte multiplicateur")]
    [SerializeField] private Color shadowColor = new Color(0f, 0f, 0f, 0.6f);
    [SerializeField] private Vector2 shadowOffset = new Vector2(0.5f, -0.5f);
    [SerializeField] private float shadowSoftness = 0f;

    private Pole _pole;
    private Coroutine _visualCoroutine;
    private Coroutine _floatCoroutine;
    private Color _baseColor;
    private Vector2 _boostTextBasePosition;
    [SerializeField] private BossEventManager bossEventManager;
    private void Awake()
    {
        _pole = GetComponent<Pole>();

        if (highlightImage != null)
            _baseColor = highlightImage.color;

        if (boostText != null)
        {
            _boostTextBasePosition = boostText.rectTransform.anchoredPosition;
            ApplyBoostTextShadow();
        }
    }

    private void OnEnable()
    {
        boostManager.BoostStarted += OnBoostStarted;
        boostManager.BoostEnded += OnBoostEnded;
    }

    private void OnDisable()
    {
        boostManager.BoostStarted -= OnBoostStarted;
        boostManager.BoostEnded -= OnBoostEnded;
    }

    /// <summary>Applique une ombre via l'Underlay TMP sur une instance de material dédiée.</summary>
    private void ApplyBoostTextShadow()
    {
        Material mat = boostText.fontMaterial;
        mat.EnableKeyword(ShaderUtilities.Keyword_Underlay);
        mat.SetColor(ShaderUtilities.ID_UnderlayColor, shadowColor);
        mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, shadowOffset.x);
        mat.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, shadowOffset.y);
        mat.SetFloat(ShaderUtilities.ID_UnderlaySoftness, shadowSoftness);
    }

    private void OnBoostStarted(Pole pole)
    {
        if (pole != _pole) return;
        if (bossEventManager != null && bossEventManager.IsEventActiveOn(_pole))
            return;
        if (confettiParticles != null)
        {
            confettiParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            confettiParticles.Play();
        }

        if (_visualCoroutine != null) StopCoroutine(_visualCoroutine);
        _visualCoroutine = StartCoroutine(ShowOverlay());

        if (_floatCoroutine != null) StopCoroutine(_floatCoroutine);
        _floatCoroutine = StartCoroutine(FloatBoostText());
    }

    private void OnBoostEnded(Pole pole)
    {
        if (pole != _pole) return;

        if (confettiParticles != null)
            confettiParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        if (_visualCoroutine != null) StopCoroutine(_visualCoroutine);
        _visualCoroutine = StartCoroutine(HideOverlay());

        if (_floatCoroutine != null)
        {
            StopCoroutine(_floatCoroutine);
            _floatCoroutine = null;
        }

        if (boostText != null)
            boostText.rectTransform.anchoredPosition = _boostTextBasePosition;
    }

    private IEnumerator ShowOverlay()
    {
        if (boostText != null)
        {
            boostText.text = $"x{_pole.CurrentBoostMultiplier:0.#}";
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

    /// <summary>Anime le texte multiplicateur avec un effet de flottement vertical.</summary>
    private IEnumerator FloatBoostText()
    {
        if (boostText == null) yield break;

        RectTransform rt = boostText.rectTransform;
        float timeOffset = Time.time;

        while (true)
        {
            float offsetY = Mathf.Sin((Time.time - timeOffset) * floatSpeed * Mathf.PI * 2f) * floatAmplitude;
            rt.anchoredPosition = _boostTextBasePosition + new Vector2(0f, offsetY);
            yield return null;
        }
    }
}
