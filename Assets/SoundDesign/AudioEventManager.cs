using UnityEngine;

public class AudioEventManager : MonoBehaviour
{
    [SerializeField] private AudioEventDispatcher audioEventDispatcher;

    [Header("Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource loopSource;
    [SerializeField] private AudioSource exclusiveSource; // pour les sons non-duplicables

    private float _exclusiveBusyUntil;

    private void OnEnable()
    {
        audioEventDispatcher.OnAudioEvent          += PlayAudioFX;
        audioEventDispatcher.OnLoopAudioEvent      += PlayLoopFX;
        audioEventDispatcher.OnStopLoopEvent       += StopLoopFX;
        audioEventDispatcher.OnExclusiveAudioEvent += PlayExclusiveFX;
    }

    private void OnDisable()
    {
        audioEventDispatcher.OnAudioEvent          -= PlayAudioFX;
        audioEventDispatcher.OnLoopAudioEvent      -= PlayLoopFX;
        audioEventDispatcher.OnStopLoopEvent       -= StopLoopFX;
        audioEventDispatcher.OnExclusiveAudioEvent -= PlayExclusiveFX;
    }

    private void PlayAudioFX(AudioClip clip)
    {
        sfxSource.Stop();
        sfxSource.clip = clip;
        sfxSource.Play();
    }

    private void PlayLoopFX(AudioClip clip)
    {
        loopSource.clip = clip;
        loopSource.loop = true;
        loopSource.Play();
    }

    private void StopLoopFX()
    {
        loopSource.Stop();
        loopSource.clip = null;
    }

    private void PlayExclusiveFX(AudioClip clip, float duration)
    {
        // Si un son est déjà en cours, on l'ignore
        if (Time.realtimeSinceStartup < _exclusiveBusyUntil) return;

        _exclusiveBusyUntil = Time.realtimeSinceStartup + duration;
        exclusiveSource.Stop();
        exclusiveSource.clip = clip;
        exclusiveSource.Play();
    }
}