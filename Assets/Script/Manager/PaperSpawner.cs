using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class PaperSpawner : MonoBehaviour
{
    [Header("PapierRef")]
    [SerializeField] public GameObject PaperRed;
    [SerializeField] public GameObject PaperGreen;
    [SerializeField] public GameObject PaperBlue;

    [Header("Point De spawn")]
    [SerializeField] private Transform pointToSpawn;
    [SerializeField] private Transform pointToGo;
    [SerializeField] private Transform parentTransform;

    [Header("Tuyaux et pile")]
    [SerializeField] private Tuyaux tuyauxRed;
    [SerializeField] private Tuyaux tuyauxBlue;
    [SerializeField] private Tuyaux tuyauxGreen;
    [SerializeField] public Pile RefPileRed;
    [SerializeField] public Pile RefPileBlue;
    [SerializeField] public Pile RefPileGreen;

    public int papersRemaining;
    public float spawnDelay = 0.65f;
    public float DelayBeforeStart = 2f;

    [Header("Quota")]
    [SerializeField] QuotatManager quotatManager;
    public int redQuota = 10;
    public int blueQuota = 15;
    public int greenQuota = 5;

    private List<GameObject> spawnList = new List<GameObject>();
    private List<GameObject> spawnnedList = new List<GameObject>();

    [Header("QuotaDistribution")]
    public int totalPapers = 50;
    public int minPerType = 18;
    public int typesCount = 3;

    private int lastRhythm = -1;

    private Coroutine spawnRoutine;
    [SerializeField] DayManager dayManager;
    private bool canSpawn = true;
    [SerializeField] private AudioEventDispatcher audioEventDispatcher;

    public event Action AllPapersSpawned;
    private bool pauseDone = false;
    public void OnEnable()
    {
        quotatManager.QuotatChosen += StartSpawn;
        dayManager.DayEnd += StopSpawn;
        dayManager.DayTransition += DestroyEverything;
    }

    void OnDisable()
    {
        quotatManager.QuotatChosen -= StartSpawn;
        dayManager.DayTransition -= DestroyEverything;
        dayManager.DayEnd -= StopSpawn;
    }

    public IEnumerator SpawnStart()
    {
        yield return new WaitForSeconds(DelayBeforeStart);
        RandomAssignation();
        QuotatSetup();
        spawnRoutine = StartCoroutine(Spawn());
        Debug.Log("ddd");
    }

    public void StartSpawn()
    {
        canSpawn = true;
        StartCoroutine(SpawnStart());
    }

    public void StopSpawn()
    {
        if (spawnRoutine != null)
        {
            StopCoroutine(spawnRoutine);
            spawnRoutine = null;
        }
    }

    private void QuotatSetup()
    {
        spawnList.Clear();

        for (int i = 0; i < redQuota; i++)
            spawnList.Add(PaperRed);

        for (int i = 0; i < blueQuota; i++)
            spawnList.Add(PaperBlue);

        for (int i = 0; i < greenQuota; i++)
            spawnList.Add(PaperGreen);

        for (int i = 0; i < spawnList.Count; i++)
        {
            int random = Random.Range(i, spawnList.Count);
            GameObject tempPlace = spawnList[i];
            spawnList[i] = spawnList[random];
            spawnList[random] = tempPlace;
        }

        papersRemaining = spawnList.Count;
    }

    public IEnumerator Spawn()
    {
        audioEventDispatcher.PlayExclusiveAudio(AudioType.Pop);
        int globalSorting = 2;
        float currentRhythm = 1f;
        int spawnCount = 0;
        pauseDone = false;
        while (spawnList.Count > 0)
        {
            if (!canSpawn)
                break;

            if (papersRemaining <= 8)
            {
                currentRhythm = 2f;
            }
            if (!pauseDone && papersRemaining <= totalPapers / 2)
            {
                pauseDone = true;
                float pauseDuration = Random.Range(4f, 8);
                Debug.Log($"Pause milieu de spawn : {pauseDuration}s");
                yield return new WaitForSeconds(pauseDuration);
            }
            else if (spawnCount % 5 == 0)
            {
                int rhythm;
                do
                {
                    rhythm = Random.Range(0, 3);
                } while (rhythm == lastRhythm);

                lastRhythm = rhythm;

                switch (rhythm)
                {
                    case 0: currentRhythm = 6f; break;
                    case 1: currentRhythm = 4f; break;
                    case 2: currentRhythm = 3.5f; break;
                }
                Debug.Log($"Nouveau rythme: {rhythm}");
            }

            papersRemaining--;

            if (papersRemaining <= 0)
            {
                AllPapersSpawned?.Invoke();
                TutorialManager.NotifyAllPapersSpawned();
            }

            float spawntiming = Random.Range(spawnDelay, spawnDelay + 0.5f) * currentRhythm;
            GameObject prefab = spawnList[0];
            spawnList.RemoveAt(0);

            GameObject paperSpawn = Instantiate(prefab, pointToSpawn.position, Quaternion.identity, parentTransform);
            spawnnedList.Add(paperSpawn);
            PaperMove pm = paperSpawn.GetComponent<PaperMove>();
            pm.dayManager = dayManager;
            pm.Subscribe();
            paperSpawn.transform.SetAsLastSibling();

            pm.spawnPos = pointToGo.position;
            pm.Tuyauxleft = tuyauxRed;
            pm.Tuyauxup = tuyauxBlue;
            pm.Tuyauxright = tuyauxGreen;
            pm.PileRed = RefPileRed;
            pm.PileGreen = RefPileGreen;
            pm.PileBlue = RefPileBlue;
            pm.Paperduration = GetPaperDuration(quotatManager.currentDifficulty, dayManager.currentWeek);
            pm.value = 10;

            SpriteRenderer spriteRenderer = paperSpawn.GetComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = globalSorting;
            if (pm.timerImage != null)
                pm.timerImage.sortingOrder = globalSorting;
            pm.textcanvas.sortingOrder = globalSorting + 1;
            globalSorting--;
            pm.SetInitialPose();

            spawnCount++;
            yield return new WaitForSeconds(spawntiming);
        }
    }

    public int[] GenerateRandomQuotas()
    {
        minPerType = totalPapers / 4;
        int[] quotas = new int[typesCount];
        int remaining = totalPapers;

        for (int i = 0; i < typesCount - 1; i++)
        {
            int maxForThis = remaining - (typesCount - i - 1) * minPerType;
            quotas[i] = Random.Range(minPerType, maxForThis + 1);
            remaining -= quotas[i];
        }

        quotas[typesCount - 1] = remaining;
        return quotas;
    }

    public void RandomAssignation()
    {
        int[] quotas = GenerateRandomQuotas();
        PaperType[] types = { PaperType.Red, PaperType.Blue, PaperType.Green };

        for (int i = 0; i < types.Length; i++)
        {
            int randomIndex = Random.Range(i, types.Length);
            PaperType temp = types[i];
            types[i] = types[randomIndex];
            types[randomIndex] = temp;
        }

        redQuota = 0;
        blueQuota = 0;
        greenQuota = 0;

        for (int i = 0; i < types.Length; i++)
        {
            switch (types[i])
            {
                case PaperType.Red: redQuota = quotas[i]; break;
                case PaperType.Blue: blueQuota = quotas[i]; break;
                case PaperType.Green: greenQuota = quotas[i]; break;
            }
        }
    }

    public void DestroyEverything()
    {
        for (int i = 0; i < spawnnedList.Count; i++)
            Destroy(spawnnedList[i].gameObject);

        spawnnedList.Clear();
        spawnList.Clear();
    }


   


    private float GetPaperDuration(int difficulty, int week)
    {
        // min et max de la durée du papier
        float startMin, startMax, targetMin, targetMax;

        switch (difficulty)
        {
            case 0: // Easy — papiers durent longtemps
                startMin = 35f; startMax = 45f;
                targetMin = 15f; targetMax = 25f;
                break;
            case 1: // Mid
                startMin = 25f; startMax = 35f;
                targetMin = 11f; targetMax = 20f;
                break;
            case 2: // Hard — papiers disparaissent vite
                startMin = 18f; startMax = 25f;
                targetMin = 5f; targetMax = 15f;
                break;
            default:
                startMin = 25f; startMax = 35f;
                targetMin = 18f; targetMax = 25f;
                break;
        }

        float maxWeek = 10f;
        float t = Mathf.Clamp01((week - 1f) / (maxWeek - 1f));

        float min = Mathf.Lerp(startMin, targetMin, t);
        float max = Mathf.Lerp(startMax, targetMax, t);

        return Random.Range(min, max);
    }
}
