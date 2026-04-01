
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;



public class Pole : MonoBehaviour
{

    [SerializeField] DayManager dayManager;
    [SerializeField] TimeManager timemanager;
    public int localQuotat;
    public int localAdvencement = 0;
    [Header("PoleType")]
    public PoleType type;
    public Sprite mySprite;

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
    public float surchargeStep = 5f;         // Valeur ajout�e � chaque incr�ment
    public float baseDelay = 0.01f;           // D�lai initial entre incr�ments
    public int[] surchargeThresholds = { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 }; // Paliers pour d�clencher malus
    public int nextThresholdIndex = 0;      // Pour savoir quel palier est le prochain
    public Coroutine SurchargeRef;      // Pour g�rer la coroutine
    public float decayRate = 7f;
    [SerializeField] Slider surchargeProgress;
    public float sliderSpeed = 10f;

    [Header("Bonus")]

    public float BonusRevenus = 1f;
    public float BoostEmployeSpeed;
    public float BoostEmployeError;
    public float BoostTimeForSurcharge;
    public List<Sprite> upgradesImages = new List<Sprite>();
    public Dictionary<Sprite, int> upgradeCounts = new Dictionary<Sprite, int>();

    public bool Stop;
    public event Action UpdatePaperCount;
    public event Action UpdatePaperFond;
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

        timemanager.TimerEnded += StopWorking;

    }
    public void OnDisable()
    {
        myTuyaux.AddPaper -= AddPaper;
        dayManager.ResetValueBeforeNextDay -= ResetAllValue;
        dayManager.NewWeekReset -= EndWeekReset;

        timemanager.TimerEnded -= StopWorking;
    }

    public void StopWorking()
    {
        ResetSurcharge();
        Stop = true;
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
        if (Stop == true)
            return;
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
        if (Stop == true)
            return;
        LaunchWorker() ;
        UpdatePaperCount?.Invoke();

        BeginSurcharge();



    }

    public void WinMoney()
    {
        localAdvencement += paperValue * Mathf.RoundToInt(BonusRevenus);
        
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
        Stop = false;
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
        StartCoroutine(RebuildAfterLoad());
       
    }
    IEnumerator RebuildAfterLoad()
    {
        yield return new WaitForEndOfFrame(); // attend que tout soit initialis�
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
            draggable = slot.GetComponentInChildren<DraggableItems>();
            if (draggable != null && draggable.linkedEmploye != null)
            {
                employeList.Add(draggable.linkedEmploye);

                yield return new WaitForSeconds(0.015f);
                var pile = draggable.linkedEmploye.GetComponentInChildren<PileBackEmploye>(true);
                if (pile != null)
                    pile.OnPoleChanged();
            }

        }
    }
    public void BeginSurcharge()
    {
        if (waitingPaper > 0 && employeList.Count > 0)
        {
            if (SurchargeRef == null)
            {
                TutorialManager.NotifyFirstOverload();
                SurchargeRef = StartCoroutine(Surcharge());
            }
        }
        else if (SurchargeRef != null && surchargeValue <= 0)
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
                
                // Augmentation progressive
                surchargeValue += surchargeStep /(1f + BoostTimeForSurcharge);
                surchargeValue = Mathf.Min(surchargeValue, maxSurcharge);
                Debug.LogWarning($"Surcharge {surchargeValue}");
                surchargeProgress.value = surchargeProgress.value = Mathf.Lerp(surchargeProgress.value, surchargeValue / maxSurcharge, 0.2f);
                
                if (nextThresholdIndex < surchargeThresholds.Length && surchargeValue >= surchargeThresholds[nextThresholdIndex])
                {
                    
                    ApplyMalus(nextThresholdIndex);
                    nextThresholdIndex++;
                    

                }

                // D�lai dynamique selon charge
                float delay = baseDelay / (1f + (waitingPaper / Mathf.Max(1, totalPaper)));
                yield return new WaitForSeconds(delay);
            }
            else if (surchargeValue > 0)
            {
                surchargeValue -= decayRate * Time.deltaTime;
                surchargeValue = Mathf.Max(surchargeValue, 0f);

                // Descend tous les paliers franchis d'un coup, sans yield entre
                while (nextThresholdIndex > 0 && surchargeValue < surchargeThresholds[nextThresholdIndex - 1])
                {
                    nextThresholdIndex--;
                    ApplyMalus(nextThresholdIndex);
                }

                surchargeProgress.value = Mathf.Lerp(surchargeProgress.value, surchargeValue / maxSurcharge, 0.2f);
                yield return null; // une seule fois par frame, apr�s avoir trait� tous les paliers
            }
            else // surchargeValue == 0 et waitingPaper == 0
            {
                ApplyMalus(-1);
                yield break; // on sort proprement
            }

        }
    }

    private void ApplyMalus(int i)
    {
        Debug.Log($"ApplyMalus appel� avec i={i} | nextThresholdIndex={nextThresholdIndex}");
        foreach (Employe emp in employeList)
        {
            switch (i)
            {
                case -1: emp.ResetMalus(); break;                          // reset explicite
                case 0: emp.Malus(TypeOfMalus.WorkRate, 0.25f); break;    // palier 10
                case 1: emp.Malus(TypeOfMalus.ErrorPercent, 0.05f); break; // palier 20
                case 2: emp.Malus(TypeOfMalus.WorkRate, 0.5f); break;     // palier 30
                case 3: emp.Malus(TypeOfMalus.ErrorPercent, 0.1f); break;   // palier 40
                case 4: emp.Malus(TypeOfMalus.WorkRate, 1f); break;       // palier 50
                case 5: emp.Malus(TypeOfMalus.ErrorPercent, 0.15f); break;   // palier 60
                case 6: emp.Malus(TypeOfMalus.WorkRate, 2f); break;       // palier 70
                case 7: emp.Malus(TypeOfMalus.ErrorPercent, 0.25f); break;  // palier 80
                case 8: emp.Malus(TypeOfMalus.WorkRate, 3f); break;       // palier 90
                case 9: emp.Malus(TypeOfMalus.Stun, 8f); break;           // palier 100
            }
        }
        if (i == 9) ResetSurcharge();
    }


    public void ResetSurcharge()
    {
        Debug.Log("ResetSurcharge");
        surchargeValue = 0f;
        nextThresholdIndex = 0;
        surchargeProgress.value = 0f;
        foreach (Employe emp in employeList)
        {
            emp.ResetMalus();
        }
        if (SurchargeRef != null)      
        {
            StopCoroutine(SurchargeRef);
            SurchargeRef = null;
        }

    }
}
