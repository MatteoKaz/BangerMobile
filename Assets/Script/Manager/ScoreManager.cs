using UnityEngine;

public class ScoreManager : MonoBehaviour
{
   public int playerMoney = 0;
   public int playerQuotat = 0;
   public int quotatWave = 0;


    public void CalculateMoney()
    {
       playerMoney = playerQuotat - quotatWave; 

    }


}
