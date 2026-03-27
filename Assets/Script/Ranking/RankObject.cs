using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankObject : MonoBehaviour
{

    [SerializeField] public TextMeshProUGUI Name;
    [SerializeField] public TextMeshProUGUI Score;
    [SerializeField] public TextMeshProUGUI Money;
    [SerializeField] public Image icone;
    [SerializeField] public string NameToShow;
    [SerializeField] public int SucceedPaper;
    [SerializeField] public int TotalPaper;
    [SerializeField] public int TotalMoney;


    public void SetText()
    {
        Name.text = NameToShow;
        Score.text = $"Papier: {SucceedPaper}/{TotalPaper}";
        Money.text = $"{TotalMoney}$";
        
        
    }
}
