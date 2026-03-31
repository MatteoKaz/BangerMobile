
using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using static Pole;


public class Employe : MonoBehaviour
{
    
    [SerializeField] public DataEmploye employeObject;
    [SerializeField] private PoleManager polemanager;
    [SerializeField] public Pole mypole;
    [SerializeField] public TimeManager timemanager;


    [Header("IdentitePersoValue")]
    public TypeOfEmployez employeType;
    public string employeTypeText;
    public string employeName;
    public string employeDescription;
   
    public float employeWorkRate;
    public float errorPercent;
    public int timeInEntreprise;
    public Image employeImage;
    public int employeIndex;
    public Image typeImage;
    public Sprite idleSprite;
    public Sprite working;
    public Sprite Surcharge;
    public List<DialogueLine> firelines = new List<DialogueLine>();
    public List<DialogueLine> firelinesChoice = new List<DialogueLine>();
    public List<DialogueLine> notfirelines = new List<DialogueLine>();



    [Header("Upgrade modifier")]
    public float employeWorkRateBonus = 0f;
    public float employeErrorPercenBonus = 0f;
    public float StressBonus = 0f;
    public float BonusPaperDone = 0f;

    public List<Sprite> upgradesImages = new List<Sprite>();
    public Dictionary<Sprite, int> upgradeCounts = new Dictionary<Sprite, int>();

    [Header("Malus")]
    public float errorPercentMalus = 0f;
    public float employeWorkRateMalus = 0f;
    public float RankingWorkRateMalus = 0f;
    public float RankingPercentMalus = 0f;
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

    [Header("Feedback")]
    [SerializeField] Image fond;
    [SerializeField] Light2D Light;
    public Color baseColor;
    private float timeBeetwennWork = 4f;
    [SerializeField] Animator animator;

    public bool iamWorking = false;
    private Coroutine WorkRoutine;
    private Coroutine StunRef;
    [Header("Ui")]
    [SerializeField] TextMeshProUGUI Name;
    [SerializeField] Image image;
    private bool  wasStunned =false;

    [Header("RandomEvent")]
    public bool occupied = false;
    public bool SwatGoing = false; 
    public bool HasBeenSwat= false;
    [Header("Swat")]
    public GameObject GrilleSwat;
    public float swatBoostSpeed = 0;
    public float swatBoostError = 0;
    public float swatBoostTimeBeetweenWork = 0; 
    public void OnEnable()
    {
        timemanager.TimerEnded += StopWorking;
    }

    public void OnDisable()
    {
        timemanager.TimerEnded -= StopWorking;
    }











    public void StopWorking()
    {
        if (WorkRoutine != null)
        {
            StopCoroutine(WorkRoutine);
            WorkRoutine = null;
        }

    }
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
        firelines = employe.firelines;
        employeWorkRate = employe.workRythme;
        errorPercent = employe.errorPercent;
        timeInEntreprise = employe.timeInEntreprise;
        employeTypeText = employe.TypeText;
        employeImage.sprite = employe.icone;
        typeImage.sprite = employe.typeSprite;
        Name.text = employe.EmployeName;
        image.sprite = employeImage.sprite;
        idleSprite = employeImage.sprite;
        working = employe.Working;
        Surcharge = employe.Surcharge;
        firelinesChoice = employe.firelinesChoice;
         notfirelines = employe.notfirelines;
        // Ajoute le nouveau
        if (polemanager != null && polemanager.TakenEmployeIndex != null)
            polemanager.TakenEmployeIndex.Add(employeIndex);

        Debug.Log("SetEmploye");
    }


    // fonction  lancer lorsqu'il commence a work 
    public void Working()
    {
        if (mypole.waitingPaper > 0 && iamWorking == false)
        {
            iamWorking = true;
            mypole.activepaper++;
            mypole.UpdatePaperUI();
            WorkRoutine = StartCoroutine(Work());
            
            Light.intensity = 0.3f;
            
        }
        
        else
        {
            iamWorking = false;

            Light.intensity = 0.0f;
            if (isStunned == false)
            {
                animator.SetTrigger("Idle");
                employeImage.sprite = idleSprite;
            }
           
        }
    }

    public IEnumerator Work()
    {
       
        float t = 0f;
        wasStunned = true;
        while (t < 1)
        {
            while (occupied)
            {
                if (StunRef != null)
                {
                    StopCoroutine(StunRef);
                    StunRef = null;
                    isStunned = false;
                }
                yield return null;
            }
            if (isStunned == false)
                {
                    if (wasStunned == true) // changement d'état  on trigger une seule fois
                    {
                        Light.color = baseColor;
                        employeImage.sprite = working;
                        animator.SetTrigger("Working");
                        wasStunned = false;
                    }

                    float dt = Mathf.Min(Time.deltaTime, 0.05f);
                    float rate = employeWorkRate
                               - mypole.BoostEmployeSpeed
                               - employeWorkRateBonus
                               -swatBoostSpeed  
                               + (employeWorkRateMalus + RankingWorkRateMalus);
                    t += dt / rate;
                    workAdvancement.value = Mathf.Lerp(0, 1, t);
                }
                else
                {
                    if (wasStunned == false) // changement d'état  on trigger une seule fois
                    {
                        employeImage.sprite = Surcharge;
                        Light.intensity = 0.3f;
                        Light.color = Color.indianRed;
                        animator.SetTrigger("Surcharge");
                        wasStunned = true;
                    }
                }
                yield return null;
            
            

        }
        float successChance = errorPercent
                      + employeErrorPercenBonus
                      + mypole.BoostEmployeError
                      + swatBoostError 
                      - (errorPercentMalus + RankingPercentMalus);

        successChance = Mathf.Clamp01(successChance);

        float roll = Random.Range(0f, 1f);

        if (roll < successChance)
        {
            mypole.WinMoney();
            moneyMake += mypole.paperValue;
            succeedPaper += 1 + Mathf.RoundToInt(BonusPaperDone);
        }


        numberOfPaperDone += 1 + Mathf.RoundToInt(BonusPaperDone);
        workAdvancement.value = 0;
       
        Light.intensity = 0.0f;
        if (isStunned == false)
        {
            employeImage.sprite = idleSprite;
            animator.SetTrigger("Idle");
        }
        yield return new WaitForSeconds(Mathf.Max(timeBeetwennWork - StressBonus - swatBoostTimeBeetweenWork, 0));
        if (isStunned)
        {
            employeImage.sprite = Surcharge;
            Light.intensity = 0.3f;
            Light.color = Color.indianRed;
            animator.SetTrigger("Surcharge");
        }
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
        employeImage.sprite = idleSprite;
        Working();
    }


    public void Malus(TypeOfMalus malusType, float value)
    {
        if (isStunned == true)
            return;
        if (SwatGoing == true)
            return;
        if (HasBeenSwat == true)
            return;
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
    public void ResetRankingMalus()
    {
        RankingWorkRateMalus = 0f;
        RankingPercentMalus = 0f;
    }
    public IEnumerator StunCoroutine(float duration)
    {
        
        isStunned = true;
        employeImage.sprite = Surcharge;
        Light.intensity = 0.3f;
        Light.color = Color.indianRed;
        animator.SetTrigger("Surcharge");
        
        yield return new WaitForSeconds(duration);
        isStunned = false;
        if (iamWorking)
        {
        Light.color = baseColor;
        employeImage.sprite = working;
        animator.SetTrigger("Working");
        }
         else
        {
        Light.intensity = 0.0f;
        employeImage.sprite = idleSprite;
        animator.SetTrigger("Idle");
         }
        
    }

    
    public void EndDayResetStat()
    {
        Light.intensity = 0.0f;
        HasBeenSwat = false;
        SwatGoing = false;
        occupied = false;
        iamWorking = false;
        Light.color = baseColor;
        if (WorkRoutine != null)
        {
            StopCoroutine(WorkRoutine);
            WorkRoutine = null;
        }
        employeImage.sprite = idleSprite;
        animator.SetTrigger("Idle");
       
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
        employeImage.sprite = idleSprite;
    }
    public void Start()
    {
        baseColor = Light.color;
    }
    private static readonly Color[] SurchargeColors = new Color[]
    {
    Color.white,                        // 0     - pas de surcharge
    new Color(1f, 0.95f, 0.95f),       // palier 10
    new Color(1f, 0.90f, 0.90f),       // palier 20
    new Color(1f, 0.85f, 0.85f),       // palier 30
    new Color(1f, 0.80f, 0.80f),       // palier 40  → FFCCCC
    new Color(1f, 0.75f, 0.75f),       // palier 50
    new Color(1f, 0.65f, 0.65f),       // palier 60
    new Color(1f, 0.55f, 0.55f),       // palier 70
    new Color(1f, 0.47f, 0.47f),       // palier 80
    new Color(0.97f, 0.40f, 0.40f),    // palier 90
    new Color(0.97f, 0.38f, 0.38f),    // palier 100 → F86262
    };

    private void Update()
    {
        if (mypole == null) return;
        if (isStunned == true)
            { employeImage.color = Color.white; return; }
           
        int stage = mypole.nextThresholdIndex; // 0 à 10
        stage = Mathf.Clamp(stage, 0, SurchargeColors.Length - 1);

        if (stage == 0)
        {
            employeImage.color = Color.white;
            return;
        }
        float speed = Mathf.Lerp(1.5f, 3f, (float)stage / (SurchargeColors.Length - 1));
        
        float ping = Mathf.PingPong(Time.time * speed, 1f);

        Color colorMin = SurchargeColors[stage - 1];
        Color colorMax = SurchargeColors[stage];

        employeImage.color = Color.Lerp(Color.white, colorMax, ping);
    }




    public void OnSwat()
    {
        occupied = true;
        if (StunRef != null)
        {
            StopCoroutine(StunRef);
            StunRef = null;
            isStunned = false; 
        }

        SwatGoing = true;
        StartCoroutine(SwatCoroutine());
    }

    public IEnumerator SwatCoroutine()
    {
        animator.SetTrigger("Swat");
        yield return new WaitForSeconds(2f);
        foreach(Transform child in GrilleSwat.transform)
        {
            GameObject go = child.gameObject;
            go.GetComponent<Image>().enabled = true;
            yield return new WaitForSeconds(0.02f);
        }

        
        yield return new WaitForSeconds(9f);
        for (int i = GrilleSwat.transform.childCount - 1; i >= 0; i--)
        {
            GrilleSwat.transform.GetChild(i).GetComponent<Image>().enabled = false;
            yield return new WaitForSeconds(0.02f);
        }
        HasBeenSwat = true;
        SwatGoing = false;
        occupied = false;
        StartCoroutine(SwatBuff());
        yield return null;  
    }

    public IEnumerator SwatBuff()
    {
        swatBoostError = 0.1f;
        swatBoostTimeBeetweenWork = 1.5f;
        swatBoostSpeed = 1.5f;
        yield return new WaitForSeconds(25f);
        swatBoostError = 0f;
        swatBoostSpeed = 0f;
        swatBoostTimeBeetweenWork = 0f;
        HasBeenSwat = false;

    }
}
