
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

using UnityEngine;

using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using static Pole;
using Random = UnityEngine.Random;

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
    private List<PoleTask> bonusTasks = new List<PoleTask>();
    public List<Sprite> upgradesImages = new List<Sprite>();
    public Dictionary<Sprite, int> upgradeCounts = new Dictionary<Sprite, int>();

    [Header("Malus")]
    public int StunMalus = 5;
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
    public int LoosePaper;
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
    [SerializeField] Animator paperDechirer;
    [SerializeField] ParticleSystem smoke;
    [SerializeField] Image cigarette;
    [SerializeField] GameObject goutte;
    [SerializeField] GameObject angry;
    [SerializeField] public GameObject couronne;

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
    public AnimationCurve swatAnim;
    public Image FondSwat;
    public Image InteractiveImage;

    private PoleTask currentTask;
    public bool iamWorking = false;
    private Coroutine WorkRoutine;
    private Coroutine StunRef;
    private bool cancelled = false;

    public event Action ScoreWinAnim;
    public event Action LooseMoney;

    [SerializeField] private AudioEventDispatcher audioeventdispatcher;
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
        if (currentTask != null)
        {
            currentTask.isActive = false;
            currentTask = null;
        }
        foreach (PoleTask bonus in bonusTasks)
            if (bonus != null) bonus.isActive = false;
        bonusTasks.Clear();

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
        
        GetComponentInChildren<PileBackEmploye>()?.OnCountUpdated();
        GetComponentInChildren<PileBackEmploye>()?.OnPoleChanged();
    }


    // fonction  lancer lorsqu'il commence a work 
    public void Working(PoleTask task, List<PoleTask> preloadedBonus = null)
    {
        cancelled = false;
        PoleTask myTask = task;
        if (myTask != null && iamWorking == false)
        {
            bonusTasks.Clear();
            if (preloadedBonus != null)
                bonusTasks.AddRange(preloadedBonus);

            foreach (PoleTask b in bonusTasks)
            {
                Image imge = b.postItObject?.transform.Find("FondPostIt")?.Find("Perso")?.GetComponent<Image>();
                if (imge != null) imge.enabled = true;
            }
            Image img = myTask.postItObject?.transform.Find("FondPostIt")?.Find("Perso")?.GetComponent<Image>();
            if (img != null) img.enabled = true;

            iamWorking = true;
            currentTask = myTask;
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
            else
            {
                
            }
        }
    }

    public IEnumerator Work()
    {
       
        float t = 0f;
        wasStunned = true;
        while (t < 1)
        {
            if (cancelled)
            {
                cancelled = false;
                iamWorking = false;
                if (currentTask != null)
                {
                    mypole.DecrementPaper(currentTask); 
                    currentTask = null;
                }
                foreach (PoleTask bonus in bonusTasks)
                    if (bonus != null) mypole.DecrementPaper(bonus);
                bonusTasks.Clear();
                yield break;
            }
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
            if (currentTask == null || currentTask.isExpired)
            {
                if (currentTask != null)
                    mypole.DecrementPaper(currentTask);
                currentTask = null;

                foreach (PoleTask bonus in bonusTasks)
                    if (bonus != null) mypole.DecrementPaper(bonus);
                bonusTasks.Clear();

                iamWorking = false;
                workAdvancement.value = 0;
                Light.intensity = 0.0f;
                if (!isStunned)
                {
                    employeImage.sprite = idleSprite;
                    animator.SetTrigger("Idle");
                }
                yield break;
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
        if (cancelled)
        {
            cancelled = false;
            iamWorking = false;
            if (currentTask != null)
            {
                mypole.DecrementPaper(currentTask); // ← retire de la queue proprement
                currentTask = null;
            }
            foreach (PoleTask bonus in bonusTasks)
                if (bonus != null) mypole.DecrementPaper(bonus);
            bonusTasks.Clear();
            yield break;
        }
        float successChance = errorPercent
                      + employeErrorPercenBonus
                      + mypole.BoostEmployeError
                      + swatBoostError 
                      - (errorPercentMalus + RankingPercentMalus);

        successChance = Mathf.Clamp01(successChance);

        float roll = Random.Range(0f, 1f);
        Debug.LogWarning($"error percent = {successChance}");
        if (roll < successChance)
        {
            mypole.WinMoney();
            ScoreWinAnim?.Invoke();
            moneyMake += mypole.paperValue;
            succeedPaper += 1;
        }
        else
        {
            audioeventdispatcher.PlayAudio(AudioType.RippingPaper);
            paperDechirer.SetTrigger("Launch");
            LoosePaper += 1;
        }

            numberOfPaperDone += 1 ;
        workAdvancement.value = 0;
       
        Light.intensity = 0.0f;
        if (isStunned == false)
        {
            employeImage.sprite = idleSprite;
            animator.SetTrigger("Idle");
        }
        mypole.DecrementPaper(currentTask);
        currentTask = null;
        foreach (PoleTask bonus in bonusTasks)
        {
            if (bonus == null || bonus.isExpired)
                mypole.DecrementPaper(bonus);
        }
        bonusTasks.RemoveAll(b => b == null || b.isExpired);

        // Traitement des bonus valides
        foreach (PoleTask bonus in bonusTasks)
        {
            roll = Random.Range(0f, 1f);

            if (roll < successChance)
            {
                mypole.WinMoney();
                ScoreWinAnim?.Invoke();
                moneyMake += mypole.paperValue;
                succeedPaper++;
            }
            else
            {
                LoosePaper += 1;
            }
                numberOfPaperDone++;
            mypole.DecrementPaper(bonus);
        }

        if (cancelled)
        {
            cancelled = false;
            iamWorking = false;
            yield break;
        }
        bonusTasks.Clear();
        
        yield return new WaitForSeconds(Mathf.Max(timeBeetwennWork - StressBonus - swatBoostTimeBeetweenWork, 0));
        if (isStunned)
        {
            employeImage.sprite = Surcharge;
            Light.intensity = 0.3f;
            Light.color = Color.indianRed;
            animator.SetTrigger("Surcharge");
        }
        

        iamWorking = false;
        WorkRoutine = null;
        mypole.LaunchWorker();



    }

    public void SwitchPole(Pole pole)
    {
        if (WorkRoutine != null)
        {
            if (currentTask != null)
            {
                Image img = currentTask.postItObject
                    ?.transform.Find("FondPostIt")?.Find("Perso")?.GetComponent<Image>();
                if (img != null) img.enabled = false;
                currentTask.isActive = false;
                mypole.activepaper = Mathf.Max(0, mypole.activepaper - 1);
                currentTask = null;
            }
            foreach (PoleTask bonus in bonusTasks)
            {
                if (bonus == null) continue;
                Image img = bonus.postItObject
                    ?.transform.Find("FondPostIt")?.Find("Perso")?.GetComponent<Image>();
                if (img != null) img.enabled = false;
                bonus.isActive = false;
                mypole.activepaper = Mathf.Max(0, mypole.activepaper - 1);
            }
            bonusTasks.Clear();
            StopCoroutine(WorkRoutine); // ← StopCoroutine SANS cancelled=true
            WorkRoutine = null;
        }

        iamWorking = false;
        workAdvancement.value = 0f;
        employeWorkRateMalus = 0f;
        errorPercentMalus = 0f;

        Pole oldPole = mypole;

        // Retire l'employé de l'ancien pole IMMÉDIATEMENT sans attendre RebuildAfterLoad
        oldPole.employeList.Remove(this);
        oldPole.UpdateBackLog(); // ← maintenant employeList est déjà à jour
        oldPole.LaunchWorker();

        mypole = pole;

        if (!isStunned)
            employeImage.sprite = idleSprite;
        FeedbackSurcharge(pole.nextThresholdIndex);
       
        mypole.UpdateBackLog();
        mypole.LaunchWorker();
        GetComponentInChildren<PileBackEmploye>()?.OnPoleChanged();
        GetComponentInChildren<PileBackEmploye>()?.OnCountUpdated();
        GetComponentInChildren<PileBackEmploye>()?.OnPoleChanged();
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

        mypole.LooseMoney(StunMalus);
        LooseMoney?.Invoke();
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
            employeImage.sprite = idleSprite; // ← remet idle même si Work() est déjà terminé
            animator.SetTrigger("Idle");
        }

    }

    
    public void EndDayResetStat()
    {
        Light.intensity = 0.0f;
        HasBeenSwat = false;
        SwatGoing = false;
        occupied = false;
        cancelled = false;  
        iamWorking = false;
        Light.color = baseColor;
        foreach (PoleTask bonus in bonusTasks)
        {
            if (bonus == null) continue;
            bonus.isActive = false;
        }
        bonusTasks.Clear();
        currentTask = null;
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
        LoosePaper = 0;
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
        GetComponentInChildren<PileBackEmploye>()?.OnCountUpdated();
        GetComponentInChildren<PileBackEmploye>()?.OnPoleChanged();
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
    Color.white,        // palier 10
    Color.white,        // palier 20
    Color.white,       // palier 30
    new Color(1f, 0.85f, 0.85f),       // palier 40  → FFCCCC
    new Color(1f, 0.80f, 0.80f),       // palier 50
    new Color(1f, 0.75f, 0.75f),       // palier 60
    new Color(1f, 0.65f, 0.65f),       // palier 70
    new Color(1f, 0.47f, 0.47f),       // palier 80
    new Color(0.97f, 0.40f, 0.40f),    // palier 90
    new Color(0.97f, 0.38f, 0.38f),    // palier 100 → F86262
    };

    private void Update()
    {
        if (mypole == null) return;
        if (HasBeenSwat )
        {
            float hue = Mathf.Repeat(Time.time * 0.9f, 1f); // vitesse du cycle
            employeImage.color = Color.HSVToRGB(hue, 0.25f, 1f);
            return;
        }
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
        InteractiveImage.raycastTarget = false; 
        SwatGoing = true;

        StartCoroutine(SwatCoroutine());
    }

    public IEnumerator SwatCoroutine()
    {
        Light.intensity = 0.0f;
        Color ColorFond = new Color(0x82 / 255f, 0x82 / 255f, 0x82 / 255f, 0.6f);
        Color color = new Color(0x8C / 255f, 0x8C / 255f, 0x8C / 255f);
        employeImage.sprite = idleSprite;
        animator.SetTrigger("Swat");
        yield return new WaitForSeconds(1.5f);
       
        float t = 0;
        t = 0;
        foreach (Transform child in GrilleSwat.transform)
        {
            GameObject go = child.gameObject;
            go.GetComponent<Image>().enabled = true;

        }
        while (t < 1f)
        {
            for (int i = GrilleSwat.transform.childCount - 1; i >= 0; i--)
            {
                Image grille = GrilleSwat.transform.GetChild(i).GetComponent<Image>();


                t += Time.deltaTime / 0.4f;
                float normalized = Mathf.Clamp01(t);




                grille.color = Color.Lerp(Color.clear, color, normalized);
                yield return null;
            }


        }
        yield return new WaitForSeconds(0.1f);
        t = 0;
        while (t< 0.75f)
        {
            t += Time.deltaTime/0.75f;
            VerticalLayoutGroup vlg =GrilleSwat.GetComponent<VerticalLayoutGroup>();
            float normalized = t / 0.75f;

            float curve = swatAnim.Evaluate(normalized);
            vlg.spacing = Mathf.Lerp(-99.1f, -47.7f, curve);
            yield return null;

        }
        t = 0;
        
        while (t < 1f)
        {
            t += Time.deltaTime / 0.2f;
            float normalized = Mathf.Clamp01(t);
            FondSwat.color = Color.Lerp(Color.clear, ColorFond, normalized);
            yield return null;
        }

        yield return new WaitForSeconds(1.5f);
        Vector3 originalPos = GrilleSwat.transform.localPosition;
        for (int i = 0; i< 3; i++)
        {
            t = 0f;
            
            while (t < 1f)
            {
                t += Time.deltaTime / Random.Range(0.5f,0.75f);
                float normalized = Mathf.Clamp01(t);

                // Shake position
                float shakeStrength = Mathf.Lerp(8f, 0f, normalized);
                Vector3 shakeOffset = new Vector3(
                    Random.Range(-1f, 1f) * shakeStrength,
                    Random.Range(-1f, 1f) * shakeStrength,
                    0f
                );

                // Punch scale
                float punch = Mathf.Sin(normalized * Mathf.PI * 4f) * Mathf.Lerp(0.15f, 0f, normalized);
                Vector3 punchScale = Vector3.one * (1f + punch);

                GrilleSwat.transform.localPosition = originalPos + shakeOffset;
                GrilleSwat.transform.localScale = punchScale;

                yield return null;
            }
        }
        

        // Reset
        GrilleSwat.transform.localPosition = originalPos;
        GrilleSwat.transform.localScale = Vector3.one;
    
        yield return new WaitForSeconds(3f);
        t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime / 0.2f;
            float normalized = Mathf.Clamp01(t);
            FondSwat.color = Color.Lerp(ColorFond, Color.clear, normalized);
            yield return null;
        }
        t = 0;
        while (t < 1f)
        {
            t += Time.deltaTime/0.75f ;
            float normalized = Mathf.Clamp01(t);
            VerticalLayoutGroup vlg = GrilleSwat.GetComponent<VerticalLayoutGroup>();
           

            
            vlg.spacing = Mathf.Lerp(-47.7f,  - 99.1f, normalized);
            yield return null;
        }

        yield return new WaitForSeconds(0.5f);
        t = 0;
        while (t < 1f)
        {
            for (int i = GrilleSwat.transform.childCount - 1; i >= 0; i--)
             {
            Image grille = GrilleSwat.transform.GetChild(i).GetComponent<Image>();
           
            
                t += Time.deltaTime / 0.3f;
                float normalized = Mathf.Clamp01(t);
                



                grille.color = Color.Lerp(color, Color.clear, normalized);
                yield return null;
            }
            

        }
        foreach (Transform child in GrilleSwat.transform)
        {
            GameObject go = child.gameObject;
            go.GetComponent<Image>().enabled = false;
            
        }

        yield return new WaitForSeconds(0.25f);

        InteractiveImage.raycastTarget = true;
        HasBeenSwat = true;
        SwatGoing = false;
        occupied = false;
        
        StartCoroutine(SwatBuff());
        yield return null;  
    }

    public IEnumerator SwatBuff()
    {
        swatBoostError = 0.15f;
        swatBoostTimeBeetweenWork = 1.5f;
        swatBoostSpeed = 2f;
        yield return new WaitForSeconds(25f);
        swatBoostError = 0f;
        swatBoostSpeed = 0f;
        swatBoostTimeBeetweenWork = 0f;
        HasBeenSwat = false;

    }


    public void FeedbackSurcharge(int i)
    {
        if (i < 6 && i > 2)
        {
            goutte.SetActive(true);
            angry.SetActive(false);
        }
        else if (i >= 9)
        {
            goutte.SetActive(false);
            angry.SetActive(false);

        }
        else if (i >= 6)
        {
            goutte.SetActive(false);
            angry.SetActive(true);
        }
        else if (i <= 2)
        {
            goutte.SetActive(false);
            angry.SetActive(false);
            
        }

    }
}
