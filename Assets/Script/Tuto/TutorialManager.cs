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
    [SerializeField] private Image nextButtonImage;
    [SerializeField] private Button backButton;

    [Header("Séquences")]
    [SerializeField] private List<TutorialStep> sequences;
    [SerializeField] private DayManager dayManager;

    [Header("Aide contextuelle (hors jour 1)")]
    [SerializeField] private TutorialStep aideDifficultySequence;
    [SerializeField] private TutorialStep aideScoreSequence;
    [SerializeField] private TutorialStep aideShopSequence;

    [Header("État de jeu (pour l'aide contextuelle)")]
    [SerializeField] private GameObject difficultyPanel;
    [SerializeField] private GameObject scorePanel;
    [SerializeField] private GameObject shopPanel;

    private List<TutorialStepData> _currentSteps;
    private int _currentStepIndex;
    private bool _isPlaying;

    private List<TutorialStepData> _lastPlayedSteps;

    private Sprite _defaultButtonSprite;
    private SpriteState _defaultSpriteState;
    private Vector2 _defaultButtonSize;
    private Vector2 _defaultButtonPosition;
    private RectTransform _nextButtonRect;

    private TutorialStep _currentSequence; // ← ajouter avec les autres champs privés

  
    public event Action<TutorialStep> TutorialEnded;


    public static event Action OnFirstDifficultyChosenStatic;
    public static event Action OnFirstPaperSentStatic;
    public static event Action OnFirstOverloadStatic;
    public static event Action OnDayEndStatic;
    public static event Action OnEmployeeFicheReachedStatic;
    public static event Action OnShopOpenedStatic;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        _nextButtonRect        = nextButton.GetComponent<RectTransform>();
        _defaultButtonSprite   = nextButtonImage.sprite;
        _defaultSpriteState    = nextButton.spriteState;
        _defaultButtonSize     = _nextButtonRect.sizeDelta;
        _defaultButtonPosition = _nextButtonRect.anchoredPosition;

        popupPanel.SetActive(false);
        backButton.gameObject.SetActive(false);

        nextButton.onClick.AddListener(OnNextClicked);
        backButton.onClick.AddListener(OnBackClicked);

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
                case TutorialTriggerType.OnFirstDifficultyChosen:
                    OnFirstDifficultyChosenStatic += () => StartCoroutine(PlayWithDelay(captured));
                    break;
                case TutorialTriggerType.OnFirstPaperSent:
                    OnFirstPaperSentStatic += () => StartCoroutine(PlayWithDelay(captured));
                    break;
                case TutorialTriggerType.OnFirstPaperProcessed:
                    TutorialEnded += (completedSequence) =>
                    {
                        // Ne se déclenche que si c'est la séquence juste avant celle-ci dans la liste
                        int thisIndex = sequences.IndexOf(captured);
                        if (thisIndex > 0 && completedSequence == sequences[thisIndex - 1])
                            StartCoroutine(PlayWithDelay(captured));
                    };
                    break;

                case TutorialTriggerType.OnFirstOverload:
                    OnFirstOverloadStatic += () => StartCoroutine(PlayWithDelay(captured));
                    break;
                case TutorialTriggerType.OnDayEnd:
                    OnDayEndStatic += () => StartCoroutine(PlayWithDelay(captured));
                    break;
                case TutorialTriggerType.OnEmployeeFicheReached:
                    OnEmployeeFicheReachedStatic += () => StartCoroutine(PlayWithDelay(captured));
                    break;
                case TutorialTriggerType.OnShopOpened:
                    OnShopOpenedStatic += () => StartCoroutine(PlayWithDelay(captured));
                    break;
            }
        }
    }

   private IEnumerator PlayWithDelay(TutorialStep sequence)
{
    if (sequence.triggerDelay > 0f)
        yield return new WaitForSecondsRealtime(sequence.triggerDelay);

    // Attend que la séquence en cours soit terminée avant de jouer la suivante
    yield return new WaitUntil(() => !_isPlaying);

    PlaySequence(sequence);
}


    /// <summary>Lance une séquence tutoriel.</summary>
    /// <summary>Lance une séquence tutoriel. Ignoré si une séquence est déjà en cours.</summary>
    public void PlaySequence(TutorialStep sequence)
    {
        if (sequence == null || sequence.steps == null || sequence.steps.Count == 0) return;
        if (_isPlaying) return;

        _currentSequence  = sequence; // ← ajouter cette ligne
        _lastPlayedSteps  = sequence.steps;
        _currentSteps     = sequence.steps;
        _currentStepIndex = 0;
        _isPlaying        = true;
        ShowCurrentStep();
    }



    /// <summary>
    /// Appelé par le bouton Aide.
    /// Jour 1 semaine 1 : rejoue la dernière séquence.
    /// Autre jour : affiche la séquence contextuelle selon l'état actif (difficulté, score, boutique).
    /// </summary>
    public void OpenAideHelp()
    {
        if (dayManager.currentDay == 1 && dayManager.currentWeek == 1)
        {
            ReplayLastSequence();
            return;
        }

        if (shopPanel != null && shopPanel.activeSelf)
        {
            PlaySequence(aideShopSequence);
            return;
        }

        if (scorePanel != null && scorePanel.activeSelf)
        {
            PlaySequence(aideScoreSequence);
            return;
        }

        if (difficultyPanel != null && difficultyPanel.activeSelf)
        {
            PlaySequence(aideDifficultySequence);
            return;
        }

        // Fallback : rejoue la dernière séquence connue
        ReplayLastSequence();
    }

    /// <summary>Rejoue la dernière séquence tutoriel depuis le début.</summary>
    public void ReplayLastSequence()
    {
        if (_lastPlayedSteps == null || _lastPlayedSteps.Count == 0) return;

        _currentSteps     = _lastPlayedSteps;
        _currentStepIndex = 0;
        _isPlaying        = true;
        ShowCurrentStep();
    }

    private void ShowCurrentStep()
    {
        TutorialStepData step = _currentSteps[_currentStepIndex];

        popupText.text = step.text;

        // Sprite normal du bouton suivant
        nextButtonImage.sprite = step.overrideButtonSprite && step.buttonSprite != null
            ? step.buttonSprite
            : _defaultButtonSprite;

        // Sprite pressé du bouton suivant
        if (step.overrideButtonSprite && step.pressedButtonSprite != null)
        {
            SpriteState state      = nextButton.spriteState;
            state.pressedSprite    = step.pressedButtonSprite;
            nextButton.spriteState = state;
        }
        else
        {
            nextButton.spriteState = _defaultSpriteState;
        }

        // Taille du bouton suivant
        _nextButtonRect.sizeDelta = step.overrideButtonSize
            ? step.buttonSize
            : _defaultButtonSize;

        // Position du bouton suivant
        _nextButtonRect.anchoredPosition = step.overrideButtonPosition
            ? step.buttonPosition
            : _defaultButtonPosition;

        // Visibilité du bouton précédent
        backButton.gameObject.SetActive(step.showBackButton && _currentStepIndex > 0);

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

    private void OnBackClicked()
    {
        if (_currentStepIndex <= 0) return;

        _currentStepIndex--;
        ShowCurrentStep();
    }

    private void ClosePopup()
    {
        popupPanel.SetActive(false);
        backButton.gameObject.SetActive(false);
        nextButtonImage.sprite           = _defaultButtonSprite;
        nextButton.spriteState           = _defaultSpriteState;
        _nextButtonRect.sizeDelta        = _defaultButtonSize;
        _nextButtonRect.anchoredPosition = _defaultButtonPosition;
        Time.timeScale                   = 1f;
        _isPlaying                       = false;

        TutorialStep justFinished = _currentSequence;
        _currentSequence = null;
        TutorialEnded?.Invoke(justFinished); // ← transmet la séquence qui vient de finir
    }


    /// <summary>Appelé depuis DifficultySlider lors du premier appui GO (jour 1, semaine 1).</summary>
    public static void NotifyFirstDifficultyChosen() => OnFirstDifficultyChosenStatic?.Invoke();

    /// <summary>Appelé depuis PaperMove lors du premier swipe.</summary>
    public static void NotifyFirstPaperSent() => OnFirstPaperSentStatic?.Invoke();

    /// <summary>Appelé depuis Employe lors du premier stun par surcharge.</summary>
    public static void NotifyFirstOverload() => OnFirstOverloadStatic?.Invoke();

    /// <summary>Appelé depuis UIScore quand l'animation de score est terminée.</summary>
    public static void NotifyDayEnd() => OnDayEndStatic?.Invoke();

    /// <summary>Appelé depuis EmployeFicheMove lors du premier swipe vers les fiches employés.</summary>
    public static void NotifyEmployeeFicheReached() => OnEmployeeFicheReachedStatic?.Invoke();

    /// <summary>Appelé depuis UiManager lors de la première ouverture de la boutique.</summary>
    public static void NotifyShopOpened() => OnShopOpenedStatic?.Invoke();
}
