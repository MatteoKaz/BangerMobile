
using System;

using TMPro;

using UnityEngine;
using UnityEngine.UI;
using static PoleLink;

public class UpgradeSetUp : MonoBehaviour
{
    [SerializeField] ShopUpgrade shopUpgrade;
    [SerializeField] GameObject itemToBuy;
    [SerializeField] GameObject parentUpgrade;
    [SerializeField] GameObject Popup;
    [SerializeField] ScrollRect myScrollRect;
    [Header("PopUp")]
    [SerializeField] TextMeshProUGUI Popupname;
    [SerializeField] TextMeshProUGUI PopUPdescription;
    [SerializeField] Image PopUPicon;
    [SerializeField] Employe emp;
    [SerializeField] ScoreManager scoreManager;
    [SerializeField] GameObject ScrollEmploye;
    [SerializeField] GameObject scrollPole;
    [SerializeField] SwatManager swatManager;
    [SerializeField] TextMeshProUGUI popUPPrice;
    [SerializeField] Image buttonBuy;
    [SerializeField] Sprite CanBuy;
    [SerializeField] Sprite CannotBuy;
    [SerializeField] TextMeshProUGUI PopUPdescriptionPole;
    [SerializeField] Image PopUPiconPole;
    [SerializeField] Pole poleRef;
    [SerializeField] GameObject swat;
    [SerializeField] BilletManager billetmanager;
    RefOfItem currentRefOfItem;
    public EmployeLink empLink;
    public PoleLink poleLink;
    private bool open = false;
    [SerializeField] public AudioEventDispatcher audioEventDispatcher;


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
            refitem.audioEventDispatcher = audioEventDispatcher; 
            refitem.durationInDays = shopUpgrade.allUpgrade[i].duration;
        }
        myScrollRect.verticalNormalizedPosition = 1f;

    }


    public void SetPopUp(RefOfItem roi)
    {
        
            Popup.SetActive(true);
            if (audioEventDispatcher != null)
                audioEventDispatcher.PlayAudio(AudioType.OpenPopUp);

            Popupname.text = roi.UpgradeName;
            PopUPdescription.text = roi.description;
            PopUPicon.sprite = roi.iconeRef;
            popUPPrice.text = roi.priceText.text;
            currentRefOfItem = roi;
             buttonBuy.sprite = CannotBuy;
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

        if (roi.categoryUpgrade == CategoryUpgrade.Usable)
        {
            if (roi.priceOfItem <= scoreManager.playerMoney)
                buttonBuy.sprite = CanBuy;
        }



    }
    public void ClosePopUp()
    {

        Popup.SetActive(false);
        if (audioEventDispatcher != null)
            audioEventDispatcher.PlayAudio(AudioType.ClosePopUp);
        open = false;
        currentRefOfItem.priceText.text = $"{currentRefOfItem.priceOfItem}$"; 
        currentRefOfItem = null;

        emp = null;
        buttonBuy.sprite = CannotBuy;
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
            buttonBuy.sprite = CanBuy;
        if (audioEventDispatcher != null)
            audioEventDispatcher.PlayAudio(AudioType.Pop);
    }


    public void chosenPole(PoleLink poleLinkref)
    {

        if (poleLinkref != null)
        poleRef = poleLinkref.pole;
        poleLink = poleLinkref;
        RefOfItem roi = currentRefOfItem;
        if (roi.priceOfItem <= scoreManager.playerMoney)
            buttonBuy.sprite = CanBuy;
        if (audioEventDispatcher != null)
            audioEventDispatcher.PlayAudio(AudioType.Pop);

    }

   public void Buy()
    {
    if (currentRefOfItem == null)
    {
        audioEventDispatcher?.PlayAudio(AudioType.CannotBuy);
        return;
    }

    RefOfItem roi = currentRefOfItem;
    
    if (roi.priceOfItem > scoreManager.playerMoney)
    {
        audioEventDispatcher?.PlayAudio(AudioType.CannotBuy);
            if (currentRefOfItem.priceOfItem > scoreManager.playerMoney)
                buttonBuy.sprite = CannotBuy;
        return;
    }
    
    switch (roi.categoryUpgrade)
    {
        case CategoryUpgrade.Usable:

            switch (roi.type)
            {
                case TypeOfUpgrade.Swat:
                        swat.SetActive(true);
                    swatManager.numberOfUtilisation += Mathf.RoundToInt(roi.upgradeValue);
                        
                        swatManager.OnBuyActivation();
                    break;
                case TypeOfUpgrade.Billet:
                        billetmanager.BilletAnim();
                        break;
            }

            break;

           case CategoryUpgrade.Employe:
            
            if (emp == null)
            {
                audioEventDispatcher?.PlayAudio(AudioType.CannotBuy);
                buttonBuy.sprite = CannotBuy;
                return;
            }

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

            switch (roi.type)
            {
                case TypeOfUpgrade.BoostErrorRate:
                    emp.employeErrorPercenBonus += roi.upgradeValue;
                        emp.employeErrorPercenBonus = Mathf.Min(emp.employeErrorPercenBonus, 0.25f);
                        break;
                case TypeOfUpgrade.BoostSpeed:
                    emp.employeWorkRateBonus += roi.upgradeValue;
                        emp.employeWorkRateBonus = Mathf.Min(emp.employeWorkRateBonus, 2.5f);
                        break;
                case TypeOfUpgrade.BoostSurchargeResistance:
                    emp.StressBonus += roi.upgradeValue;
                        emp.StressBonus = Mathf.Min(emp.StressBonus, 1.5f);
                    break;
                case TypeOfUpgrade.DoublePaperDone:
                    emp.BonusPaperDone_Shop = roi.upgradeValue;
                    break;
            }

            break;

            case CategoryUpgrade.Pole:
            
            if (poleRef == null)
            {
                audioEventDispatcher?.PlayAudio(AudioType.CannotBuy);
                buttonBuy.sprite = CannotBuy;
                return;
            }

            if (!poleRef.upgradeCounts.ContainsKey(roi.iconeRef))
                poleRef.upgradeCounts[roi.iconeRef] = 0;

            poleRef.upgradeCounts[roi.iconeRef]++;

            poleLink.timedUpgrades.Add(new TimedUpgrade
            {
                icon = roi.iconeRef,
                type = roi.type,
                value = roi.upgradeValue,
                daysRemaining = roi.durationInDays
            });

            poleLink.SetIcone();

            switch (roi.type)
            {
                case TypeOfUpgrade.BoostSpeedPole:
                    poleRef.BoostEmployeSpeed += roi.upgradeValue;
                    break;
                case TypeOfUpgrade.BoostErrorPole:
                    poleRef.BoostEmployeError += roi.upgradeValue;
                    break;
                case TypeOfUpgrade.PrimePole:
                    poleRef.BonusRevenus += roi.upgradeValue;
                    break;
                case TypeOfUpgrade.CigarettePole:
                    poleRef.BoostTimeForSurcharge += roi.upgradeValue;
                    break;
            }

            break;
    }
    
            scoreManager.playerMoney -= roi.priceOfItem;
             scoreManager.playerMoney = Mathf.Max(scoreManager.playerMoney, 0);

             roi.priceOfItem += roi.inflation;
            shopUpgrade.allUpgrade[roi.index].price = roi.priceOfItem;
            popUPPrice.text = $"{roi.priceOfItem}$";
    
            audioEventDispatcher?.PlayAudio(AudioType.BuyShop);
            if(currentRefOfItem.priceOfItem > scoreManager.playerMoney)
                {
            buttonBuy.sprite = CannotBuy;
            }
           
         }
    
    public void Update()
    {
        if(open)
        {
            if (currentRefOfItem.priceOfItem > scoreManager.playerMoney)
            {
                
                buttonBuy.sprite = CannotBuy;
                
            }
        }
       
        
    }
    public void ResetPrices()
    {
        RefOfItem[] items = parentUpgrade.GetComponentsInChildren<RefOfItem>();
        foreach (RefOfItem item in items)
        {
            item.priceOfItem = shopUpgrade.allUpgrade[item.index].price;
            item.priceText.text = $"{item.priceOfItem}$";
        }

    }

}


