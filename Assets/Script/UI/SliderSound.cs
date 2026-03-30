using UnityEngine;
using UnityEngine.EventSystems;

public class SliderSound : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private AudioEventDispatcher audioEventDispatcher;
    [SerializeField] private AudioType            slideAudioType = AudioType.PenWritting;

    public void OnPointerDown(PointerEventData eventData)
    {
        audioEventDispatcher.PlayLoopAudio(slideAudioType);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        audioEventDispatcher.StopLoopAudio();
    }
}