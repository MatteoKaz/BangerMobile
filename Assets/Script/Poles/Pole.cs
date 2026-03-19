using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Pole : MonoBehaviour
{
    public int localQuotat;
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
    }

    public void DecrementPaper()
    {
        totalPaper -= 1;
        waitingPaper = totalPaper - activepaper ;

    }

}
