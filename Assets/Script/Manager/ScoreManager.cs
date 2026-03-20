using Unity.VisualScripting;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
   public int playerMoney = 0;
   public int playerQuotat = 0;
   public int quotatOfTheDay = 0;
    public int WeekMedianQuotat = 0;
    [SerializeField] DayManager dayManager;

    public void OnEnable()
    {
        dayManager.DayTransition += CalculateMoney;
    }
    public void CalculateMoney()
    {
       playerMoney += playerQuotat - quotatOfTheDay; 

    }



}
