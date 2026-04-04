using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EmployeFicheInfo : MonoBehaviour
{
    [SerializeField] public Employe employe;

    /// <summary>Expose l'employé lié pour les scripts externes (ex. MVPButton).</summary>
    public Employe LinkedEmploye => employe;

    [SerializeField] TextMeshProUGUI employeName;
    [SerializeField] TextMeshProUGUI employeDescription;
    [SerializeField] TextMeshProUGUI employeType;
    [SerializeField] TextMeshProUGUI employeRendement;
    [SerializeField] TextMeshProUGUI employeMoney;
    [SerializeField] TextMeshProUGUI tempsinEntreprise;
    [SerializeField] TextMeshProUGUI pole;
    [SerializeField] Image poleImage;
    [SerializeField] Image typeimage;
    [SerializeField] public Image Image;
    [SerializeField] EmployeLink employeLink;
    [SerializeField] UiManager UiManager;
    [SerializeField] Image[] listAmelio;
    [SerializeField] RouletteWheel rouletteWheel;
    [SerializeField] private AudioEventDispatcher audioEventDispatcher;

    private void OnEnable()
    {
        UiManager.ScoreAnim += SetUpText;
        rouletteWheel.EmployeSelected += SetUpText;
    }

    private void OnDisable()
    {
        UiManager.ScoreAnim -= SetUpText;
    }

    public void SetUpText()
    {
        StartCoroutine(TextSET());
    }

    public IEnumerator TextSET()
    {

        yield return new WaitForSeconds(1f);
        int missedPaper = Mathf.Abs(employe.succeedPaper - employe.numberOfPaperDone);
        int moneyLost = missedPaper * (employe.mypole.paperValue *Mathf.RoundToInt(employe.mypole.BonusRevenus));
        if (employe != null)
        {
            for (int i = 0; i < listAmelio.Length; i++)
            {
                listAmelio[i].enabled = employeLink.imagesUpgrades[i].enabled;
                listAmelio[i].sprite = employeLink.imagesUpgrades[i].sprite;
            }
            employeName.text        = employe.employeName;
            employeDescription.text = employe.employeDescription;
            employeType.text        = $"\n{employe.employeTypeText}";
            employeRendement.text   = $"Papier raté : {missedPaper} = {moneyLost}$";
            employeMoney.text       = $"Efficacité: 1/{employe.errorPercent + employe.employeErrorPercenBonus}s";
            tempsinEntreprise.text  = $"Apport: {employe.moneyMake}$";
            Image.sprite            = employe.idleSprite;
            poleImage.sprite = employe.mypole.mySprite;
            typeimage.sprite = employe.typeImage.sprite;
            pole.text = $"Papier réussi :{employe.succeedPaper}/{employe.numberOfPaperDone}";

        }
    }
}