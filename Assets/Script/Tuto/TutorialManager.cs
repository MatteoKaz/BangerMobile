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
        {
            Instance = this;
            Debug.Log("[TutorialManager] Instance initialisée.");
        }
        else
        {
            Debug.LogWarning("[TutorialManager] Instance dupliquée détruite.");
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        _nextButtonRect        = nextButton.GetComponent<RectTransform>();
        _defaultButtonSprite   = nextButtonImage.sprite;
        _defaultSpriteState    = nextButton.spriteState;
        _defaultButtonSize     = _nextButtonRect.sizeDelta;
        _defaultButtonPosition = _nextButtonRect.anchoredPosition;

        _popupPanelRect       = popupPanel.GetComponent<RectTransform>();
        _defaultPanelSize     = _popupPanelRect.sizeDelta;
        _defaultPanelPosition = _popupPanelRect.anchoredPosition;

        popupPanel.SetActive(false);
        backButton.gameObject.SetActive(false);

        nextButton.onClick.AddListener(OnNextClicked);
        backButton.onClick.AddListener(OnBackClicked);

        Debug.Log($"[TutorialManager] Start — {sequences?.Count ?? 0} séquences chargées.");

        if (difficultyPanel == null) Debug.LogWarning("[TutorialManager] difficultyPanel non assigné.");
        if (scorePanel == null)      Debug.LogWarning("[TutorialManager] scorePanel non assigné.");
        if (shopPanel == null)       Debug.LogWarning("[TutorialManager] shopPanel non assigné.");
        if (dayManager == null)      Debug.LogError("[TutorialManager] dayManager non assigné !");

        LoadPlayedSequences();
        SubscribeToTriggers();
    }

    private void OnDestroy()
    {
        Debug.Log("[TutorialManager] OnDestroy — désabonnement des handlers.");
        Time.timeScale = 1f;

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
        if (string.IsNullOrEmpty(saved))
        {
            Debug.Log("[TutorialManager] LoadPlayedSequences — Aucune séquence sauvegardée.");
            return;
        }

        HashSet<string> playedNames = new HashSet<string>(saved.Split(','));
        foreach (TutorialStep seq in sequences)
        {
            if (playedNames.Contains(seq.name))
            {
                _playedSequences.Add(seq);
                Debug.Log($"[TutorialManager] LoadPlayedSequences — Séquence déjà jouée restaurée : {seq.name}");
            }
        }

        Debug.Log($"[TutorialManager] LoadPlayedSequences — {_playedSequences.Count} séquence(s) marquées comme jouées.");
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
        Debug.Log($"[TutorialManager] SavePlayedSequences — Sauvegardé : {string.Join(", ", names)}");
    }

    private void SubscribeToTriggers()
    {
        Debug.Log("[TutorialManager] SubscribeToTriggers — Abonnement aux triggers.");

        foreach (TutorialStep seq in sequences)
        {
            TutorialStep captured = seq;

            Action handler = () =>
            {
                if (this != null)
                {
                    Debug.Log($"[TutorialManager] Trigger reçu pour : {captured.name} (type : {captured.triggerType})");
                    StartCoroutine(PlayWithDelay(captured));
                }
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
                        {
                            Debug.Log($"[TutorialManager] OnFirstPaperProcessed déclenché après : {completedSequence?.name}");
                            StartCoroutine(PlayWithDelay(captured));
                        }
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

            Debug.Log($"[TutorialManager] SubscribeToTriggers — {seq.name} abonné à {seq.triggerType}");

            if (seq.triggerType != TutorialTriggerType.OnFirstPaperProcessed)
                _handlers.Add((handler, seq.triggerType));
        }
    }

    private IEnumerator PlayWithDelay(TutorialStep sequence)
    {
        if (_playedSequences.Contains(sequence))
        {
            Debug.Log($"[TutorialManager] PlayWithDelay — {sequence.name} déjà jouée, ignorée.");
            yield break;
        }

        if (sequence.triggerDelay > 0f)
        {
            Debug.Log($"[TutorialManager] PlayWithDelay — Attente de {sequence.triggerDelay}s avant {sequence.name}");
            yield return new WaitForSecondsRealtime(sequence.triggerDelay);
        }

        if (_isPlaying)
        {
            Debug.Log($"[TutorialManager] PlayWithDelay — {sequence.name} en attente (une séquence est déjà en cours).");
            yield return new WaitUntil(() => !_isPlaying);
        }

        Debug.Log($"[TutorialManager] PlayWithDelay — Lancement de {sequence.name}");
        PlaySequence(sequence);
    }

    /// <summary>Lance une séquence tutoriel. Ignoré si déjà jouée ou si une séquence est en cours.</summary>
    public void PlaySequence(TutorialStep sequence)
    {
        if (sequence == null || sequence.steps == null || sequence.steps.Count == 0)
        {
            Debug.LogWarning("[TutorialManager] PlaySequence — Séquence nulle ou sans steps.");
            return;
        }
        if (_isPlaying)
        {
            Debug.LogWarning($"[TutorialManager] PlaySequence — {sequence.name} ignorée, une séquence est déjà en cours.");
            return;
        }
        if (_playedSequences.Contains(sequence))
        {
            Debug.Log($"[TutorialManager] PlaySequence — {sequence.name} déjà jouée, ignorée.");
            return;
        }

        Debug.Log($"[TutorialManager] PlaySequence — Démarrage de {sequence.name} ({sequence.steps.Count} steps)");
        _currentSequence  = sequence;
        _lastPlayedSteps  = sequence.steps;
        _currentSteps     = sequence.steps;
        _currentStepIndex = 0;
        _isPlaying        = true;
        ShowCurrentStep();
    }

    /// <summary>
    /// Appelé par le bouton Aide.
    /// Jour 1 semaine 1 : rejoue la dernière séquence.
    /// Hors jour 1 : trouve la séquence contextuelle dans la liste sequences
    /// selon l'état actif du jeu (panel ouvert), puis la force en replay.
    /// Repli sur ReplayLastSequence si aucun panel n'est détecté.
    /// </summary>
    public void OpenAideHelp()
    {
        Debug.Log($"[TutorialManager] OpenAideHelp — Jour {dayManager.currentDay} Semaine {dayManager.currentWeek}");

        if (dayManager.currentDay == 1 && dayManager.currentWeek == 1)
        {
            Debug.Log("[TutorialManager] OpenAideHelp — Jour 1 S1 : ReplayLastSequence");
            ReplayLastSequence();
            return;
        }

        Debug.Log($"[TutorialManager] OpenAideHelp — shopPanel : {(shopPanel != null ? shopPanel.activeSelf.ToString() : "NULL")} | scorePanel : {(scorePanel != null ? scorePanel.activeSelf.ToString() : "NULL")} | difficultyPanel : {(difficultyPanel != null ? difficultyPanel.activeSelf.ToString() : "NULL")}");

        TutorialStep found = null;

        if (shopPanel != null && shopPanel.activeSelf)
            found = FindSequenceByTrigger(TutorialTriggerType.OnShopOpened,
                                          TutorialTriggerType.OnFirstFridayShop);
        else if (scorePanel != null && scorePanel.activeSelf)
            found = FindSequenceByTrigger(TutorialTriggerType.OnDayEnd,
                                          TutorialTriggerType.OnFirstFridayScore,
                                          TutorialTriggerType.OnFirstFridayRanking);
        else if (difficultyPanel != null && difficultyPanel.activeSelf)
            found = FindSequenceByTrigger(TutorialTriggerType.OnFirstDifficultyChosen);

        if (found != null)
        {
            Debug.Log($"[TutorialManager] OpenAideHelp — Séquence contextuelle trouvée : {found.name}");
            ReplaySequence(found);
        }
        else
        {
            Debug.Log("[TutorialManager] OpenAideHelp — Aucun panel actif détecté, fallback ReplayLastSequence.");
            ReplayLastSequence();
        }
    }

    /// <summary>
    /// Cherche dans sequences la première séquence dont le triggerType
    /// correspond à l'un des types fournis (priorité dans l'ordre des arguments).
    /// </summary>
    private TutorialStep FindSequenceByTrigger(params TutorialTriggerType[] types)
    {
        foreach (TutorialTriggerType type in types)
        {
            TutorialStep match = sequences.Find(s => s.triggerType == type);
            Debug.Log($"[TutorialManager] FindSequenceByTrigger — Recherche {type} : {(match != null ? match.name : "non trouvé")}");
            if (match != null) return match;
        }
        return null;
    }

    /// <summary>
    /// Rejoue une séquence spécifique depuis le début, sans vérification
    /// du HashSet des séquences déjà jouées (usage bouton aide).
    /// </summary>
    public void ReplaySequence(TutorialStep sequence)
    {
        if (sequence == null || sequence.steps == null || sequence.steps.Count == 0)
        {
            Debug.LogWarning("[TutorialManager] ReplaySequence — Séquence nulle ou vide.");
            return;
        }

        Debug.Log($"[TutorialManager] ReplaySequence — Lecture forcée de : {sequence.name} ({sequence.steps.Count} steps)");
        _currentSequence  = sequence;
        _lastPlayedSteps  = sequence.steps;
        _currentSteps     = sequence.steps;
        _currentStepIndex = 0;
        _isPlaying        = true;
        ShowCurrentStep();
    }

    /// <summary>Rejoue la dernière séquence tutoriel depuis le début, sans bloquer sur le HashSet.</summary>
    public void ReplayLastSequence()
    {
        if (_lastPlayedSteps == null || _lastPlayedSteps.Count == 0)
        {
            Debug.LogWarning("[TutorialManager] ReplayLastSequence — Aucune séquence précédente en mémoire.");
            return;
        }

        Debug.Log($"[TutorialManager] ReplayLastSequence — Rejoue {_lastPlayedSteps.Count} steps.");
        _currentSteps     = _lastPlayedSteps;
        _currentStepIndex = 0;
        _isPlaying        = true;
        ShowCurrentStep();
    }

    private void ShowCurrentStep()
    {
        TutorialStepData step = _currentSteps[_currentStepIndex];
        Debug.Log($"[TutorialManager] ShowCurrentStep — Step {_currentStepIndex + 1}/{_currentSteps.Count} : \"{step.text}\"");

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
        {
            Debug.Log("[TutorialManager] ShowCurrentStep — Jeu mis en pause.");
            Time.timeScale = 0f;
        }
    }

    private void OnNextClicked()
    {
        TutorialStepData currentStep = _currentSteps[_currentStepIndex];
        Debug.Log($"[TutorialManager] OnNextClicked — Step {_currentStepIndex + 1}/{_currentSteps.Count}");

        if (currentStep.unpauseOnClose)
        {
            Debug.Log("[TutorialManager] OnNextClicked — Jeu remis en marche.");
            Time.timeScale = 1f;
        }

        _currentStepIndex++;

        if (_currentStepIndex < _currentSteps.Count)
            ShowCurrentStep();
        else
            ClosePopup();
    }

    private void OnBackClicked()
    {
        if (_currentStepIndex <= 0)
        {
            Debug.LogWarning("[TutorialManager] OnBackClicked — Déjà au premier step.");
            return;
        }

        Debug.Log($"[TutorialManager] OnBackClicked — Retour au step {_currentStepIndex}");
        _currentStepIndex--;
        ShowCurrentStep();
    }

    private void ClosePopup()
    {
        Debug.Log($"[TutorialManager] ClosePopup — Fermeture de la séquence : {_currentSequence?.name ?? "inconnue"}");

        popupPanel.SetActive(false);
        backButton.gameObject.SetActive(false);
        nextButtonImage.sprite           = _defaultButtonSprite;
        nextButton.spriteState           = _defaultSpriteState;
        _nextButtonRect.sizeDelta        = _defaultButtonSize;
        _nextButtonRect.anchoredPosition = _defaultButtonPosition;
        _popupPanelRect.sizeDelta        = _defaultPanelSize;
        _popupPanelRect.anchoredPosition = _defaultPanelPosition;
        Time.timeScale = 1f;
        _isPlaying     = false;

        TutorialStep justFinished = _currentSequence;
        if (justFinished != null)
        {
            _playedSequences.Add(justFinished);
            SavePlayedSequences();
            Debug.Log($"[TutorialManager] ClosePopup — {justFinished.name} marquée comme jouée.");
        }

        _currentSequence = null;
        TutorialEnded?.Invoke(justFinished);
    }

    /// <summary>Appelé depuis DayManager.LaunchFirstDayInit au lancement du premier jour.</summary>
    public static void NotifyGameStart()
    {
        Debug.Log("[TutorialManager] NotifyGameStart");
        OnGameStartStatic?.Invoke();
    }

    /// <summary>Appelé depuis DifficultySlider lors du premier appui GO (jour 1, semaine 1).</summary>
    public static void NotifyFirstDifficultyChosen()
    {
        Debug.Log("[TutorialManager] NotifyFirstDifficultyChosen");
        OnFirstDifficultyChosenStatic?.Invoke();
    }

    /// <summary>Appelé depuis PaperMove lors du premier swipe.</summary>
    public static void NotifyFirstPaperSent()
    {
        Debug.Log("[TutorialManager] NotifyFirstPaperSent");
        OnFirstPaperSentStatic?.Invoke();
    }

    /// <summary>Appelé depuis Employe lors du premier stun par surcharge.</summary>
    public static void NotifyFirstOverload()
    {
        Debug.Log("[TutorialManager] NotifyFirstOverload");
        OnFirstOverloadStatic?.Invoke();
    }

    /// <summary>Appelé depuis PaperSpawner quand tous les papiers ont été spawnés.</summary>
    public static void NotifyAllPapersSpawned()
    {
        Debug.Log("[TutorialManager] NotifyAllPapersSpawned");
        OnAllPapersSpawnedStatic?.Invoke();
    }

    /// <summary>Appelé depuis UIScore quand l'animation de score est terminée.</summary>
    public static void NotifyDayEnd()
    {
        Debug.Log("[TutorialManager] NotifyDayEnd");
        OnDayEndStatic?.Invoke();
    }

    /// <summary>Appelé depuis EmployeFicheMove lors du premier swipe vers les fiches employés.</summary>
    public static void NotifyEmployeeFicheReached()
    {
        Debug.Log("[TutorialManager] NotifyEmployeeFicheReached");
        OnEmployeeFicheReachedStatic?.Invoke();
    }

    /// <summary>Appelé depuis UiManager lors de la première ouverture de la boutique.</summary>
    public static void NotifyShopOpened()
    {
        Debug.Log("[TutorialManager] NotifyShopOpened");
        OnShopOpenedStatic?.Invoke();
    }

    /// <summary>Appelé depuis UiManager.EnableScore lors du premier vendredi.</summary>
    public static void NotifyFirstFridayScore()
    {
        Debug.Log("[TutorialManager] NotifyFirstFridayScore");
        OnFirstFridayScoreStatic?.Invoke();
    }

    /// <summary>Appelé depuis RankingManager.SetRankingOrder lors du premier vendredi.</summary>
    public static void NotifyFirstFridayRanking()
    {
        Debug.Log("[TutorialManager] NotifyFirstFridayRanking");
        OnFirstFridayRankingStatic?.Invoke();
    }

    /// <summary>Appelé depuis UiManager.EnableShop lors du premier vendredi.</summary>
    public static void NotifyFirstFridayShop()
    {
        Debug.Log("[TutorialManager] NotifyFirstFridayShop");
        OnFirstFridayShopStatic?.Invoke();
    }
}
