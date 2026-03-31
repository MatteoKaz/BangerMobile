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
    public AudioType audioType;
    public AudioClip audioClip;
}

[CreateAssetMenu(fileName = "AudioEventDispatcher", menuName = "Scriptable Objects/AudioEventDispatcher")]
public class AudioEventDispatcher : ScriptableObject
{
    [SerializeField] private AudioInfos[] audioClips;

    public event Action<AudioClip> OnAudioEvent;
    public event Action<AudioClip> OnLoopAudioEvent;
    public event Action            OnStopLoopEvent;
    
    public void PlayAudio(AudioType audioType)
    {
        AudioClip clip = FindClip(audioType);
        if (clip != null)
            OnAudioEvent?.Invoke(clip);
    }
    
    public void PlayLoopAudio(AudioType audioType)
    {
        AudioClip clip = FindClip(audioType);
        if (clip != null)
            OnLoopAudioEvent?.Invoke(clip);
    }
    
    public void StopLoopAudio()
    {
        OnStopLoopEvent?.Invoke();
    }

    private AudioClip FindClip(AudioType audioType)
    {
        for (int i = 0; i < audioClips.Length; i++)
        {
            if (audioClips[i].audioType == audioType)
                return audioClips[i].audioClip;
        }
        return null;
    }
}