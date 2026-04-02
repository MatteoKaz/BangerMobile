
using System.Collections.Generic;

using UnityEngine;

public class PoleManager : MonoBehaviour
{
    [SerializeField] public Pole[] poles;
    public int quotaGlobal;
    public int EndGamePlayerQuotat;
    public List<int> TakenEmployeIndex;
    [SerializeField] List<Employe> employes;

    [Header("Ref")]
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] QuotatManager quotatManager;
    [SerializeField] DayManager dayManager;

    private void Awake()
    {
        quotatManager.QuotatChosen += InitializedDay;
        dayManager.FirstDayInitialization += InitializePole;
        dayManager.DayEnd += QuotatAssemble;
    }

    private void OnDisable()
    {
        quotatManager.QuotatChosen -= InitializedDay;
        dayManager.FirstDayInitialization -= InitializePole;
        dayManager.DayEnd -= QuotatAssemble;
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
            if (employe.mypole != null)
            {
                employe.mypole.employeList.Add(employe);
                employe.InitialSetIdentity();
            }
            else
            {
                Debug.LogWarning($"Employé {employe.name} n'a pas de pôle assigné dans la map !");
            }
        }
    }

    public void QuotatAssemble()
    {
        foreach (Pole pole in poles)
        {
            scoreManager.playerQuotat += pole.localAdvencement;
        }
    }
}
