using UnityEngine;

public class ScoreManager : MonoBehaviour
{
   public int playerMoney = 0;
   public int playerQuotat = 0;
   public int quotatOfTheDay = 0;
    public int WeekMedianQuotat = 0;
    [SerializeField] DayManager dayManager;

    public void CalculateMoney()
    {
       playerMoney += playerQuotat - quotatOfTheDay; 

    }

    public void SetQuotat()
    {
         
    }


}
