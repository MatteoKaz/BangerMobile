using System;
using System.Collections;

using UnityEngine;

public class ScoreManager : MonoBehaviour
{
   public int playerMoney = 0;
   public int playerQuotat = 0;
   public int quotatOfTheDay = 0;
    public int WeekMedianQuotat = 0;
    [SerializeField] DayManager dayManager;
    public event Action LaunchScoreAnim;

    public void OnEnable()
    {
        dayManager.DayTransition += CalculateMoney;
    }
    public void CalculateMoney()
    {
       playerMoney += playerQuotat - quotatOfTheDay;
        StartCoroutine(AnimLauncher());
    }

    public IEnumerator AnimLauncher()
    {
        yield return new WaitForSeconds(1f);
            LaunchScoreAnim?.Invoke();

    }

    

   



}
