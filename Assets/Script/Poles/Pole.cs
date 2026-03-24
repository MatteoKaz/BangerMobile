using JetBrains.Annotations;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
   
    public List<Employe> employeList = new List<Employe>();

    [Header("Papier")]
    public int totalPaper = 0;
    public int waitingPaper = 0;
    public int activepaper = 0;
    public int paperValue = 10;
    public int papertoadd = 5;

    [Header("Surcharge")]
    public float surchargeValue = 0f;             // Valeur actuelle
    public float maxSurcharge = 100f;        // Surcharge maximale
    public float surchargeStep = 1f;         // Valeur ajoutée à chaque incrément
    public float baseDelay = 0.5f;           // Délai initial entre incréments
    public int[] surchargeThresholds = {0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 }; // Paliers pour déclencher malus
    private int nextThresholdIndex = 0;      // Pour savoir quel palier est le prochain
    public Coroutine SurchargeRef;      // Pour gérer la coroutine
    public float decayRate = 5f;
    [SerializeField] Slider surchargeProgress;
    public float sliderSpeed = 10f;



    
    public event Action UpdatePaperCount;

    public enum TypeOfMalus
    {
        WorkRate,
        ErrorPercent,
        Stun,
    }

    [SerializeField] public GameObject contentparent;
    [SerializeField] Tuyaux myTuyaux;

    [SerializeField] public string PoleName;

    public event Action eventWinMoney;

    public void Start()
    {
        UpdatePaperCount?.Invoke();
    }
    public void OnEnable()
    {
        myTuyaux.AddPaper += AddPaper;
        dayManager.ResetValueBeforeNextDay += ResetAllValue;
        dayManager.NewWeekReset += EndWeekReset;
    }
    public void OnDisable()
    {
        myTuyaux.AddPaper -= AddPaper;
        dayManager.ResetValueBeforeNextDay -= ResetAllValue;
        dayManager.NewWeekReset -= EndWeekReset;
    }
    public void EndWeekReset()
    {
        foreach(Employe emp in employeList)
        {
            emp.EndWeekReset();
        }
    }
    public void AddPaper()
    {
       
        totalPaper += papertoadd;
        
        waitingPaper = totalPaper - activepaper;
        waitingPaper = Mathf.Clamp(waitingPaper, 0, waitingPaper);

        LaunchWorker();
        BeginSurcharge();
        UpdatePaperCount?.Invoke();

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
        UpdatePaperCount?.Invoke();

        BeginSurcharge();



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
    public void UpdatePaperUI()
    {
        UpdatePaperCount?.Invoke();
    }
    public void LaunchWorker()
    {
   
        foreach (Employe employe in employeList)
        {
            if (employe.iamWorking == false && waitingPaper >0 )
            {
                
                employe.Working();
                waitingPaper = Mathf.Max(0, totalPaper - activepaper);
                BeginSurcharge();
                


            }
            
        }
        UpdatePaperCount?.Invoke();
    }


    public void ResetAllValue()
    {
        totalPaper = 0;
        waitingPaper = 0;
        activepaper = 0;
        localQuotat = 0; 
        localAdvencement = 0;
        UpdatePaperCount?.Invoke();
        ResetSurcharge();
        foreach (Employe emp in  employeList)
        {
            emp.EndDayResetStat();
        }
        
    }
    public void RebuildEmployeList()
    {
        employeList.Clear();

        List<InventorySlot> slots = new List<InventorySlot>();

        foreach (Transform child in contentparent.transform)
        {
            var slot = child.GetComponent<InventorySlot>();
            if (slot != null)
                slots.Add(slot);
        }

        // tri explicite
        slots.Sort((a, b) => a.slotIndex.CompareTo(b.slotIndex));

        foreach (var slot in slots)
        {
            var draggable = slot.GetComponentInChildren<DraggableItems>();
            if (draggable != null)
            {
                employeList.Add(draggable.linkedEmploye);
            }
        }
    }

    public void BeginSurcharge()
    {
        if (waitingPaper>0 && employeList.Count > 0)
        {
            Debug.Log("SurchargeBegin");
            if (SurchargeRef == null)
                SurchargeRef = StartCoroutine(Surcharge());
        }
        else if (SurchargeRef != null)
        {
            StopCoroutine(SurchargeRef);
            SurchargeRef = null;
        }
    }
    public IEnumerator Surcharge()
    {
        
        while (true)
        {
            if (waitingPaper > 0)
            {
                Debug.Log("Doing");
                // Augmentation progressive
                surchargeValue += surchargeStep;
                surchargeValue = Mathf.Min(surchargeValue, maxSurcharge);
                surchargeProgress.value = surchargeProgress.value = Mathf.Lerp(surchargeProgress.value, surchargeValue / maxSurcharge, 0.2f);
                // Vérifie les paliers
                if (nextThresholdIndex < surchargeThresholds.Length && surchargeValue >= surchargeThresholds[nextThresholdIndex])
                {
                    ApplyMalus();
                    nextThresholdIndex++;
                }

                // Délai dynamique selon charge
                float delay = baseDelay / (1f + (waitingPaper / Mathf.Max(1, totalPaper)));
                yield return new WaitForSeconds(delay);
            }
            else if (surchargeValue > 0)
            {
                // Décroissance progressive quand pas de papier
                surchargeValue -= decayRate * Time.deltaTime;
                surchargeValue = Mathf.Max(surchargeValue, 0f);

                // Réinitialise les paliers si on redescend sous un seuil
                while (nextThresholdIndex > 0 && surchargeValue < surchargeThresholds[nextThresholdIndex - 1])
                {
                    nextThresholdIndex--;
                    ApplyMalus();
                    yield return null;
                }
                surchargeProgress.value = surchargeProgress.value = Mathf.Lerp(surchargeProgress.value, surchargeValue / maxSurcharge, 0.2f);
                yield return null; // mise à jour chaque frame
            }
            else if (surchargeValue == 0)
            {
                ApplyMalus();
                yield return null;
            }
                
            else
            {
                yield return null; // rien à faire, attend la frame suivante
            }
           
            
        }
    }

    private void ApplyMalus()
    {
        
        foreach (Employe emp in employeList)
        {
            switch (nextThresholdIndex)
            {
                case 0: emp.ResetMalus(); break;
                case 1: emp.Malus(TypeOfMalus.WorkRate, 0.25f); break; // palier 10
                case 2: emp.Malus(TypeOfMalus.ErrorPercent, 0.5f); break; // palier 20
                case 3: emp.Malus(TypeOfMalus.WorkRate, 0.5f); break; // palier 30
                case 4: emp.Malus(TypeOfMalus.ErrorPercent, 1f); break; // palier 40
                case 5: emp.Malus(TypeOfMalus.WorkRate, 1f); break; // palier 50
                case 6: emp.Malus(TypeOfMalus.ErrorPercent, 5f); break; // palier 60
                case 7: emp.Malus(TypeOfMalus.WorkRate, 2f); break; // palier 70
                case 8: emp.Malus(TypeOfMalus.ErrorPercent, 10f); break; // palier 80
                case 9: emp.Malus(TypeOfMalus.WorkRate, 3f); break; // palier 90
                case 10: emp.Malus(TypeOfMalus.Stun, 5f);ResetSurcharge(); break; // palier 10

            }
           
        }
    }


    public void ResetSurcharge()
    {
        surchargeValue = 0f;
        nextThresholdIndex = 0;
        surchargeProgress.value = 0f;

        if (SurchargeRef != null)
        {
            StopCoroutine(SurchargeRef);
            SurchargeRef = null;
        }
    }
}
