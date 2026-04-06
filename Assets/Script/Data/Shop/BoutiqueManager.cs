
using UnityEngine;

public class BoutiqueManager : MonoBehaviour
{
    [SerializeField] GameObject menuBoutique;
    [SerializeField] private AudioEventDispatcher audioEventDispatcher;
    public void Echap()
    {
        menuBoutique.SetActive(false);
        audioEventDispatcher?.PlayAudio(AudioType.ClosePopUp);
    }
}
