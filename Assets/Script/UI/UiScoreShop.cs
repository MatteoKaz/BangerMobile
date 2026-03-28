using TMPro;
using UnityEngine;

public class UiScoreShop : MonoBehaviour
{
   
    [SerializeField] TextMeshProUGUI Uimoney;
    [SerializeField] ScoreManager scoreManager;
   

    // Update is called once per frame
    void Update()
    {
        Uimoney.text = $"{ scoreManager.playerMoney}$";


    }
}
