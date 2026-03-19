using System;
using System.Globalization;
using Unity.VisualScripting;
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

    
    public void OnEnable()
    {
        dayManager.DayBegin += QuotatPerDay;
    }
    void OnDisable()
    {
        dayManager.DayBegin -= QuotatPerDay;
    }

    public void QuotatCroissance()
    {

    }

    //Temporaire
    public void QuotatPerDay()
    {
        //TemporairePourTest
        DayQuotat = WeekQuotat;
        quotatEasy = Mathf.RoundToInt(DayQuotat * 0.75f);
        quotatMid = Mathf.RoundToInt(DayQuotat * 1f);
        quotatHard = Mathf.RoundToInt(DayQuotat * 1.25f);
        ChosenQuotat(quotatEasy);
       

    }

    public void ChosenQuotat(int quotatChosen)
    {
        DayQuotat = quotatChosen;
        int valueToSpawn = DayQuotat / 10;
        paperSpawner.totalPapers = Mathf.RoundToInt(multToBalance * valueToSpawn) ;

        QuotatChosen?.Invoke();
        Debug.Log("ddd");
    }

    
}
