using TMPro;
using UnityEngine;


using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HoldButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private bool isHeld = false;
    private float timeHeld = 0.3f;
    private float timer = 0f;
    private bool isOpen = false;

    [SerializeField] EmployeLink employeLink;
    [SerializeField] PoleLink poleLink;
    [SerializeField] GameObject popup;
    [SerializeField] public Image myTypeSprite;
    [SerializeField] public Image employeImagepopUp;
    [SerializeField] public TextMeshProUGUI popupNameUi;
    [SerializeField] public TextMeshProUGUI popupApportMoney;
    [SerializeField] public TextMeshProUGUI PaperDone;
    [SerializeField] public TextMeshProUGUI TimeInEntreprise;
    [SerializeField] Image[] popUPimagesUpgrades;
    [SerializeField] GameObject[] popUPimagesgo;


    [SerializeField] ClosePopUp closePopUp;
    [SerializeField] Type typePress = Type.Employe;
    public enum Type
    {
        Employe,
        Pole,
    }

    private void OnEnable()
    {
        closePopUp.ClosePopUpCallback += ClosePopUp;
    }

    private void OnDisable()
    {
        closePopUp.ClosePopUpCallback -= ClosePopUp;
    }
    public void OnPointerDown(PointerEventData eventData)
    {
        isHeld = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isHeld = false;
        timer = 0f;
    }

    void Update()
    {
        if (isHeld)
        {
            
            timer += Time.deltaTime;
            if (timer > timeHeld)
            {
                if (typePress == Type.Employe)
                openPopUp();
                if (typePress == Type.Pole)
                    openPopUpPole();
            }
        }
    }

    public void openPopUp()
    {
       
        if (!isOpen)
        {
            isOpen = true;
            popup.SetActive(true);
            myTypeSprite.sprite = employeLink.myTypeSprite.sprite;
            popupNameUi.text = employeLink.MyName;
            popupApportMoney.text = $"Apport: {employeLink.myemp.WeekmoneyMake}";
            PaperDone.text = $"Papier: {employeLink.myemp.WeeksucceedPaper}/{employeLink.myemp.WeeknumberOfPaperDone}";
            TimeInEntreprise.text = $"Temps: {employeLink.myemp.timeInEntreprise}jours";
            employeImagepopUp.sprite = employeLink.myCase.sprite;

            for (int i = 0; i < popUPimagesUpgrades.Length; i++)
            {
                bool active = employeLink.imagesUpgrades[i].enabled;
                popUPimagesgo[i].SetActive(active);
                popUPimagesUpgrades[i].sprite = employeLink.imagesUpgrades[i].sprite;

                if (active)
                {
                    Sprite spr = employeLink.imagesUpgrades[i].sprite;
                    int count = employeLink.myemp.upgradeCounts.ContainsKey(spr) ?
                                employeLink.myemp.upgradeCounts[spr] : 0;
                    var txt = popUPimagesgo[i].GetComponentInChildren<TextMeshProUGUI>(true);
                    if (txt != null) txt.text = count >= 1 ? $"{count}" : "";
                }
            }




        }
    }

    public void openPopUpPole()
    {

        if (!isOpen)
        {
            isOpen = true;
            popup.SetActive(true);

            employeImagepopUp.sprite = poleLink.pole.mySprite;
  

            for (int i = 0; i < popUPimagesUpgrades.Length; i++)
            {
                bool active = poleLink.imagesUpgrades[i].enabled;
                popUPimagesgo[i].SetActive(active);
                popUPimagesUpgrades[i].sprite = poleLink.imagesUpgrades[i].sprite;

                if (active)
                {
                    Sprite spr = poleLink.imagesUpgrades[i].sprite;

                    // count
                    int count = poleLink.pole.upgradeCounts.ContainsKey(spr) ?
                                poleLink.pole.upgradeCounts[spr] : 0;

                    // plus petit daysRemaining parmi tous les timedUpgrades avec ce sprite
                    int minDays = int.MaxValue;
                    foreach (var t in poleLink.timedUpgrades)
                    {
                        if (t.icon == spr && t.daysRemaining < minDays)
                            minDays = t.daysRemaining;
                    }
                    if (minDays == int.MaxValue) minDays = 0;

                    var texts = popUPimagesgo[i].GetComponentsInChildren<TextMeshProUGUI>(true);
                    if (texts.Length >= 1) texts[0].text = count >= 1 ? $"x{count}" : "";
                    if (texts.Length >= 2) texts[1].text = minDays > 0 ? $"Temps restant: {minDays}j" : "";
                }
            }




        }
    }
    public void ClosePopUp()
    {
        isOpen = false;
        
    }
}