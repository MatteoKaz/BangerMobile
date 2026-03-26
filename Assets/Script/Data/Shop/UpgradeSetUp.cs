
using System;

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

    [SerializeField] TextMeshProUGUI popUPPrice;
    [SerializeField] Image buttonBuy;
    [SerializeField] TextMeshProUGUI PopUPdescriptionPole;
    [SerializeField] Image PopUPiconPole;
    [SerializeField] Pole poleRef;
    RefOfItem currentRefOfItem;

    public event Action EmployeSet;

    public void Start()
    {

        for (int i = 0; i < shopUpgrade.allUpgrade.Count; i++)
        {
            GameObject item = Instantiate(itemToBuy, parentUpgrade.transform);
            RefOfItem refitem = item.GetComponent<RefOfItem>();
            refitem.upgradeSetUp = this;
            refitem.priceOfItem = shopUpgrade.allUpgrade[i].price;
            refitem.priceText.text = $"{shopUpgrade.allUpgrade[i].price}";
            refitem.numberText.text = $"{i+1}";
            refitem.type = shopUpgrade.allUpgrade[i].type;
            refitem.UpgradeName = shopUpgrade.allUpgrade[i].UpgradeName;
            refitem.upgradeValue = shopUpgrade.allUpgrade[i].upgradeValue;
            refitem.icone.sprite = shopUpgrade.allUpgrade[i].icone;
            refitem.iconeRef = shopUpgrade.allUpgrade[i].icone;
            refitem.inflation = shopUpgrade.allUpgrade[i].inflation;
            refitem.description = shopUpgrade.allUpgrade[i].Description;
            refitem.categoryUpgrade = shopUpgrade.allUpgrade[i].category;
        }
    }


    public void SetPopUp(RefOfItem roi)
    {

        Popup.SetActive(true);
        EmployeSet?.Invoke();
        Popupname.text = roi.UpgradeName;
        PopUPdescription.text = roi.description;
        PopUPicon.sprite = roi.iconeRef;
        popUPPrice.text = roi.priceText.text;
        currentRefOfItem = roi;
        buttonBuy.color = Color.grey;


    }
    public void ClosePopUp()
    {

        Popup.SetActive(false);
        currentRefOfItem = null;
        emp = null;
        buttonBuy.color = Color.grey;
    }

    public void chosenEmploye(EmployeLink employeLinkRef)
    {
        if (employeLinkRef!= null)
            emp = employeLinkRef.myemp;
        buttonBuy.color = Color.green;
    }


    public void chosenPole(Pole pole)
    {
        if (pole != null)
        poleRef = pole;
        buttonBuy.color = Color.green;
        
    }

    public void Buy()
    {
        if (currentRefOfItem == null)
        {
            buttonBuy.color = Color.grey;
            return;
        }
           
        if (emp == null)
        {
            buttonBuy.color = Color.grey;
            return;
        }
       

        RefOfItem roi = currentRefOfItem;
        if (roi.priceOfItem <=scoreManager.playerMoney)
        {
            scoreManager.playerMoney-=roi.priceOfItem;
            switch(roi.type)
            {
                case TypeOfUpgrade.BoostErrorRate:
                    emp.employeErrorPercenBonus += roi.upgradeValue; Debug.Log($"upgradeValue: {roi.upgradeValue}"); 
                    break;
                case TypeOfUpgrade.BoostSpeed:
                    emp.employeWorkRateBonus += roi.upgradeValue; break;
                case TypeOfUpgrade.BoostSurchargeResistance:
                    emp.StressBonus += roi.upgradeValue; break;
                //case TypeOfUpgrade.PrimePole:
                   // poleRef.
            }
        }
        else
        {
            Debug.Log("NoMoney");
        }
       
    }

}
