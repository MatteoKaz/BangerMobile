using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RefOfItem : MonoBehaviour
{
    public UpgradeSetUp upgradeSetUp;
    [SerializeField] public TextMeshProUGUI priceText;
    [SerializeField] public TextMeshProUGUI numberText;
    [SerializeField] public Image icone;
    public int priceOfItem;
    public string UpgradeName;
    public TypeOfUpgrade type;
    public string upgradeValue;
    public Sprite iconeRef;
    public int inflation;
    public string description;



    public void OpenPopUP()
    {
      //Effet
      upgradeSetUp.SetPopUp(this);
    }


    public void Buy()
    {

    }
}



