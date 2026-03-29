
using System;

using TMPro;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.UI;

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
    public EmployeLink empLink;
    public PoleLink poleLink;
    private bool open = false;



    public event Action EmployeSet;
    public event Action PoleSet;
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
            refitem.index = i;
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
            PoleSet?.Invoke();
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
        empLink = employeLinkRef;
        RefOfItem roi = currentRefOfItem;
        if (roi.priceOfItem <= scoreManager.playerMoney)
            buttonBuy.color = Color.green;
    }


    public void chosenPole(PoleLink poleLinkref)
    {

        if (poleLinkref != null)
        poleRef = poleLinkref.pole;
        poleLink = poleLinkref;
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
                shopUpgrade.allUpgrade[roi.index].price = roi.priceOfItem;
                popUPPrice.text = $"{roi.priceOfItem}$";
                if (!emp.upgradeCounts.ContainsKey(roi.iconeRef))
                    emp.upgradeCounts[roi.iconeRef] = 0;
                emp.upgradeCounts[roi.iconeRef]++;
                if (empLink != null)
                {
                    if (!emp.upgradesImages.Contains(roi.iconeRef))
                    {
                        emp.upgradesImages.Add(roi.iconeRef);
                        
                        empLink.upgradesImages.Add(roi.iconeRef);
                        empLink.SetIcone();
                    }
                }
                else
                {
                    
                }

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
                shopUpgrade.allUpgrade[roi.index].price = roi.priceOfItem;
                popUPPrice.text = $"{roi.priceOfItem}$";
                if (!poleRef.upgradeCounts.ContainsKey(roi.iconeRef))
                    poleRef.upgradeCounts[roi.iconeRef] = 0;
                poleRef.upgradeCounts[roi.iconeRef]++;
                if (poleLink != null)
                {
                    if (!poleRef.upgradesImages.Contains(roi.iconeRef))
                    {
                        poleRef.upgradesImages.Add(roi.iconeRef);

                        poleLink.upgradesImages.Add(roi.iconeRef);
                        poleLink.SetIcone();
                    }
                }
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


