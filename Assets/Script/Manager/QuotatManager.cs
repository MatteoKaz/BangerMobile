using System;
using System.Globalization;
using Unity.VisualScripting;
using UnityEngine;

public class QuotatManager : MonoBehaviour
{
    public int WeekQuotat = 500;
    public int DayQuotat ;
    [SerializeField] DayManager dayManager;
    public event Action QuotatChosen;
    public int quotatEasy;
    public int quotatMid;
    public int quotatHard;
    [SerializeField] PaperSpawner paperSpawner;
    public float multToBalance = 2f;

    public event Action QuotatIsSet;
    public event Action CalculQuotat;
    
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
        
        Debug.Log($"QuotatPerDay {DayQuotat}");
        CalculQuotat?.Invoke();
    }
    public void SelectQuotat(int difficultySelect)
    {
        switch (difficultySelect)
        {
            case 0:
                DayQuotat = quotatEasy;
                break;
            case 1: 
                DayQuotat = quotatMid;
                break;
           case 2:
                DayQuotat = quotatHard;
                break;
        }
        QuotatIsSet?.Invoke();
        ChosenQuotat(DayQuotat);
    }

    public void ChosenQuotat(int quotatChosen)
    {
        DayQuotat = quotatChosen;
        int valueToSpawn = DayQuotat / 10;
        paperSpawner.totalPapers = Mathf.RoundToInt(multToBalance * valueToSpawn) ;

        QuotatChosen?.Invoke();
        Debug.Log($"Quotat manager {DayQuotat}");
    }

    
}
