using System.Collections;
using UnityEngine;

public class PaperMove : MonoBehaviour
{
    [Header("Tuyaux ref")]
    [SerializeField] public Tuyaux Tuyauxup;
    [SerializeField] public Tuyaux Tuyauxleft;
    [SerializeField] public Tuyaux Tuyauxright;
    [Header("PileRef")]
    [SerializeField] public Pile PileRed;
    [SerializeField] public Pile PileBlue;
    [SerializeField] public Pile PileGreen;

    [Header("Reste")]
    [SerializeField] public Vector3 spawnPos;
    bool validPaper = false;
    public bool OnPile = false;
    bool Launch = false;

    public PaperType paperType;
    public GameObject myself;

    public Vector3 pileTarget;
    public Pile pileRef;
    [SerializeField] public DayManager dayManager;
    private bool CanThrow = true;

    private static bool _firstPaperSentNotified = false;

    public void Awake() { }

    public void Subscribe()
    {
        if (dayManager != null)
        {
            dayManager.DayEnd += DisableThrow;
            dayManager.FirstDayInitialization += ResetFirstPaperSentFlag;
        }
    }

    public void OnDisable()
    {
        if (dayManager != null)
        {
            dayManager.DayEnd -= DisableThrow;
            dayManager.FirstDayInitialization -= ResetFirstPaperSentFlag;
        }
    }

    public void SetInitialPose()
    {
        myself = this.gameObject;
        StartCoroutine(SpawnPosition());
    }

    public void DisableThrow()
    {
        CanThrow = false;
    }

    /// <summary>Remet à zéro le flag du premier papier envoyé au début du premier jour.</summary>
    private static void ResetFirstPaperSentFlag()
    {
        _firstPaperSentNotified = false;
    }

    public void MoveRightTuyaux()
    {
        if (!CanThrow) return;
        if (OnPile == true) return;

        NotifyFirstPaperSentOnce();
        StartCoroutine(MoveToTuyaux(Tuyauxright.transform.position, Tuyauxright));
        validPaper = Tuyauxright.tuyauxType == paperType;
    }

    public void MoveUpTuyaux()
    {
        if (!CanThrow) return;
        if (OnPile == true) return;

        NotifyFirstPaperSentOnce();
        StartCoroutine(MoveToTuyaux(Tuyauxup.transform.position, Tuyauxup));
        validPaper = Tuyauxup.tuyauxType == paperType;
    }

    public void MoveLeftTuyaux()
    {
        if (!CanThrow) return;
        if (OnPile == true) return;

        NotifyFirstPaperSentOnce();
        StartCoroutine(MoveToTuyaux(Tuyauxleft.transform.position, Tuyauxleft));
        validPaper = Tuyauxleft.tuyauxType == paperType;
    }

    /// <summary>Notifie le TutorialManager du premier swipe, une seule fois par session.</summary>
    private static void NotifyFirstPaperSentOnce()
    {
        if (_firstPaperSentNotified) return;
        _firstPaperSentNotified = true;
        TutorialManager.NotifyFirstPaperSent();
    }

    public void MoveToPile()
    {
        if (!CanThrow) return;
        if (OnPile == false)
        {
            Debug.Log("Pile");
            switch (paperType)
            {
                case PaperType.Red:
                    pileRef = PileRed;
                    pileTarget = PileRed.transform.position;
                    break;
                case PaperType.Green:
                    pileRef = PileGreen;
                    pileTarget = PileGreen.transform.position;
                    break;
                case PaperType.Blue:
                    pileRef = PileBlue;
                    pileTarget = PileBlue.transform.position;
                    break;
            }
            OnPile = true;
            StartCoroutine(MoveToPileCo(pileTarget, pileRef));
        }
    }

    public void RemoveFromPile()
    {
        if (!CanThrow) return;
        if (OnPile == true)
        {
            switch (paperType)
            {
                case PaperType.Red:
                    PileRed.RemoveFromPile();
                    break;
                case PaperType.Green:
                    PileGreen.RemoveFromPile();
                    break;
                case PaperType.Blue:
                    PileBlue.RemoveFromPile();
                    break;
            }
            StartCoroutine(SpawnPosition());
        }
    }

    IEnumerator MoveToTuyaux(Vector3 target, Tuyaux tuyaux)
    {
        Vector3 start = transform.position;
        float t = 0f;
        float duration = 0.3f;

        Vector3 direction = target - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float Offset = 90f;
        float randomValue = Random.Range(1, 5);
        Quaternion targetRot = Quaternion.Euler(0f, 0f, angle - Offset + randomValue);

        float arcHeight = Random.Range(0.3f, 0.7f);
        float rotationAmplitude = Random.Range(5f, 15f);
        float oscillationSpeed = Random.Range(3f, 6f);

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            Vector3 basePos = Vector3.Lerp(start, target, t);
            basePos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;
            transform.position = basePos;
            float oscillation = Mathf.Sin(t * Mathf.PI * 4f) * rotationAmplitude;
            Quaternion dynamicRot = Quaternion.Euler(0f, 0f, angle + Offset + oscillation);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * 10f);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, dynamicRot, 120f * Time.deltaTime);
            yield return null;
            transform.position = target;
            transform.rotation = targetRot;
        }

        t = 0f;
        float durationScale = 0.15f;
        Vector3 scalePaper = transform.localScale;
        Vector3 TargetScale = new Vector3(0f, 0f, 0f);
        Vector3 targetpos = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
        while (t < 1f)
        {
            t += Time.deltaTime / durationScale;
            transform.localScale = Vector3.Lerp(scalePaper, TargetScale, t);
            transform.position = Vector3.Lerp(target, targetpos, t);
            yield return null;
        }

        if (validPaper == true)
        {
            tuyaux.GoodPaper();
            Destroy(myself);
        }
        else
        {
            tuyaux.WrongPaper();
            Destroy(myself);
        }
    }

    public IEnumerator SpawnPosition()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        sprite.color = Color.white;
        Vector3 start = transform.position;
        float t = 0f;
        float duration = 0.3f;

        Vector3 direction = spawnPos - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float Offset = 90f;
        float randomValue = Random.Range(1, 5);
        Quaternion targetRot = Quaternion.Euler(0f, 0f, angle - Offset + randomValue);

        float arcHeight = Random.Range(-0.5f, 0.9f);
        float rotationAmplitude = Random.Range(-15f, 25f);
        float oscillationSpeed = Random.Range(3f, 6f);

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            Vector3 basePos = Vector3.Lerp(start, spawnPos, t);
            basePos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;
            transform.position = basePos;
            float oscillation = Mathf.Sin(t * Mathf.PI * 4f) * rotationAmplitude;
            Quaternion dynamicRot = Quaternion.Euler(0f, 0f, angle + Offset + oscillation);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * 10f);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, dynamicRot, 120f * Time.deltaTime);
            yield return null;
            transform.position = spawnPos;
            OnPile = false;
        }
    }

    public IEnumerator MoveToPileCo(Vector3 target, Pile pile)
    {
        Vector3 start = transform.position;
        float t = 0f;
        float duration = 0.3f;

        Vector3 direction = target - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        float Offset = 90f;
        float randomValue = Random.Range(-5, 5);
        Quaternion targetRot = Quaternion.Euler(0f, 0f, angle - Offset + randomValue);

        float arcHeight = Random.Range(0.3f, 0.7f);
        float rotationAmplitude = Random.Range(5f, 15f);
        float oscillationSpeed = Random.Range(3f, 6f);

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            Vector3 basePos = Vector3.Lerp(start, target, t);
            basePos.y += Mathf.Sin(t * Mathf.PI) * arcHeight;
            transform.position = basePos;
            float oscillation = Mathf.Sin(t * Mathf.PI * 4f) * rotationAmplitude;
            Quaternion dynamicRot = Quaternion.Euler(0f, 0f, angle + Offset + oscillation);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, Time.deltaTime * 10f);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, dynamicRot, 120f * Time.deltaTime);
            yield return null;
            transform.position = target;
            transform.rotation = targetRot;
        }

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        sprite.color = new Color(0f, 0f, 0f, 0f);
        pile.AddToPile();
    }
}
