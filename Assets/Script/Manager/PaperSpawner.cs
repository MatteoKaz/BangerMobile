using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


    public float spawnDelay = 1.5f;
    public float DelayBeforeStart = 2f;

    [Header("Quota")]
    [SerializeField] QuotatManager quotatManager;
    public int redQuota = 10;
    public int blueQuota = 15;
    public int greenQuota = 5;

    private List<GameObject> spawnList = new List<GameObject>();
    private List<GameObject> spawnnedList = new List<GameObject>();

    [Header("QuotaDistribution")]
    public int totalPapers = 50;//NombreMax
    public int minPerType = 18; // quota min 
    public int typesCount = 3; //NombreDeType

    private Coroutine spawnRoutine;
    [SerializeField] DayManager dayManager;
    private bool canSpawn = true;


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
        StopCoroutine(spawnRoutine);
        canSpawn = false;

    }
    private void QuotatSetup()
    {
        spawnList.Clear();

        // Ajouter le nombre exact de papiers de chaque type
        for (int i = 0; i < redQuota; i++)
        {
            spawnList.Add(PaperRed);
        }
           
        for (int i = 0; i < blueQuota; i++)
        {
            spawnList.Add(PaperBlue);
        }
        for (int i = 0; i < greenQuota; i++)
        { 
            spawnList.Add(PaperGreen); 
        }

        for (int i = 0; i < spawnList.Count; i++)
        {
            int random = Random.Range(i, spawnList.Count);
            GameObject tempPlace = spawnList[i];
            spawnList[i] = spawnList[random];
            spawnList[random] = tempPlace;
        }

    }


   
    public IEnumerator Spawn()
    {
        int globalSorting = 2;
        while (spawnList.Count > 0)
        {
            if (!canSpawn)
                break;
            float spawntiming = Random.Range(spawnDelay, spawnDelay + 2f);
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
            GameObject parentObj = paperSpawn;
            SpriteRenderer spriteRenderer = parentObj.GetComponent<SpriteRenderer>();
            spriteRenderer.sortingOrder = globalSorting--;
            pm.SetInitialPose();
            
            yield return new WaitForSeconds(spawntiming);
        }

    }

    //Generation de quota
    public int[] GenerateRandomQuotas()
    {
        minPerType = totalPapers/4;
        int[] quotas = new int[typesCount];

        int remaining = totalPapers;

        for (int i = 0; i < typesCount - 1; i++)
        {
            int maxForThis = remaining - (typesCount - i - 1) * minPerType;
            quotas[i] = Random.Range(minPerType, maxForThis + 1);
            remaining -= quotas[i];
        }

        // Le dernier type prend le reste
        quotas[typesCount - 1] = remaining;


        return quotas;
    }

    public void RandomAssignation()
    {
        int[] quotas = GenerateRandomQuotas(); // par ex [15, 20, 15]

        // Liste des types
        PaperType[] types = { PaperType.Red, PaperType.Blue, PaperType.Green };

        // Mélanger la liste de types
        for (int i = 0; i < types.Length; i++)
        {
            int randomIndex = Random.Range(i, types.Length);
            PaperType temp = types[i];
            types[i] = types[randomIndex];
            types[randomIndex] = temp;
        }

        // Assigner les quotas aléatoirement
        redQuota = 0;
        blueQuota = 0;
        greenQuota = 0;

        for (int i = 0; i < types.Length; i++)
        {
            switch (types[i])
            {
                case PaperType.Red:
                    redQuota = quotas[i];
                    break;
                case PaperType.Blue:
                    blueQuota = quotas[i];
                    break;
                case PaperType.Green:
                    greenQuota = quotas[i];
                    break;
            }
        }
    }

    public void DestroyEverything()
    {
        for(int i = 0; i< spawnnedList.Count; i++)
        {
            Destroy(spawnnedList[i].gameObject);

        }
        spawnnedList.Clear();
        spawnList.Clear();
    }
}
