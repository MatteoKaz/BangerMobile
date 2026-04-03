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
    public float surchargeValue = 0f;
    public float maxSurcharge = 100f;
    public float surchargeStep = 5f;
    public float baseDelay = 0.01f;
    public int[] surchargeThresholds = { 10, 20, 30, 40, 50, 60, 70, 80, 90, 100 };
    public int nextThresholdIndex = 0;
    public Coroutine SurchargeRef;
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

    [Header("Tasks")]
    public List<PoleTask> taskQueue = new List<PoleTask>();
    public float defaultTaskDuration = 15f;
    [SerializeField] private GameObject postItPrefab;
    [SerializeField] private Transform[] postItSlots;
    private int postItSortingOrder = 0;
    private Coroutine taskTimerRef;

    public Transform quotatTextTarget;
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
    private static bool _firstOverloadNotified = false;

    public void Start()
    {
        UpdatePaperCount?.Invoke();
    }

    public void OnEnable()
    {
        myTuyaux.AddPaper += AddPaper;
        dayManager.ResetValueBeforeNextDay += ResetAllValue;
        dayManager.NewWeekReset += EndWeekReset;
        dayManager.FirstDayInitialization += ResetFirstOverloadFlag;
        timemanager.TimerEnded += StopWorking;
    }

    public void OnDisable()
    {
        myTuyaux.AddPaper -= AddPaper;
        dayManager.ResetValueBeforeNextDay -= ResetAllValue;
        dayManager.NewWeekReset -= EndWeekReset;
        dayManager.FirstDayInitialization -= ResetFirstOverloadFlag;
        timemanager.TimerEnded -= StopWorking;
    }

    public void StopWorking()
    {
        ResetSurcharge();
        Stop = true;
    }

    private static void ResetFirstOverloadFlag()
    {
        _firstOverloadNotified = false;
    }

    public void EndWeekReset()
    {
        foreach (Employe emp in employeList)
            emp.EndWeekReset();
    }

    public void AddPaper(float duration, int value)
    {
        PoleTask task = new PoleTask
        {
            timeLimit = duration,
            timeRemaining = duration,
            value = value,
            isActive = false
        };
        taskQueue.Add(task);
        totalPaper++;
        waitingPaper = totalPaper - activepaper;
        if (Stop) return;
        SpawnPostIt(task);
        StartTaskTimers();
        LaunchWorker();
        BeginSurcharge();
        UpdatePaperCount?.Invoke();
    }

    public void DecrementPaper(PoleTask task)
    {
        totalPaper = Mathf.Clamp(totalPaper - 1, 0, int.MaxValue);
        activepaper = Mathf.Clamp(activepaper - 1, 0, int.MaxValue);
        waitingPaper = Mathf.Max(0, totalPaper - activepaper);

        if (task != null)
        {
            if (task.postItObject != null)
                Destroy(task.postItObject);
            taskQueue.Remove(task);
        }

        if (Stop) return;
        LaunchWorker();
        UpdatePaperCount?.Invoke();
        BeginSurcharge();
        UpdatePaperFond?.Invoke();
    }

    private Color GetPoleColor()
    {
        return type switch
        {
            PoleType.RedPole => new Color(1f, UnityEngine.Random.Range(0.75f, 0.90f), UnityEngine.Random.Range(0.75f, 0.90f)),
            PoleType.BluePole => new Color(UnityEngine.Random.Range(0.75f, 0.90f), UnityEngine.Random.Range(0.75f, 0.90f), 1f),
            PoleType.GreenPole => new Color(UnityEngine.Random.Range(0.75f, 0.90f), 1f, UnityEngine.Random.Range(0.75f, 0.90f)),
            _ => Color.white
        };
    }

    private void SpawnPostIt(PoleTask task)
    {
        if (postItPrefab == null) return;

        int slotIndex = (taskQueue.Count - 1) % postItSlots.Length;
        Transform slot = postItSlots[slotIndex];

        GameObject go = Instantiate(postItPrefab, slot);

        int index = Mathf.Clamp(taskQueue.Count - 1, 0, 2);
        go.transform.localPosition = new Vector3(
            UnityEngine.Random.Range(-20f, 20f),
            index * -30f,
            0f
        );

        go.transform.SetAsFirstSibling();

        Image img = go.transform.Find("FondPostIt")?.GetComponent<Image>();
        if (img != null)
            img.color = GetPoleColor();

        Canvas canvas = go.GetComponent<Canvas>();
        if (canvas != null)
        {
            canvas.overrideSorting = true;
            canvas.sortingOrder = postItSortingOrder--;
        }

        PostItUI postIt = go.GetComponent<PostItUI>();
        postIt.linkedTask = task;
        task.postItObject = go;
    }

    public void StartTaskTimers()
    {
        if (taskTimerRef == null)
            taskTimerRef = StartCoroutine(TaskTimerTick());
    }

    private IEnumerator TaskTimerTick()
    {
        while (true)
        {
            for (int i = taskQueue.Count - 1; i >= 0; i--)
            {
                PoleTask task = taskQueue[i];
                task.timeRemaining -= Time.deltaTime;

                if (task.timeRemaining <= 0)
                {
                    taskQueue.RemoveAt(i);
                    totalPaper--;
                    waitingPaper = Mathf.Max(0, totalPaper - activepaper);
                    if (task.postItObject != null)
                        Destroy(task.postItObject);
                    UpdatePaperCount?.Invoke();
                }
            }
            if (taskQueue.Count == 0)
            {
                taskTimerRef = null;
                yield break;
            }
            yield return null;
        }
    }

    public void WinMoney()
    {
        localAdvencement += paperValue * Mathf.RoundToInt(BonusRevenus);
    }

    public void LooseMoney(int value)
    {
        localAdvencement -= value;
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
            if (employe.iamWorking == false && waitingPaper > 0)
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
        postItSortingOrder = 0;
        UpdatePaperCount?.Invoke();
        ResetSurcharge();

        if (taskTimerRef != null)
        {
            StopCoroutine(taskTimerRef);
            taskTimerRef = null;
        }
        foreach (PoleTask task in taskQueue)
            if (task.postItObject != null)
                Destroy(task.postItObject);
        taskQueue.Clear();

        foreach (Employe emp in employeList)
            emp.EndDayResetStat();
    }

    public void RebuildEmployeList()
    {
        StartCoroutine(RebuildAfterLoad());
    }

    IEnumerator RebuildAfterLoad()
    {
        yield return new WaitForEndOfFrame();
        employeList.Clear();

        List<InventorySlot> slots = new List<InventorySlot>();
        foreach (Transform child in contentparent.transform)
        {
            var slot = child.GetComponent<InventorySlot>();
            if (slot != null)
                slots.Add(slot);
        }

        slots.Sort((a, b) => a.slotIndex.CompareTo(b.slotIndex));

        foreach (var slot in slots)
        {
            var draggable = slot.GetComponentInChildren<DraggableItems>();
            if (draggable != null)
                employeList.Add(draggable.linkedEmploye);

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
                SurchargeRef = StartCoroutine(Surcharge());
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
                float employeeCount = Mathf.Max(1, employeList.Count);
                float chargePerEmployee = (float)waitingPaper / Mathf.Pow(employeeCount, 3.5f);
                surchargeValue += (surchargeStep * chargePerEmployee) / (1f + BoostTimeForSurcharge);
                surchargeValue = Mathf.Min(surchargeValue, maxSurcharge);
                Debug.LogWarning($"Surcharge {surchargeValue}");
                surchargeProgress.value = Mathf.Lerp(surchargeProgress.value, surchargeValue / maxSurcharge, 0.2f);
                if (nextThresholdIndex < surchargeThresholds.Length && surchargeValue >= surchargeThresholds[nextThresholdIndex])
                {
                    NotifyFirstOverloadOnce();
                    ApplyMalus(nextThresholdIndex);
                    nextThresholdIndex++;
                }
                float delay = baseDelay / (1f + (waitingPaper / (Mathf.Max(1, totalPaper) * employeeCount)));
                yield return new WaitForSeconds(delay);
            }
            else if (surchargeValue > 0)
            {
                surchargeValue -= decayRate * Time.deltaTime;
                surchargeValue = Mathf.Max(surchargeValue, 0f);
                while (nextThresholdIndex > 0 && surchargeValue < surchargeThresholds[nextThresholdIndex - 1])
                {
                    nextThresholdIndex--;
                    ApplyMalus(nextThresholdIndex);
                }
                surchargeProgress.value = Mathf.Lerp(surchargeProgress.value, surchargeValue / maxSurcharge, 0.2f);
                yield return null;
            }
            else
            {
                ApplyMalus(-1);
                yield break;
            }
        }
    }

    private static void NotifyFirstOverloadOnce()
    {
        if (_firstOverloadNotified) return;
        _firstOverloadNotified = true;
        TutorialManager.NotifyFirstOverload();
    }

    private void ApplyMalus(int i)
    {
        Debug.Log($"ApplyMalus appelé avec i={i} | nextThresholdIndex={nextThresholdIndex}");
        foreach (Employe emp in employeList)
        {
            switch (i)
            {
                case -1: emp.ResetMalus(); break;
                case 0: emp.Malus(TypeOfMalus.WorkRate, 0.25f); break;
                case 1: emp.Malus(TypeOfMalus.ErrorPercent, 0.05f); break;
                case 2: emp.Malus(TypeOfMalus.WorkRate, 0.5f); break;
                case 3: emp.Malus(TypeOfMalus.ErrorPercent, 0.1f); break;
                case 4: emp.Malus(TypeOfMalus.WorkRate, 1f); break;
                case 5: emp.Malus(TypeOfMalus.ErrorPercent, 0.15f); break;
                case 6: emp.Malus(TypeOfMalus.WorkRate, 2f); break;
                case 7: emp.Malus(TypeOfMalus.ErrorPercent, 0.25f); break;
                case 8: emp.Malus(TypeOfMalus.WorkRate, 3f); break;
                case 9: emp.Malus(TypeOfMalus.Stun, 8f); break;
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
            emp.ResetMalus();

        if (SurchargeRef != null)
        {
            StopCoroutine(SurchargeRef);
            SurchargeRef = null;
        }
    }
}