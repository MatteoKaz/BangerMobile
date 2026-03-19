using System;
using System.Globalization;
using UnityEngine;

public class QuotatManager : MonoBehaviour
{
    public int WeekQuotat = 150;
    public int DayQuotat;
    [SerializeField] DayManager dayManager;
    public event Action QuotatChosen;
    public int quotatEasy;
    public int quotatMid;
    public int quotatHard;
    [SerializeField] PaperSpawner paperSpawner;
    public float multToBalance = 2f;
    public void QuotatCroissance()
    {

    }

    //Temporaire
    public void QuotatPerDay()
    {
        DayQuotat = WeekQuotat;
        quotatEasy *= Mathf.RoundToInt(DayQuotat * 0.75f);
        quotatMid *= Mathf.RoundToInt(DayQuotat * 1f);
        quotatHard *= Mathf.RoundToInt(DayQuotat * 1.25f);


    }

    public void ChosenQuotat(int quotatChosen)
    {
        DayQuotat = quotatChosen;
        paperSpawner.totalPapers = Mathf.RoundToInt(multToBalance *  (DayQuotat/10)) ;

        QuotatChosen?.Invoke();
    }

    
}
