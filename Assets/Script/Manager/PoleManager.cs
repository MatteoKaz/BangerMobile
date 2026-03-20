using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PoleManager : MonoBehaviour
{
    [SerializeField] Pole[] poles;
    public int quotaGlobal;
    public int EndGamePlayerQuotat;
    public List<int> TakenEmployeIndex;
    [SerializeField] List<Employe> employes;

    [Header("Ref")]
    [SerializeField] private ScoreManager scoreManager; 
    [SerializeField] QuotatManager quotatManager;
    [SerializeField] DayManager dayManager;

    

    private void OnEnable()
    {
        quotatManager.QuotatChosen += InitializedDay;
        dayManager.FirstDayInitialization += InitializePole;
        dayManager.DayEnd += QuotatAssemble;
    }

    public void InitializedDay()
    {
        
        QuotaGiver(quotatManager.DayQuotat);

        
    }
    public void QuotaGiver(int quota)
    {
        for (int i = 0; i < poles.Length; i++)
        {

            poles[i].localQuotat = Mathf.RoundToInt(quota / poles.Length);
            Debug.Log($"{ poles[i].localQuotat}");
            poles[i].UpdateUI();
           

        }
       
        
    }
    


    public void InitializePole()
    {
        Debug.Log("InitializePole");
        int i = 0;
        foreach (Employe employe in employes)
        {
            if ( i < poles.Length )
            {
                if (poles[i].employeList.Count < 2)
                {

                    poles[i].employeList.Add(employe);
                    employe.mypole = poles[i];
                    employe.InitialSetIdentity();
                    

                }
                else
                {
                    i++;
                    poles[i].employeList.Add(employe);
                    employe.mypole = poles[i];
                    employe.InitialSetIdentity();

                }
            }
            else
            {
                Debug.Log(i);
                return;
            }

        }
        

    }

    public void QuotatAssemble()
    {
        foreach(Pole pole in poles)
        {
            scoreManager.playerQuotat += pole.localAdvencement;
        }
    }
}
