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
    [SerializeField] public Image Image;

    [SerializeField] UiManager UiManager;
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
        if (employe != null)
        {
            employeName.text        = employe.employeName;
            employeDescription.text = employe.employeDescription;
            employeType.text        = $"Caractéristique : \n{employe.employeTypeText}";
            employeRendement.text   = $"Papier: {employe.succeedPaper}/{employe.numberOfPaperDone}, 1/{employe.employeWorkRate}s";
            employeMoney.text       = $"Apport: {employe.moneyMake} $";
            tempsinEntreprise.text  = $"Temps : {employe.timeInEntreprise}";
            Image.sprite            = employe.idleSprite;
            pole.text               = $"Pole: {employe.mypole.PoleName}";
        }
    }
}