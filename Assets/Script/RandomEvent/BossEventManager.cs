using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class BossEventManager : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private DayManager dayManager;
    [SerializeField] private PoleManager poleManager;
    [SerializeField] private QuotatManager quotatManager;

    [Header("Boss visuel")]
    [SerializeField] private GameObject bossObject;
    [SerializeField] private Transform[] bossPoses;
    [SerializeField] private float rotateInDuration = 0.6f;
    [SerializeField] private float rotateOutDuration = 0.4f;

    [Header("Lights 2D")]
    [SerializeField] private Light2D eventLight1;
    [SerializeField] private Light2D eventLight2;
    [SerializeField] private float lightFadeInDuration = 0.5f;
    [SerializeField] private float lightFadeOutDuration = 0.4f;

    [Header("Paramètres événement")]
    [SerializeField] private float malus = 0.3f;
    [SerializeField] private float eventDuration = 20f;
    [SerializeField] private float chancePerDay = 0.5f;
    [SerializeField] private float minDelay = 5f;
    [SerializeField] private float maxDelay = 20f;

    private static readonly Color ColorRed = new Color(1f, 0.2f, 0.2f);
    private static readonly Color ColorBlue = new Color(0.2f, 0.4f, 1f);
    private static readonly Color ColorGreen = new Color(0.2f, 1f, 0.4f);

    public event Action<Pole> BossEventStarted;
    public event Action<Pole> BossEventEnded;

    private Pole _targetPole;
    private int _originalPaperValue;
    private bool _malusActive = false;

    private Coroutine _eventCoroutine;
    private Coroutine _lightCoroutine;
    private bool _eventRunning = false;
    private bool _eventUsedThisWeek = false;

    public bool IsEventActiveOn(Pole pole) => _eventRunning && _targetPole == pole;

    private void OnEnable()
    {
        quotatManager.QuotatIsSet += OnDayBegin;
        dayManager.DayEnd += OnDayEnd;
        dayManager.NewWeekReset += OnNewWeek;
    }

    private void OnDisable()
    {
        quotatManager.QuotatIsSet -= OnDayBegin;
        dayManager.DayEnd -= OnDayEnd;
        dayManager.NewWeekReset -= OnNewWeek;
    }

    private void OnNewWeek() => _eventUsedThisWeek = false;

    private void OnDayBegin()
    {
        if (dayManager.currentDay < 4) return;
        if (_eventUsedThisWeek) return;
        if (UnityEngine.Random.value > chancePerDay) return;

        _eventUsedThisWeek = true;

        if (_eventCoroutine != null) StopCoroutine(_eventCoroutine);
        _eventCoroutine = StartCoroutine(EventRoutine());
    }

    private void OnDayEnd()
    {
        StopEvent();
    }

   

    private IEnumerator EventRoutine()
    {
        // Délai avant apparition
        yield return new WaitForSeconds(UnityEngine.Random.Range(minDelay, maxDelay));

        // Choisit un pôle
        Pole[] poles = poleManager.poles;
        _targetPole = poles[UnityEngine.Random.Range(0, poles.Length)];
        _originalPaperValue = _targetPole.paperValue;
        _malusActive = false;

        _eventRunning = true;
        BossEventStarted?.Invoke(_targetPole);
        SetLight(_targetPole);
        bossObject.SetActive(false);
        yield return StartCoroutine(BossEnter());

        // Applique le malus
        _targetPole.paperValue = Mathf.RoundToInt(_originalPaperValue * malus);
        _malusActive = true;

        yield return new WaitForSeconds(eventDuration);

        // Retire le malus
        if (_malusActive && _targetPole != null)
        {
            _targetPole.paperValue = _originalPaperValue;
            _malusActive = false;
        }

        yield return StartCoroutine(BossExit());

        DisableLight();

        BossEventEnded?.Invoke(_targetPole);
        _eventRunning = false;
        _targetPole = null;

        // Relance un nouveau cycle dans la même journée
        _eventCoroutine = StartCoroutine(EventRoutine());
    }

  

    private void StopEvent()
    {
        if (_eventCoroutine != null)
        {
            StopCoroutine(_eventCoroutine);
            _eventCoroutine = null;
        }

        // Restaure paperValue avec la vraie valeur originale
        if (_malusActive && _targetPole != null)
        {
            _targetPole.paperValue = _originalPaperValue;
            _malusActive = false;
        }

        if (_targetPole != null)
        {
            BossEventEnded?.Invoke(_targetPole);
            _targetPole = null;
        }

        if (bossObject != null)
            bossObject.SetActive(false);

        DisableLight();
        _eventRunning = false;
    }

    

    private IEnumerator BossEnter()
    {
        if (bossObject == null || bossPoses == null || bossPoses.Length == 0) yield break;

        Transform pose = bossPoses[UnityEngine.Random.Range(0, bossPoses.Length)];
        Vector3 target = pose.position;
        Vector3 start = target + new Vector3(-Screen.width * 0.02f, 0f, 0f);
        Quaternion rotation = pose.localRotation;
        bossObject.SetActive(true);
        bossObject.transform.position = start;
        bossObject.transform.rotation = rotation;
        Quaternion startRot = Quaternion.Euler(0f, 0f, -90f);
        Quaternion targetRot = pose.rotation;


        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / rotateInDuration;
            float ease = 1f - Mathf.Pow(1f - Mathf.Clamp01(t), 3f);
            bossObject.transform.position = Vector3.Lerp(start, target, ease);
            bossObject.transform.rotation = Quaternion.Lerp(startRot, targetRot, ease);
            yield return null;
        }

        bossObject.transform.position = target;
        bossObject.transform.rotation = targetRot;
    }

    private IEnumerator BossExit()
    {
        if (bossObject == null) yield break;

        Vector3 start = bossObject.transform.position;
        Vector3 target = start + new Vector3(Screen.width * 0.02f, 0f, 0f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / rotateOutDuration;
            float ease = Mathf.Pow(Mathf.Clamp01(t), 2f);
            bossObject.transform.position = Vector3.Lerp(start, target, ease);
            bossObject.transform.rotation = Quaternion.Euler(0f, 0f, Mathf.Lerp(0f, 90f, ease));
            yield return null;
        }

        bossObject.SetActive(false);
    }

    

    private void SetLight(Pole pole)
    {
        Color c = pole.type switch
        {
            PoleType.RedPole => ColorRed,
            PoleType.BluePole => ColorBlue,
            PoleType.GreenPole => ColorGreen,
            _ => Color.white
        };

        if (eventLight1 != null) eventLight1.color = c;
        if (eventLight2 != null) eventLight2.color = c;

        if (_lightCoroutine != null) StopCoroutine(_lightCoroutine);
        _lightCoroutine = StartCoroutine(LightFadeIn());
    }

    private void DisableLight()
    {
        if (_lightCoroutine != null) StopCoroutine(_lightCoroutine);
        _lightCoroutine = StartCoroutine(LightFadeOut());
    }

    private IEnumerator LightFadeIn()
    {
        if (eventLight1 != null) eventLight1.enabled = true;
        if (eventLight2 != null) eventLight2.enabled = true;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / lightFadeInDuration;
            float intensity = Mathf.Lerp(0f, 1f, Mathf.Clamp01(t));
            if (eventLight1 != null) eventLight1.intensity = intensity;
            if (eventLight2 != null) eventLight2.intensity = intensity;
            yield return null;
        }
    }

    private IEnumerator LightFadeOut()
    {
        float startIntensity = eventLight1 != null ? eventLight1.intensity : 1f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / lightFadeOutDuration;
            float intensity = Mathf.Lerp(startIntensity, 0f, Mathf.Clamp01(t));
            if (eventLight1 != null) eventLight1.intensity = intensity;
            if (eventLight2 != null) eventLight2.intensity = intensity;
            yield return null;
        }

        if (eventLight1 != null) eventLight1.enabled = false;
        if (eventLight2 != null) eventLight2.enabled = false;
    }
}