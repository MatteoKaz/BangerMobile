using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;

public class Pole : MonoBehaviour
{

    [SerializeField] DayManager dayManager; 
    public int localQuotat;
    public int localAdvencement = 0;    
    [Header("PoleType")]
    public PoleType type;

    [Header("Employe")]
    public int maxEmploye = 4;
    public int currentEmploye;
    public float surcharge = 0f;
    public List<Employe> employeList = new List<Employe>();

    [Header("Papier")]
    public int totalPaper = 0;
    public int waitingPaper = 0;
    public int activepaper = 0;
    public int paperValue = 10;
    public int papertoadd = 5;

    [SerializeField] public GameObject contentparent;
    [SerializeField] Tuyaux myTuyaux;

    [SerializeField] public string PoleName;

    public event Action eventWinMoney;
    

    public void OnEnable()
    {
        myTuyaux.AddPaper += AddPaper;
        dayManager.ResetValueBeforeNextDay += ResetAllValue;
    }
    public void OnDisable()
    {
        myTuyaux.AddPaper -= AddPaper;
        dayManager.ResetValueBeforeNextDay -= ResetAllValue;
    }

    public void AddPaper()
    {
       
        totalPaper += papertoadd;
        
        waitingPaper = totalPaper - activepaper;
        waitingPaper = Mathf.Clamp(waitingPaper, 0, waitingPaper);

        LaunchWorker();
        


    }

    public void DecrementPaper()
    {
        totalPaper -= 1;
        totalPaper = Mathf.Clamp(totalPaper , 0, totalPaper);
        activepaper--;
        activepaper = Mathf.Clamp(activepaper, 0, activepaper);
        waitingPaper = totalPaper - activepaper;
        waitingPaper = Mathf.Clamp(waitingPaper, 0, waitingPaper);
        LaunchWorker() ;
        
       


    }

    public void WinMoney()
    {
        localAdvencement += paperValue;
        eventWinMoney?.Invoke();
    }

    public void UpdateUI()
    {
        eventWinMoney?.Invoke();
    }
    public void LaunchWorker()
    {
        foreach (Employe employe in employeList)
        {
            if (employe.iamWorking == false && waitingPaper >0 )
            {
               
                employe.Working();
                
                waitingPaper = totalPaper - activepaper;
                waitingPaper = Mathf.Clamp(waitingPaper, 0, waitingPaper);


            }
        }
    }


    public void ResetAllValue()
    {
        totalPaper = 0;
        waitingPaper = 0;
        activepaper = 0;
        localQuotat = 0; 
        localAdvencement = 0;
        foreach(Employe emp in  employeList)
        {
            emp.EndDayResetStat();
        }
        
    }


}
