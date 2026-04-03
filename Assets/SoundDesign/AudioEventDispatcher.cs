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
    Levier,
    Bouton,
    Spin,
    SpinSlow,
    SpinEnd,
    BuyShop,
    OpenPopUp,
    ClosePopUp,
    ClickShopIcon,
    MouseClick,
    Walk,
    Fired,
    Talk,
    CannotBuy,
    FlyMouche,
    MoucheDead,
    Surcharge,
    Gain,
    Perte,
    Swat,
    Alert,
}
[System.Serializable]
public struct AudioInfos
{
    public AudioType   audioType;
    public AudioClip[] audioClips;
}

[CreateAssetMenu(fileName = "AudioEventDispatcher", menuName = "Scriptable Objects/AudioEventDispatcher")]
public class AudioEventDispatcher : ScriptableObject
{
    [SerializeField] private AudioInfos[] audioClips;

    public event Action<AudioClip>        OnAudioEvent;
    public event Action<AudioClip>        OnQueuedAudioEvent;   // attend la fin du son en cours
    public event Action<AudioClip>        OnLoopAudioEvent;
    public event Action<AudioClip, float> OnExclusiveAudioEvent;
    public event Action                   OnStopLoopEvent;

    /// <summary>Joue un son one-shot (interrompt le précédent).</summary>
    public void PlayAudio(AudioType audioType)
    {
        AudioClip clip = GetClip(audioType);
        if (clip != null)
            OnAudioEvent?.Invoke(clip);
    }

    /// <summary>
    /// Attend que le son en cours soit terminé avant de jouer le suivant.
    /// Idéal pour des sons qui doivent s'enchaîner sans se couper.
    /// </summary>
    public void PlayQueuedAudio(AudioType audioType)
    {
        AudioClip clip = FindRandomClip(audioType);
        if (clip != null)
            OnQueuedAudioEvent?.Invoke(clip);
    }

    /// <summary>
    /// Joue un son seulement s'il n'est pas déjà en cours.
    /// Le cooldown est géré dans AudioEventManager.
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

    public AudioClip FindRandomClip(AudioType audioType)
    {
        for (int i = 0; i < audioClips.Length; i++)
        {
            if (audioClips[i].audioType != audioType) continue;
            if (audioClips[i].audioClips == null || audioClips[i].audioClips.Length == 0) return null;

            return audioClips[i].audioClips[UnityEngine.Random.Range(0, audioClips[i].audioClips.Length)];
        }
        return null;
    }
    public AudioClip GetClip(AudioType audioType)
    {
        for (int i = 0; i < audioClips.Length; i++)
        {
            if (audioClips[i].audioType != audioType) continue;
            if (audioClips[i].audioClips == null || audioClips[i].audioClips.Length == 0) return null;
            return audioClips[i].audioClips[UnityEngine.Random.Range(0, audioClips[i].audioClips.Length)];
        }
        return null;
    }

}