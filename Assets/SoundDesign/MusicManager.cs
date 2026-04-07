using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gère toutes les musiques de fond avec fade in/out indépendants par source.
/// Singleton persistant entre les scènes.
/// </summary>
public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("Sources audio (une par couche)")]
    [SerializeField] private AudioSource musicMenu;
    [SerializeField] private AudioSource musicIngame;
    [SerializeField] private AudioSource musicShop;
    [SerializeField] private AudioSource musicFire;

    [Header("Clips")]
    [SerializeField] private AudioClip clipMenu;
    [SerializeField] private AudioClip clipIngame;
    [SerializeField] private AudioClip clipShop;
    [SerializeField] private AudioClip clipFire;

    [Header("Volumes cibles")]
    [SerializeField] [Range(0f, 1f)] private float volumeMenu   = 0.6f;
    [SerializeField] [Range(0f, 1f)] private float volumeIngame = 0.6f;
    [SerializeField] [Range(0f, 1f)] private float volumeShop   = 0.5f;
    [SerializeField] [Range(0f, 1f)] private float volumeFire   = 0.7f;

    [Header("Durée des fades (secondes)")]
    [SerializeField] private float fadeDuration = 1.5f;

    // Coroutines actives par source pour ne pas s'interrompre mutuellement.
    private readonly Dictionary<AudioSource, Coroutine> _activeFades
        = new Dictionary<AudioSource, Coroutine>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ── API publique ──────────────────────────────────────────────────────────

    /// <summary>Démarre la musique du menu avec fade in.</summary>
    public void PlayMenu()   => StartMusic(musicMenu,   clipMenu,   volumeMenu);

    /// <summary>Démarre la musique in-game avec fade in.</summary>
    public void PlayIngame() => StartMusic(musicIngame, clipIngame, volumeIngame);

    /// <summary>Démarre la musique de la boutique avec fade in.</summary>
    public void PlayShop()   => StartMusic(musicShop,   clipShop,   volumeShop);

    /// <summary>Démarre la musique de licenciement avec fade in.</summary>
    public void PlayFire()   => StartMusic(musicFire,   clipFire,   volumeFire);

    /// <summary>Fade out la musique du menu.</summary>
    public void StopMenu()   => FadeOut(musicMenu);

    /// <summary>Fade out la musique in-game.</summary>
    public void StopIngame() => FadeOut(musicIngame);

    /// <summary>Fade out la musique de la boutique.</summary>
    public void StopShop()   => FadeOut(musicShop);

    /// <summary>Fade out la musique de licenciement.</summary>
    public void StopFire()   => FadeOut(musicFire);

    /// <summary>Fade out toutes les musiques en cours.</summary>
    public void StopAll()
    {
        FadeOut(musicMenu);
        FadeOut(musicIngame);
        FadeOut(musicShop);
        FadeOut(musicFire);
    }

    // ── Interne ───────────────────────────────────────────────────────────────

    private void StartMusic(AudioSource source, AudioClip clip, float targetVolume)
    {
        if (source == null)
        {
            Debug.LogWarning($"MusicManager : AudioSource non assignée pour le clip {clip?.name}.", this);
            return;
        }
        if (clip == null)
        {
            Debug.LogWarning("MusicManager : clip non assigné.", this);
            return;
        }

        CancelFade(source);
        source.clip   = clip;
        source.loop   = true;
        source.volume = 0f;
        source.Play();
        _activeFades[source] = StartCoroutine(FadeInCoroutine(source, targetVolume));
    }

    private void FadeOut(AudioSource source)
    {
        if (source == null || !source.isPlaying) return;
        CancelFade(source);
        _activeFades[source] = StartCoroutine(FadeOutCoroutine(source));
    }

    private void CancelFade(AudioSource source)
    {
        if (_activeFades.TryGetValue(source, out Coroutine running) && running != null)
        {
            StopCoroutine(running);
            _activeFades.Remove(source);
        }
    }

    private IEnumerator FadeInCoroutine(AudioSource source, float targetVolume)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed      += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(0f, targetVolume, elapsed / fadeDuration);
            yield return null;
        }
        source.volume = targetVolume;
        _activeFades.Remove(source);
    }

    private IEnumerator FadeOutCoroutine(AudioSource source)
    {
        float elapsed = 0f;
        float start   = source.volume;

        while (elapsed < fadeDuration)
        {
            elapsed      += Time.unscaledDeltaTime;
            source.volume = Mathf.Lerp(start, 0f, elapsed / fadeDuration);
            yield return null;
        }
        source.Stop();
        source.volume = 0f;
        _activeFades.Remove(source);
    }
}
