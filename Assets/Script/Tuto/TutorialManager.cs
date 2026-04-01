using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gère l'affichage des pop-ups tutoriel et la mise en pause associée.
/// Reçoit une liste de TutorialStep et s'abonne automatiquement aux bons déclencheurs.
/// </summary>
public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private TextMeshProUGUI popupText;
    [SerializeField] private Button nextButton;

    [Header("Séquences")]
    [SerializeField] private List<TutorialStep> sequences;
    [SerializeField] private DayManager dayManager;

    private List<TutorialStepData> _currentSteps;
    private int _currentStepIndex;
    private bool _isPlaying;

    public event Action TutorialEnded;

    public static event Action OnFirstPaperSentStatic;
    public static event Action OnFirstOverloadStatic;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        popupPanel.SetActive(false);
        nextButton.onClick.AddListener(OnNextClicked);
        SubscribeToTriggers();
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;
    }

    private void SubscribeToTriggers()
    {
        foreach (TutorialStep seq in sequences)
        {
            TutorialStep captured = seq;
            switch (seq.triggerType)
            {
                case TutorialTriggerType.OnFirstDayStart:
                    dayManager.FirstDayInitialization += () => StartCoroutine(PlayWithDelay(captured));
                    break;
                case TutorialTriggerType.OnFirstPaperSent:
                    OnFirstPaperSentStatic += () => StartCoroutine(PlayWithDelay(captured));
                    break;
                case TutorialTriggerType.OnFirstPaperProcessed:
                    TutorialEnded += () => StartCoroutine(PlayWithDelay(captured));
                    break;
                case TutorialTriggerType.OnFirstOverload:
                    OnFirstOverloadStatic += () => StartCoroutine(PlayWithDelay(captured));
                    break;
                case TutorialTriggerType.OnDayEnd:
                    dayManager.DayEnd += () => StartCoroutine(PlayWithDelay(captured));
                    break;
            }
        }
    }

    private IEnumerator PlayWithDelay(TutorialStep sequence)
    {
        if (sequence.triggerDelay > 0f)
            yield return new WaitForSecondsRealtime(sequence.triggerDelay);

        PlaySequence(sequence);
    }

    /// <summary>Lance une séquence tutoriel.</summary>
    public void PlaySequence(TutorialStep sequence)
    {
        if (sequence == null || sequence.steps == null || sequence.steps.Count == 0) return;

        _currentSteps = sequence.steps;
        _currentStepIndex = 0;
        _isPlaying = true;
        ShowCurrentStep();
    }

    private void ShowCurrentStep()
    {
        TutorialStepData step = _currentSteps[_currentStepIndex];
        popupText.text = step.text;
        popupPanel.SetActive(true);

        if (step.pauseOnShow)
            Time.timeScale = 0f;
    }

    private void OnNextClicked()
    {
        TutorialStepData currentStep = _currentSteps[_currentStepIndex];

        if (currentStep.unpauseOnClose)
            Time.timeScale = 1f;

        _currentStepIndex++;

        if (_currentStepIndex < _currentSteps.Count)
            ShowCurrentStep();
        else
            ClosePopup();
    }

    private void ClosePopup()
    {
        popupPanel.SetActive(false);
        Time.timeScale = 1f;
        _isPlaying = false;
        TutorialEnded?.Invoke();
    }

    /// <summary>Appelé depuis PaperMove lors du premier swipe.</summary>
    public static void NotifyFirstPaperSent() => OnFirstPaperSentStatic?.Invoke();

    /// <summary>Appelé depuis Pole lors du premier seuil de surcharge.</summary>
    public static void NotifyFirstOverload() => OnFirstOverloadStatic?.Invoke();
}
