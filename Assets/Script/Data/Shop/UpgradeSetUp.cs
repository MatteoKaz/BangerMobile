using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeSetUp : MonoBehaviour
{
    [SerializeField] ShopUpgrade shopUpgrade;
    [SerializeField] GameObject itemToBuy;
    [SerializeField] GameObject parentUpgrade;
    [SerializeField] GameObject Popup;
    [SerializeField] TextMeshProUGUI Popupname;
    [SerializeField] TextMeshProUGUI PopUPdescription;
    [SerializeField] Image PopUPicon;
    [SerializeField] Employe emp;
    [SerializeField] ScoreManager scoreManager;


    public void Start()
    {

        for (int i = 0; i < shopUpgrade.allUpgrade.Count; i++)
        {
            GameObject item = Instantiate(itemToBuy, parentUpgrade.transform);
            RefOfItem refitem = item.GetComponent<RefOfItem>();
            refitem.upgradeSetUp = this;
            refitem.priceOfItem = shopUpgrade.allUpgrade[i].price;
            refitem.priceText.text = $"{shopUpgrade.allUpgrade[i].price}";
            refitem.numberText.text = $"{i}";
            refitem.type = shopUpgrade.allUpgrade[i].type;
            refitem.UpgradeName = shopUpgrade.allUpgrade[i].UpgradeName;
            refitem.upgradeValue = shopUpgrade.allUpgrade[i].upgradeValue;
            refitem.icone.sprite = shopUpgrade.allUpgrade[i].icone;
            refitem.iconeRef = shopUpgrade.allUpgrade[i].icone;
            refitem.inflation = shopUpgrade.allUpgrade[i].inflation;
            refitem.description = shopUpgrade.allUpgrade[i].Description;
        }
    }


    public void SetPopUp(RefOfItem roi)
    {
        Popup.SetActive(true);
        Popupname.text = roi.UpgradeName;
        PopUPdescription.text = roi.description;
        PopUPicon.sprite = roi.iconeRef;
        

    }


    public void chosenEmploye(EmployeLink employeLinkRef)
    {
        emp = employeLinkRef.myemp;
    }


    public void Buy(RefOfItem roi)
    {
        if(roi.priceOfItem <=scoreManager.playerMoney)
        {
            scoreManager.playerMoney-=roi.priceOfItem;
            
        }
    }

}
