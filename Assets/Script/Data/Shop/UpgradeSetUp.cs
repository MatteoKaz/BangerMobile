
using System;

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI.Table;

public class UpgradeSetUp : MonoBehaviour
{
    [SerializeField] ShopUpgrade shopUpgrade;
    [SerializeField] GameObject itemToBuy;
    [SerializeField] GameObject parentUpgrade;
    [SerializeField] GameObject Popup;

    [Header("PopUp")]
    [SerializeField] TextMeshProUGUI Popupname;
    [SerializeField] TextMeshProUGUI PopUPdescription;
    [SerializeField] Image PopUPicon;
    [SerializeField] Employe emp;
    [SerializeField] ScoreManager scoreManager;
    [SerializeField] GameObject ScrollEmploye;
    [SerializeField] GameObject scrollPole;

    [SerializeField] TextMeshProUGUI popUPPrice;
    [SerializeField] Image buttonBuy;
    [SerializeField] TextMeshProUGUI PopUPdescriptionPole;
    [SerializeField] Image PopUPiconPole;
    [SerializeField] Pole poleRef;
    RefOfItem currentRefOfItem;


    private bool open = false;



    public event Action EmployeSet;

    public void Start()
    {

        for (int i = 0; i < shopUpgrade.allUpgrade.Count; i++)
        {
            GameObject item = Instantiate(itemToBuy, parentUpgrade.transform);
            RefOfItem refitem = item.GetComponent<RefOfItem>();
            refitem.upgradeSetUp = this;
            refitem.priceOfItem = shopUpgrade.allUpgrade[i].price;
            refitem.priceText.text = $"{shopUpgrade.allUpgrade[i].price}$";
            refitem.numberText.text = $"{i+1}";
            refitem.type = shopUpgrade.allUpgrade[i].type;
            refitem.UpgradeName = shopUpgrade.allUpgrade[i].UpgradeName;
            refitem.upgradeValue = shopUpgrade.allUpgrade[i].upgradeValue;
            refitem.icone.sprite = shopUpgrade.allUpgrade[i].icone;
            refitem.iconeRef = shopUpgrade.allUpgrade[i].icone;
            refitem.inflation = shopUpgrade.allUpgrade[i].inflation;
            refitem.description = shopUpgrade.allUpgrade[i].Description;
            refitem.categoryUpgrade = shopUpgrade.allUpgrade[i].category;
            refitem.NameUI.text = shopUpgrade.allUpgrade[i].UpgradeName;
        }
    }


    public void SetPopUp(RefOfItem roi)
    {
        
            Popup.SetActive(true);
            
            Popupname.text = roi.UpgradeName;
            PopUPdescription.text = roi.description;
            PopUPicon.sprite = roi.iconeRef;
            popUPPrice.text = roi.priceText.text;
            currentRefOfItem = roi;
            buttonBuy.color = Color.grey;
            open = true;
        if (roi.categoryUpgrade == CategoryUpgrade.Employe)
        {
            
            ScrollEmploye.SetActive(true);
                scrollPole.SetActive(false);
            EmployeSet?.Invoke();
        }
        if (roi.categoryUpgrade == CategoryUpgrade.Pole)
        {
            
            scrollPole.SetActive(true);
            ScrollEmploye.SetActive(false);
        }




    }
    public void ClosePopUp()
    {

        Popup.SetActive(false);
        open = false;
        currentRefOfItem.priceText.text = $"{currentRefOfItem.priceOfItem}$"; 
        currentRefOfItem = null;

        emp = null;
        buttonBuy.color = Color.grey;
        scrollPole.SetActive(false);
        ScrollEmploye.SetActive(false);
        poleRef = null;

    }

    public void chosenEmploye(EmployeLink employeLinkRef)
    {
        if (employeLinkRef!= null)
            emp = employeLinkRef.myemp;
        RefOfItem roi = currentRefOfItem;
        if (roi.priceOfItem <= scoreManager.playerMoney)
            buttonBuy.color = Color.green;
    }


    public void chosenPole(Pole pole)
    {

        if (pole != null)
        poleRef = pole;
        RefOfItem roi = currentRefOfItem;
        if (roi.priceOfItem <= scoreManager.playerMoney)
            buttonBuy.color = Color.green;
        
    }

    public void Buy()
    {
        if (currentRefOfItem == null)
        {
            buttonBuy.color = Color.grey;
            return;
        }
           
       
       

        RefOfItem roi = currentRefOfItem;
        if (roi.categoryUpgrade == CategoryUpgrade.Employe)
        {
            if (roi.priceOfItem <= scoreManager.playerMoney)
            {
                if (emp == null)
                {
                    buttonBuy.color = Color.grey;
                    return;
                }
                scoreManager.playerMoney -= roi.priceOfItem;
                scoreManager.playerMoney = Mathf.Max(scoreManager.playerMoney, 0);
                roi.priceOfItem += roi.inflation;
              
                popUPPrice.text = $"{roi.priceOfItem}$";
                switch (roi.type)
                {
                    case TypeOfUpgrade.BoostErrorRate:
                        emp.employeErrorPercenBonus += roi.upgradeValue; Debug.Log($"upgradeValue: {roi.upgradeValue}");
                        break;
                    case TypeOfUpgrade.BoostSpeed:
                        emp.employeWorkRateBonus += roi.upgradeValue; break;
                    case TypeOfUpgrade.BoostSurchargeResistance:
                        emp.StressBonus += roi.upgradeValue; break;
                    case TypeOfUpgrade.DoublePaperDone:
                        emp.BonusPaperDone = roi.upgradeValue; break;
                    
                        
                }
               
            }
        }
        if (roi.categoryUpgrade == CategoryUpgrade.Pole)
        {
            if (roi.priceOfItem <= scoreManager.playerMoney)
            {

                if (poleRef == null)
                {
                    buttonBuy.color = Color.grey;
                    return;
                }
                scoreManager.playerMoney -= roi.priceOfItem;
                scoreManager.playerMoney = Mathf.Max(scoreManager.playerMoney, 0);
                roi.priceOfItem += roi.inflation;
                popUPPrice.text = $"{roi.priceOfItem}$";
                switch (roi.type)
                {
                    case TypeOfUpgrade.BoostSpeedPole:
                        poleRef.BoostEmployeSpeed += roi.upgradeValue; 
                        break;
                    case TypeOfUpgrade.BoostErrorPole:
                        poleRef.BoostEmployeError += roi.upgradeValue; break;
                    case TypeOfUpgrade.PrimePole:
                        poleRef.BonusRevenus += roi.upgradeValue; break;
                    case TypeOfUpgrade.CigarettePole:
                        poleRef.BoostTimeForSurcharge += roi.upgradeValue; break;

                }
               
                
            }
        }
        





    }
    public void Update()
    {
        if(open)
        {
            if (currentRefOfItem.priceOfItem > scoreManager.playerMoney)
            {
                
                buttonBuy.color = Color.grey;
                
            }
        }
       
        
    }

}


