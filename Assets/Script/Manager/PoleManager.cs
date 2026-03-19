using NUnit.Framework;
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
    [SerializeField] private ScoreManager scoreManager; 
    [SerializeField] QuotatManager quotatManager;

    private void OnEnable()
    {
        quotatManager.QuotatChosen += InitializedDay;
    }

    public void InitializedDay()
    {
        QuotaGiver(quotatManager.DayQuotat);
    }
    public void QuotaGiver(int quota)
    {
        for (int i = 0; i < poles.Length - 1; i++)
        {
            poles[i].localQuotat = quota / 3;

        }
    }
    


    public void InitializePole()
    {
        int i = 0;
        int numberOftimes = 0;    
        foreach (Employe employe in employes)
        {
            if (numberOftimes == 2 || numberOftimes == 4)
            {
                i++;

            }
            poles[i].employeList.Add(employe);
            employe.mypole = poles[i];
            employe.InitialSetIdentity();
            numberOftimes++;

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
