using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioEventManager : MonoBehaviour
{
    [SerializeField] private AudioEventDispatcher audioEventDispatcher;

    [Header("Sources")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioSource loopSource;
    [SerializeField] private AudioSource exclusiveSource;

    [Header("Spawn cooldown (secondes entre deux sons de spawn)")]
    [SerializeField] private float exclusiveCooldown = 0.4f;

    private float               _exclusiveBusyUntil;
    private Queue<AudioClip>    _sfxQueue = new Queue<AudioClip>();
    private bool                _isProcessingQueue;

    private void OnEnable()
    {
        audioEventDispatcher.OnAudioEvent          += PlayAudioFX;
        audioEventDispatcher.OnQueuedAudioEvent    += EnqueueAudioFX;
        audioEventDispatcher.OnLoopAudioEvent      += PlayLoopFX;
        audioEventDispatcher.OnStopLoopEvent       += StopLoopFX;
        audioEventDispatcher.OnExclusiveAudioEvent += PlayExclusiveFX;
    }

    private void OnDisable()
    {
        audioEventDispatcher.OnAudioEvent          -= PlayAudioFX;
        audioEventDispatcher.OnQueuedAudioEvent    -= EnqueueAudioFX;
        audioEventDispatcher.OnLoopAudioEvent      -= PlayLoopFX;
        audioEventDispatcher.OnStopLoopEvent       -= StopLoopFX;
        audioEventDispatcher.OnExclusiveAudioEvent -= PlayExclusiveFX;
    }

    // ── One-shot (interrompt le précédent) ────────────────────────────────

    private void PlayAudioFX(AudioClip clip)
    {
        sfxSource.Stop();
        sfxSource.clip = clip;
        sfxSource.Play();
    }

    // ── Queued (attend la fin du son en cours) ────────────────────────────

    private void EnqueueAudioFX(AudioClip clip)
    {
        _sfxQueue.Enqueue(clip);
        if (!_isProcessingQueue)
            StartCoroutine(ProcessQueue());
    }

    private IEnumerator ProcessQueue()
    {
        _isProcessingQueue = true;

        while (_sfxQueue.Count > 0)
        {
            // Attend que la source soit libre
            yield return new WaitUntil(() => !sfxSource.isPlaying);

            if (_sfxQueue.Count == 0) break;

            AudioClip next = _sfxQueue.Dequeue();
            sfxSource.clip = next;
            sfxSource.Play();
        }

        _isProcessingQueue = false;
    }

    // ── Loop ──────────────────────────────────────────────────────────────

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

    // ── Exclusive (cooldown + non-duplicable) ─────────────────────────────

    private void PlayExclusiveFX(AudioClip clip, float duration)
    {
        float now     = Time.realtimeSinceStartup;
        float lockout = Mathf.Max(duration, exclusiveCooldown);

        if (now < _exclusiveBusyUntil) return;

        _exclusiveBusyUntil = now + lockout;
        exclusiveSource.Stop();
        exclusiveSource.clip = clip;
        exclusiveSource.Play();
    }
}
