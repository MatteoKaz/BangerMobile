using UnityEngine;

public class PanelBool : MonoBehaviour
{
    public GameObject panel;
    [SerializeField] private AudioEventDispatcher audioEventDispatcher;
    public void Toggle()
    {
        audioEventDispatcher.PlayAudio(AudioType.Click);
        panel.SetActive(!panel.activeSelf);
    }
}
