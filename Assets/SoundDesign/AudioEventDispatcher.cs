using System;
using UnityEngine;

public enum AudioType
{
    None, 
    Destruction, 
    Death, 
    Hurt, 
    Wrong,
    Point, 
    Win, 
    Loose, 
    Jump, 
    Victory,
    Click, 
    Ambiance, 
    Boutique, 
    Swipe1, 
    Swipe2, 
    Swipe3, 
    TurnPageRight,
    TurnPageLeft, 
    Drag, 
    Drop, 
    SadEmployee, 
    HappyEmployee, 
    AngryEmployee,
    Tampon, 
    End, 
    Pop, 
    Pop2,
    PenWritting, 
    PenWritting2,
}
[System.Serializable]
public struct AudioInfos
{
    public AudioType   audioType;
    public AudioClip[] audioClips; // plusieurs variantes possibles
}

[CreateAssetMenu(fileName = "AudioEventDispatcher", menuName = "Scriptable Objects/AudioEventDispatcher")]
public class AudioEventDispatcher : ScriptableObject
{
    [SerializeField] private AudioInfos[] audioClips;

    public event Action<AudioClip>        OnAudioEvent;
    public event Action<AudioClip>        OnLoopAudioEvent;
    public event Action<AudioClip, float> OnExclusiveAudioEvent; // non-duplicable
    public event Action                   OnStopLoopEvent;

    /// <summary>Joue un son one-shot (interrompt le précédent).</summary>
    public void PlayAudio(AudioType audioType)
    {
        AudioClip clip = FindRandomClip(audioType);
        if (clip != null)
            OnAudioEvent?.Invoke(clip);
    }

    /// <summary>
    /// Joue un son seulement s'il n'est pas déjà en cours.
    /// Idéal pour les sons de spawn : pas de doublon, premier son prioritaire.
    /// </summary>
    public void PlayExclusiveAudio(AudioType audioType)
    {
        AudioClip clip = FindRandomClip(audioType);
        if (clip != null)
            OnExclusiveAudioEvent?.Invoke(clip, clip.length);
    }

    /// <summary>Démarre un son en boucle jusqu'à StopLoopAudio.</summary>
    public void PlayLoopAudio(AudioType audioType)
    {
        AudioClip clip = FindRandomClip(audioType);
        if (clip != null)
            OnLoopAudioEvent?.Invoke(clip);
    }

    /// <summary>Arrête le son en boucle en cours.</summary>
    public void StopLoopAudio()
    {
        OnStopLoopEvent?.Invoke();
    }

    private AudioClip FindRandomClip(AudioType audioType)
    {
        for (int i = 0; i < audioClips.Length; i++)
        {
            if (audioClips[i].audioType != audioType) continue;
            if (audioClips[i].audioClips == null || audioClips[i].audioClips.Length == 0) return null;

            int index = UnityEngine.Random.Range(0, audioClips[i].audioClips.Length);
            return audioClips[i].audioClips[index];
        }
        return null;
    }
}