using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] public List<Sprite> listAmelio;
    [SerializeField] Image[] popUPimagesgo;
    [SerializeField] GameObject[] AmelioParent;
    [SerializeField] RouletteWheel rouletteWheel;
    [SerializeField] private AudioEventDispatcher audioEventDispatcher;
    [SerializeField] GameObject couronne;
    private void OnEnable()
    {
        UiManager.ScoreAnim += SetUpText;
        rouletteWheel.EmployeSelected += SetUpText;
    }

    private void OnDisable()
    {
        UiManager.ScoreAnim -= SetUpText;
        rouletteWheel.EmployeSelected -= SetUpText;
    }

    public void SetUpText()
    {
        listAmelio = new List<Sprite>(employe.upgradesImages);
        StartCoroutine(TextSET());
    }

    public IEnumerator TextSET()
    {

        yield return new WaitForSeconds(1f);
        int missedPaper = employe.LoosePaper;
        int moneyLost = missedPaper * (employe.mypole.paperValue );
        if (employe != null)
        {
            int count = Mathf.Min(popUPimagesgo.Length, AmelioParent.Length);
            for (int i = 0; i < count; i++)
            {
                if (i < listAmelio.Count)
                {
                    AmelioParent[i].SetActive(true);
                    popUPimagesgo[i].enabled = true;
                    popUPimagesgo[i].sprite = listAmelio[i];

                    Sprite spr = listAmelio[i];
                    int upgradeCount = employe.upgradeCounts.ContainsKey(spr) ?
                                employe.upgradeCounts[spr] : 0;
                    var txt = AmelioParent[i].GetComponentInChildren<TextMeshProUGUI>(true);
                    if (txt != null) txt.text = upgradeCount >= 1 ? $"{upgradeCount}" : "";
                }
                else
                {
                    AmelioParent[i].SetActive(false);
                    popUPimagesgo[i].enabled = false;
                }
            }
            employeName.text        = employe.employeName;
            employeDescription.text = employe.employeDescription;
            employeType.text        = $"\n{employe.employeTypeText}";
            employeRendement.text   = $"Papier raté : {missedPaper} = {moneyLost}$";
            employeMoney.text       = $"Efficacité: 1/{employe.employeWorkRate}s";
            tempsinEntreprise.text  = $"Apport: {employe.moneyMake}$";
            Image.sprite            = employe.idleSprite;
            poleImage.sprite = employe.mypole.mySprite;
            typeimage.sprite = employe.typeImage.sprite;
            pole.text = $"Papier réussi :{employe.succeedPaper}/{employe.numberOfPaperDone}";
            couronne.SetActive(employe.couronne.activeSelf);

        }
    }
}