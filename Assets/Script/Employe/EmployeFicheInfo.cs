using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmployeFicheInfo : MonoBehaviour
{
    [SerializeField] Employe employe;

    [SerializeField] TextMeshProUGUI employeName;
    [SerializeField] TextMeshProUGUI employeDescription;
    [SerializeField] TextMeshProUGUI employeType;
    [SerializeField] TextMeshProUGUI employeRendement;
    [SerializeField] TextMeshProUGUI employeMoney;
    [SerializeField] TextMeshProUGUI tempsinEntreprise;
    [SerializeField] TextMeshProUGUI pole;
    [SerializeField] Image Image;

    [SerializeField] UiManager UiManager;

    private void OnEnable()
    {
        UiManager.ScoreAnim  += SetUpText;
    }

    private void OnDisable()
    {
        UiManager.ScoreAnim  -= SetUpText;
    }

    public void Start()
    {

       // SetUpText();
    }
    public void SetUpText()
    {
        StartCoroutine(TextSET());
    }
    public  IEnumerator TextSET()
    {
        yield return new WaitForSeconds(1f);
        if (employe != null)
        {
            employeName.text = employe.employeName;
            employeDescription.text = $"{employe.employeDescription}";
            employeType.text = $"Caractéristique : \n{employe.employeTypeText}";
            employeRendement.text = $"Papier: {employe.succeedPaper}/{employe.numberOfPaperDone}, 1/{employe.employeWorkRate}s";
            employeMoney.text = $"Apport: {employe.moneyMake} $ ";
            tempsinEntreprise.text = $"Temps : {employe.timeInEntreprise}";
            Image.sprite = employe.employeImage.sprite;
            pole.text = $"Pole: {employe.mypole.PoleName}";
            Debug.Log("Je set");
        }
    }
}
