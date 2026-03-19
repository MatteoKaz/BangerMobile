using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;

public class Pole : MonoBehaviour
{
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

    [SerializeField] public GameObject contentparent;
    [SerializeField] Tuyaux myTuyaux;


    public void OnEnable()
    {
        myTuyaux.AddPaper += AddPaper;
    }
    public void OnDisable()
    {
        myTuyaux.AddPaper -= AddPaper;  
    }

    public void AddPaper()
    {
       
        totalPaper += 1;
        LaunchWorker();
        
        waitingPaper = totalPaper - activepaper;
       
        
    }

    public void DecrementPaper()
    {

        totalPaper = Mathf.Clamp(totalPaper -1, 0, totalPaper);
        localAdvencement += paperValue;
        waitingPaper = totalPaper - activepaper ;
        LaunchWorker();


    }


    public void LaunchWorker()
    {
        foreach (Employe employe in employeList)
        {
            if (employe.iamWorking == false)
            {
                employe.Working();
            }
        }
    }


    public void ResetAllValue()
    {
        totalPaper = 0;
        waitingPaper = 0;
        activepaper = 0;
        localQuotat = 0;    
    }


}
