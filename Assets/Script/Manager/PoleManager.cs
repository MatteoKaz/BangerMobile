
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
            // Boucle jusqu'à trouver un pole qui n'a pas 2 employés
            while (i < poles.Length && poles[i].employeList.Count >= 2)
                i++;

            if (i >= poles.Length)
            {
                Debug.Log("Plus de place pour les employés restants");
                break;
            }

            // Assignation
            poles[i].employeList.Add(employe);
            employe.mypole = poles[i];
            employe.InitialSetIdentity();
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
