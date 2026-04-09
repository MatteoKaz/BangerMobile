using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(Pole))]
public class BossEventOverlay : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private BossEventManager bossEventManager;
    [SerializeField] private Image highlightImage;
    [SerializeField] private TextMeshProUGUI malusText;
    [SerializeField] private ParticleSystem confettiParticles;
    [Header("Paramètres visuels")]
    [SerializeField] private float minAlpha = 0f;
    [SerializeField] private float maxAlpha = 0.35f;
    [SerializeField] private float pulseSpeed = 5f;

    [Header("Flottement")]
    [SerializeField] private float floatAmplitude = 6f;
    [SerializeField] private float floatSpeed = 1.5f;

    private Pole _pole;
    private Coroutine _visualCoroutine;
    private Coroutine _floatCoroutine;
    private Color _baseColor;
    private Vector2 _textBasePosition;

    // Couleurs highlight par pôle
    private static readonly Color ColorRed = new Color(1f, 0.2f, 0.2f);
    private static readonly Color ColorBlue = new Color(0.2f, 0.4f, 1f);
    private static readonly Color ColorGreen = new Color(0.2f, 1f, 0.4f);

    private void Awake()
    {
        _pole = GetComponent<Pole>();

        if (highlightImage != null)
            _baseColor = highlightImage.color;

        if (malusText != null)
            _textBasePosition = malusText.rectTransform.anchoredPosition;
    }

    private void OnEnable()
    {
        bossEventManager.BossEventStarted += OnEventStarted;
        bossEventManager.BossEventEnded += OnEventEnded;
    }

    private void OnDisable()
    {
        bossEventManager.BossEventStarted -= OnEventStarted;
        bossEventManager.BossEventEnded -= OnEventEnded;
    }

    private void OnEventStarted(Pole pole)
    {
        if (pole != _pole) return;

        // Couleur du highlight selon le type de pôle
        if (highlightImage != null)
        {
            Color c = pole.type switch
            {
                PoleType.RedPole => ColorRed,
                PoleType.BluePole => ColorBlue,
                PoleType.GreenPole => ColorGreen,
                _ => Color.white
            };
            _baseColor = c;
        }
        if (confettiParticles != null)
        {
            confettiParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            confettiParticles.Play();
        }
        if (_visualCoroutine != null) StopCoroutine(_visualCoroutine);
        _visualCoroutine = StartCoroutine(ShowOverlay());

        if (_floatCoroutine != null) StopCoroutine(_floatCoroutine);
        _floatCoroutine = StartCoroutine(FloatText());
    }

    private void OnEventEnded(Pole pole)
    {
        if (pole != _pole) return;

        if (_visualCoroutine != null) StopCoroutine(_visualCoroutine);
        _visualCoroutine = StartCoroutine(HideOverlay());
        if (confettiParticles != null)
            confettiParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (_floatCoroutine != null)
        {
            StopCoroutine(_floatCoroutine);
            _floatCoroutine = null;
        }

        if (malusText != null)
            malusText.rectTransform.anchoredPosition = _textBasePosition;
    }

    private IEnumerator ShowOverlay()
    {
        if (malusText != null)
        {
            // Affiche la valeur du malus en % ex : "-50%"
            //int malusPercent = Mathf.RoundToInt((1f - 0.5f) * 100f);
            malusText.text = $"x{0.3}";
            malusText.gameObject.SetActive(true);
        }

        while (true)
        {
            float t = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;

            if (highlightImage != null)
                highlightImage.color = new Color(
                    _baseColor.r, _baseColor.g, _baseColor.b,
                    Mathf.Lerp(minAlpha, maxAlpha, t));

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
                highlightImage.color = new Color(
                    _baseColor.r, _baseColor.g, _baseColor.b,
                    startAlpha * ratio);

            yield return null;
        }

        if (highlightImage != null)
            highlightImage.color = new Color(_baseColor.r, _baseColor.g, _baseColor.b, 0f);

        if (malusText != null)
            malusText.gameObject.SetActive(false);
    }

    private IEnumerator FloatText()
    {
        if (malusText == null) yield break;

        RectTransform rt = malusText.rectTransform;
        float timeOffset = Time.time;

        while (true)
        {
            float offsetY = Mathf.Sin((Time.time - timeOffset) * floatSpeed * Mathf.PI * 2f) * floatAmplitude;
            rt.anchoredPosition = _textBasePosition + new Vector2(0f, offsetY);
            yield return null;
        }
    }
}