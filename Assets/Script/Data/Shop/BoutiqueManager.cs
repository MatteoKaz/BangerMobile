using UnityEngine;

public class BoutiqueManager : MonoBehaviour
{
    [SerializeField] private GameObject menuBoutique;
    [SerializeField] private AudioEventDispatcher audioEventDispatcher;
    [SerializeField] private DayManager dayManager;

    private void OnEnable()  => dayManager.DayEnd += OnDayEnd;
    private void OnDisable() => dayManager.DayEnd -= OnDayEnd;

    /// <summary>Ouvre la boutique et lance la musique.</summary>
    public void OpenShop()
    {
        menuBoutique.SetActive(true);
        OpenShopMusic();
    }

    /// <summary>Lance uniquement la musique boutique (appelé aussi depuis ShopPanel.OnEnable).</summary>
    public void OpenShopMusic()
    {
        MusicManager.Instance?.StopIngame();
        MusicManager.Instance?.PlayShop();
    }


    public void Echap()
    {
        menuBoutique.SetActive(false);
        audioEventDispatcher?.PlayAudio(AudioType.ClosePopUp);
        MusicManager.Instance?.StopShop();
        MusicManager.Instance?.PlayIngame();
    }

    private void OnDayEnd()
    {
        MusicManager.Instance?.StopShop();
    }
}