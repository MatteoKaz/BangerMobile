using NUnit.Framework.Constraints;
using UnityEngine;

public class BoutiqueManager : MonoBehaviour
{
    [SerializeField] GameObject menuBoutique;
    [SerializeField] private AudioEventDispatcher audioEventDispatcher;
    public void Echap()
    {
        menuBoutique.SetActive(false);
        if (audioEventDispatcher == null)
            audioEventDispatcher?.PlayAudio(AudioType.Click);
    }
}
