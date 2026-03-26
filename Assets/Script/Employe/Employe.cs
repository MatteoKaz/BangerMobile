
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Pole;

public class Employe : MonoBehaviour
{
    
    [SerializeField] public DataEmploye employeObject;
    [SerializeField] private PoleManager polemanager;
    [SerializeField] public Pole mypole;



    [Header("Identit�PersoValue")]
    public TypeOfEmployez employeType;
    public string employeTypeText;
    public string employeName;
    public string employeDescription;
    public string employeFireDescription;
    public float employeWorkRate;
    public float errorPercent;
    public int timeInEntreprise;
    public Image employeImage;
    public int employeIndex;
    

    [Header("Upgrade modifier")]
    public float employeWorkRateBonus;
    public float employeErrorPercenBonus;
    public float StressBonus;
    public float BonusPaperDone;

    [Header("Malus")]
    public float errorPercentMalus = 0f;
    public float employeWorkRateMalus = 0f;
    public bool isStunned =false ;
    [Header("Ui")]
    [SerializeField] Slider workAdvancement;

    [Header("MyStatDay")]
    public int numberOfPaperDone;
    public int succeedPaper;
    public int moneyMake;

    [Header("MyForWeek")]
    public int WeeknumberOfPaperDone;
    public int WeeksucceedPaper;
    public int WeekmoneyMake;


    public float timeBeetwennWork = 0.2f;

    public bool iamWorking = false;
    private Coroutine WorkRoutine;
    private Coroutine StunRef;
    [Header("Ui")]
    [SerializeField] TextMeshProUGUI Name;
    [SerializeField] Image image;

    public void InitialSetIdentity()
    {
        Debug.Log("EmployeSetUp");

        if (employeObject == null || employeObject.allEmploye == null || employeObject.allEmploye.Count == 0)
            return;

        if (polemanager == null || polemanager.TakenEmployeIndex == null)
            return;

        if (polemanager.TakenEmployeIndex.Count >= employeObject.allEmploye.Count)
        {
            Debug.LogWarning("Tous les employ�s ont d�j� �t� pris !");
            return;
        }

        do
        {
            employeIndex = Random.Range(0, employeObject.allEmploye.Count);
        }
        while (polemanager.TakenEmployeIndex.Contains(employeIndex));

        SetIdentity(employeIndex);
    }

    public void SetIdentity(int index)
    {
        if (employeObject == null || employeObject.allEmploye == null || index < 0 || index >= employeObject.allEmploye.Count)
            return;

        // Retire l'ancien si valide
        if (polemanager != null && polemanager.TakenEmployeIndex != null && employeIndex >= 0)
            polemanager.TakenEmployeIndex.Remove(employeIndex);

        employeIndex = index;

        var employe = employeObject.allEmploye[employeIndex];

        employeType = employe.type;
        employeName = employe.EmployeName;
        employeDescription = employe.description;
        employeFireDescription = employe.fireDescription;
        employeWorkRate = employe.workRythme;
        errorPercent = employe.errorPercent;
        timeInEntreprise = employe.timeInEntreprise;
        employeTypeText = employe.TypeText;
        employeImage.sprite = employe.icone;

        Name.text = employe.EmployeName;
        image.sprite = employeImage.sprite;
        // Ajoute le nouveau
        if (polemanager != null && polemanager.TakenEmployeIndex != null)
            polemanager.TakenEmployeIndex.Add(employeIndex);

        Debug.Log("SetEmploye");
    }


    // fonction � lancer lorsqu'il commence a work 
    public void Working()
    {
        if (mypole.waitingPaper > 0 && iamWorking == false)
        {
            iamWorking = true;
            mypole.activepaper++;
            mypole.UpdatePaperUI();
            WorkRoutine = StartCoroutine(Work());
            
        }
        
        else
        {
            iamWorking = false;
            
        }
    }

    public IEnumerator Work()
    {
        
        float t = 0f;
       
        while (t < 1)
        {
            if (isStunned == false) // 
            {
                float dt = Mathf.Min(Time.deltaTime, 0.05f); // max 50ms par frame
                float rate = Mathf.Max(0.01f, employeWorkRate + employeWorkRateMalus);
                t += dt / rate;
                workAdvancement.value = Mathf.Lerp(0, 1, t);
            }
            yield return null;

        }
        float Succeed = Random.Range(0f, 1f);
        if ((errorPercent - errorPercentMalus) > Succeed)
        {
            mypole.WinMoney();
            moneyMake += mypole.paperValue;
            succeedPaper += 1 ;

        }

       
        numberOfPaperDone += 1 ;
        workAdvancement.value = 0;
        Debug.Log("workDone");
        yield return new WaitForSeconds(timeBeetwennWork);
        iamWorking = false;
        mypole.DecrementPaper();
        
        


    }

    public void SwitchPole(Pole pole)
    {
        if (WorkRoutine != null)
        {
            StopCoroutine(WorkRoutine);
            WorkRoutine = null;
        }
        iamWorking = false;        
        workAdvancement.value = 0f;
        employeWorkRateMalus = 0f;
        errorPercentMalus = 0f;
        mypole = pole;
        Working();
    }


    public void Malus(TypeOfMalus malusType, float value)
    {
        switch (malusType)
        {
            case TypeOfMalus.WorkRate:
                employeWorkRateMalus = value;   // augmente le temps de travail
                break;
            case TypeOfMalus.ErrorPercent:
                errorPercentMalus = value;      // augmente les chances d'erreur
                break;
            case TypeOfMalus.Stun:
                
                if (StunRef != null) StopCoroutine(StunRef);
                StunRef = StartCoroutine(StunCoroutine(value));
                break;
                

        }

    }
    public void ResetMalus()
    {
        employeWorkRateMalus = 0f;
        errorPercentMalus = 0f;
    }
    public IEnumerator StunCoroutine(float duration)
    {
        isStunned = true;
        yield return new WaitForSeconds(duration);
        isStunned = false;
    }

    
    public void EndDayResetStat()
    {
        iamWorking = false;
        if (WorkRoutine != null)
        {
            StopCoroutine(WorkRoutine);
            WorkRoutine = null;
        } // ← accolade manquante ici !

        isStunned = false;
        employeWorkRateMalus = 0f;
        errorPercentMalus = 0f;
        timeInEntreprise += 1;
        workAdvancement.value = 0;
        WeekmoneyMake += moneyMake;
        WeeknumberOfPaperDone += numberOfPaperDone;
        WeeksucceedPaper += succeedPaper;
        succeedPaper = 0;
        moneyMake = 0;
        numberOfPaperDone = 0;
        if (StunRef != null)
        {
            StopCoroutine(StunRef);
            StunRef = null;
        }
        isStunned = false;
    }
    public void EndWeekReset()
    {
        WeekmoneyMake = 0;
        WeeknumberOfPaperDone = 0;
        WeeksucceedPaper = 0;
    }

}
