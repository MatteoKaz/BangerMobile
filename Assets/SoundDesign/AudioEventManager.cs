using UnityEngine;

public class AudioEventManager : MonoBehaviour
{
    [SerializeField] private AudioEventDispatcher audioEventDispatcher;

    [Header("Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource loopSource;

    private void OnEnable()
    {
        audioEventDispatcher.OnAudioEvent     += PlayAudioFX;
        audioEventDispatcher.OnLoopAudioEvent += PlayLoopFX;
        audioEventDispatcher.OnStopLoopEvent  += StopLoopFX;
    }

    private void OnDisable()
    {
        audioEventDispatcher.OnAudioEvent     -= PlayAudioFX;
        audioEventDispatcher.OnLoopAudioEvent -= PlayLoopFX;
        audioEventDispatcher.OnStopLoopEvent  -= StopLoopFX;
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
}