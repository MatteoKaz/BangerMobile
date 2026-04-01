using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RefOfItem : MonoBehaviour
{
    public UpgradeSetUp upgradeSetUp;
    [SerializeField] public TextMeshProUGUI priceText;
    [SerializeField] public TextMeshProUGUI numberText;
    [SerializeField] public TextMeshProUGUI NameUI;
    [SerializeField] public Image icone;
    public int index;
    public int priceOfItem;
    public string UpgradeName;
    public TypeOfUpgrade type;
    public float upgradeValue;
    public Sprite iconeRef;
    public int inflation;
    public string description;
    public CategoryUpgrade categoryUpgrade;
    public string nameEncadre;
    public int durationInDays;

    [SerializeField] public AudioEventDispatcher audioEventDispatcher;

    public void OpenPopUP()
    {
      //Effet
      if (audioEventDispatcher != null)
          audioEventDispatcher.PlayAudio(AudioType.ClickShopIcon);
      upgradeSetUp.SetPopUp(this);
    }
   

    public void Buy()
    {

    }
}



