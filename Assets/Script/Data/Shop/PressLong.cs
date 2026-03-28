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
                openPopUp();
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
    public void ClosePopUp()
    {
        isOpen = false;
        
    }
}