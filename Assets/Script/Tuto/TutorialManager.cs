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
[RequireComponent(typeof(Animator))]
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

    private Vector2 _defaultPanelSize;
    private Vector2 _defaultPanelPosition;
    private RectTransform _popupPanelRect;

    private TutorialStep _currentSequence;

    private readonly HashSet<TutorialStep> _playedSequences = new HashSet<TutorialStep>();
    private const string PlayedSequencesPrefsKey = "TutorialPlayedSequences";

    // Stocke les handlers pour pouvoir les désabonner proprement dans OnDestroy
    private readonly List<(Action handler, TutorialTriggerType type)> _handlers = new List<(Action, TutorialTriggerType)>();

    public event Action<TutorialStep> TutorialEnded;

    public static event Action OnGameStartStatic;
    public static event Action OnFirstDifficultyChosenStatic;
    public static event Action OnFirstPaperSentStatic;
    public static event Action OnFirstOverloadStatic;
    public static event Action OnAllPapersSpawnedStatic;
    public static event Action OnDayEndStatic;
    public static event Action OnEmployeeFicheReachedStatic;
    public static event Action OnShopOpenedStatic;
    public static event Action OnFirstFridayScoreStatic;
    public static event Action OnFirstFridayRankingStatic;
    public static event Action OnFirstFridayShopStatic;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        _nextButtonRect      = nextButton.GetComponent<RectTransform>();
        _defaultButtonSprite = nextButtonImage.sprite;
        _defaultSpriteState  = nextButton.spriteState;
        _defaultButtonSize   = _nextButtonRect.sizeDelta;
        _defaultButtonPosition = _nextButtonRect.anchoredPosition;

        _popupPanelRect      = popupPanel.GetComponent<RectTransform>();
        _defaultPanelSize    = _popupPanelRect.sizeDelta;
        _defaultPanelPosition = _popupPanelRect.anchoredPosition;

        popupPanel.SetActive(false);
        backButton.gameObject.SetActive(false);

        nextButton.onClick.AddListener(OnNextClicked);
        backButton.onClick.AddListener(OnBackClicked);

        LoadPlayedSequences();
        SubscribeToTriggers();
    }

    private void OnDestroy()
    {
        Time.timeScale = 1f;

        // Désabonne tous les handlers pour éviter les MissingReferenceException
        foreach (var (handler, type) in _handlers)
        {
            switch (type)
            {
                case TutorialTriggerType.OnGameStart:             OnGameStartStatic             -= handler; break;
                case TutorialTriggerType.OnFirstDifficultyChosen: OnFirstDifficultyChosenStatic -= handler; break;
                case TutorialTriggerType.OnFirstPaperSent:        OnFirstPaperSentStatic        -= handler; break;
                case TutorialTriggerType.OnFirstOverload:         OnFirstOverloadStatic         -= handler; break;
                case TutorialTriggerType.OnAllPapersSpawned:      OnAllPapersSpawnedStatic      -= handler; break;
                case TutorialTriggerType.OnDayEnd:                OnDayEndStatic                -= handler; break;
                case TutorialTriggerType.OnEmployeeFicheReached:  OnEmployeeFicheReachedStatic  -= handler; break;
                case TutorialTriggerType.OnShopOpened:            OnShopOpenedStatic            -= handler; break;
                case TutorialTriggerType.OnFirstFridayScore:      OnFirstFridayScoreStatic      -= handler; break;
                case TutorialTriggerType.OnFirstFridayRanking:    OnFirstFridayRankingStatic    -= handler; break;
                case TutorialTriggerType.OnFirstFridayShop:       OnFirstFridayShopStatic       -= handler; break;
            }
        }

        _handlers.Clear();
    }

    /// <summary>Charge depuis PlayerPrefs les séquences déjà jouées lors d'une session précédente.</summary>
    private void LoadPlayedSequences()
    {
        string saved = PlayerPrefs.GetString(PlayedSequencesPrefsKey, string.Empty);
        if (string.IsNullOrEmpty(saved)) return;

        HashSet<string> playedNames = new HashSet<string>(saved.Split(','));
        foreach (TutorialStep seq in sequences)
        {
            if (playedNames.Contains(seq.name))
                _playedSequences.Add(seq);
        }
    }

    /// <summary>Réinitialise les PlayerPrefs tutoriel SANS instance (appelable depuis Awake d'autres scripts).</summary>
    public static void ResetTutorialPrefsStatic()
    {
        PlayerPrefs.DeleteKey(PlayedSequencesPrefsKey);
        PlayerPrefs.Save();
        Debug.Log("[TutorialManager] PlayerPrefs tutoriel réinitialisés (static).");
    }

    /// <summary>Réinitialise les PlayerPrefs ET l'état en mémoire. Accessible via clic droit sur le composant.</summary>
    [ContextMenu("Reset Tutorial PlayerPrefs")]
    public void ResetTutorialPrefs()
    {
        PlayerPrefs.DeleteKey(PlayedSequencesPrefsKey);
        PlayerPrefs.Save();
        _playedSequences.Clear();
        Debug.Log("[TutorialManager] PlayerPrefs tutoriel réinitialisés.");
    }

    /// <summary>Persiste dans PlayerPrefs la liste des séquences déjà jouées.</summary>
    private void SavePlayedSequences()
    {
        var names = new List<string>();
        foreach (TutorialStep seq in _playedSequences)
            names.Add(seq.name);

        PlayerPrefs.SetString(PlayedSequencesPrefsKey, string.Join(",", names));
        PlayerPrefs.Save();
    }

    private void SubscribeToTriggers()
    {
        foreach (TutorialStep seq in sequences)
        {
            TutorialStep captured = seq;

            Action handler = () =>
            {
                if (this != null)
                    StartCoroutine(PlayWithDelay(captured));
            };

            switch (seq.triggerType)
            {
                case TutorialTriggerType.OnGameStart:
                    OnGameStartStatic += handler;
                    break;
                case TutorialTriggerType.OnFirstDifficultyChosen:
                    OnFirstDifficultyChosenStatic += handler;
                    break;
                case TutorialTriggerType.OnFirstPaperSent:
                    OnFirstPaperSentStatic += handler;
                    break;
                case TutorialTriggerType.OnFirstPaperProcessed:
                    TutorialEnded += (completedSequence) =>
                    {
                        if (this == null) return;
                        int thisIndex = sequences.IndexOf(captured);
                        if (thisIndex > 0 && completedSequence == sequences[thisIndex - 1])
                            StartCoroutine(PlayWithDelay(captured));
                    };
                    break;
                case TutorialTriggerType.OnFirstOverload:
                    OnFirstOverloadStatic += handler;
                    break;
                case TutorialTriggerType.OnAllPapersSpawned:
                    OnAllPapersSpawnedStatic += handler;
                    break;
                case TutorialTriggerType.OnDayEnd:
                    OnDayEndStatic += handler;
                    break;
                case TutorialTriggerType.OnEmployeeFicheReached:
                    OnEmployeeFicheReachedStatic += handler;
                    break;
                case TutorialTriggerType.OnShopOpened:
                    OnShopOpenedStatic += handler;
                    break;
                case TutorialTriggerType.OnFirstFridayScore:
                    OnFirstFridayScoreStatic += handler;
                    break;
                case TutorialTriggerType.OnFirstFridayRanking:
                    OnFirstFridayRankingStatic += handler;
                    break;
                case TutorialTriggerType.OnFirstFridayShop:
                    OnFirstFridayShopStatic += handler;
                    break;
            }

            // OnFirstPaperProcessed utilise TutorialEnded (non statique), pas besoin de le stocker
            if (seq.triggerType != TutorialTriggerType.OnFirstPaperProcessed)
                _handlers.Add((handler, seq.triggerType));
        }
    }

    private IEnumerator PlayWithDelay(TutorialStep sequence)
    {
        if (_playedSequences.Contains(sequence))
            yield break;

        if (sequence.triggerDelay > 0f)
            yield return new WaitForSecondsRealtime(sequence.triggerDelay);

        yield return new WaitUntil(() => !_isPlaying);

        PlaySequence(sequence);
    }

    /// <summary>Lance une séquence tutoriel. Ignoré si déjà jouée ou si une séquence est en cours.</summary>
    public void PlaySequence(TutorialStep sequence)
    {
        if (sequence == null || sequence.steps == null || sequence.steps.Count == 0) return;
        if (_isPlaying) return;
        if (_playedSequences.Contains(sequence)) return;

        _currentSequence   = sequence;
        _lastPlayedSteps   = sequence.steps;
        _currentSteps      = sequence.steps;
        _currentStepIndex  = 0;
        _isPlaying         = true;
        ShowCurrentStep();
    }

    /// <summary>
    /// Appelé par le bouton Aide.
    /// Jour 1 semaine 1 : rejoue la dernière séquence.
    /// Autre jour : affiche la séquence contextuelle selon l'état actif.
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

        ReplayLastSequence();
    }

    /// <summary>Rejoue la dernière séquence tutoriel depuis le début, sans bloquer sur le HashSet.</summary>
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

        nextButtonImage.sprite = step.overrideButtonSprite && step.buttonSprite != null
            ? step.buttonSprite
            : _defaultButtonSprite;

        if (step.overrideButtonSprite && step.pressedButtonSprite != null)
        {
            SpriteState state = nextButton.spriteState;
            state.pressedSprite = step.pressedButtonSprite;
            nextButton.spriteState = state;
        }
        else
        {
            nextButton.spriteState = _defaultSpriteState;
        }

        _nextButtonRect.sizeDelta = step.overrideButtonSize
            ? step.buttonSize
            : _defaultButtonSize;

        _nextButtonRect.anchoredPosition = step.overrideButtonPosition
            ? step.buttonPosition
            : _defaultButtonPosition;

        _popupPanelRect.sizeDelta = step.overridePanelSize
            ? step.panelSize
            : _defaultPanelSize;

        _popupPanelRect.anchoredPosition = step.overridePanelPosition
            ? step.panelPosition
            : _defaultPanelPosition;

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
        nextButtonImage.sprite          = _defaultButtonSprite;
        nextButton.spriteState          = _defaultSpriteState;
        _nextButtonRect.sizeDelta       = _defaultButtonSize;
        _nextButtonRect.anchoredPosition = _defaultButtonPosition;
        _popupPanelRect.sizeDelta       = _defaultPanelSize;
        _popupPanelRect.anchoredPosition = _defaultPanelPosition;
        Time.timeScale = 1f;
        _isPlaying     = false;

        TutorialStep justFinished = _currentSequence;
        if (justFinished != null)
        {
            _playedSequences.Add(justFinished);
            SavePlayedSequences();
        }

        _currentSequence = null;
        TutorialEnded?.Invoke(justFinished);
    }

    /// <summary>Appelé depuis DayManager.LaunchFirstDayInit au lancement du premier jour.</summary>
    public static void NotifyGameStart() => OnGameStartStatic?.Invoke();

    /// <summary>Appelé depuis DifficultySlider lors du premier appui GO (jour 1, semaine 1).</summary>
    public static void NotifyFirstDifficultyChosen() => OnFirstDifficultyChosenStatic?.Invoke();

    /// <summary>Appelé depuis PaperMove lors du premier swipe.</summary>
    public static void NotifyFirstPaperSent() => OnFirstPaperSentStatic?.Invoke();

    /// <summary>Appelé depuis Employe lors du premier stun par surcharge.</summary>
    public static void NotifyFirstOverload() => OnFirstOverloadStatic?.Invoke();

    /// <summary>Appelé depuis PaperSpawner quand tous les papiers ont été spawnés.</summary>
    public static void NotifyAllPapersSpawned() => OnAllPapersSpawnedStatic?.Invoke();

    /// <summary>Appelé depuis UIScore quand l'animation de score est terminée.</summary>
    public static void NotifyDayEnd() => OnDayEndStatic?.Invoke();

    /// <summary>Appelé depuis EmployeFicheMove lors du premier swipe vers les fiches employés.</summary>
    public static void NotifyEmployeeFicheReached() => OnEmployeeFicheReachedStatic?.Invoke();

    /// <summary>Appelé depuis UiManager lors de la première ouverture de la boutique.</summary>
    public static void NotifyShopOpened() => OnShopOpenedStatic?.Invoke();

    /// <summary>Appelé depuis UiManager.EnableScore lors du premier vendredi.</summary>
    public static void NotifyFirstFridayScore() => OnFirstFridayScoreStatic?.Invoke();

    /// <summary>Appelé depuis RankingManager.SetRankingOrder lors du premier vendredi.</summary>
    public static void NotifyFirstFridayRanking() => OnFirstFridayRankingStatic?.Invoke();

    /// <summary>Appelé depuis UiManager.EnableShop lors du premier vendredi.</summary>
    public static void NotifyFirstFridayShop() => OnFirstFridayShopStatic?.Invoke();
}
