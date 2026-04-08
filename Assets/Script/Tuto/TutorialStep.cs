using System.Collections.Generic;
using UnityEngine;

public enum TutorialStepType
{
    Info,
    Warning,
    Instruction,
    Highlight
}

public enum TutorialTriggerType
{
    OnGameStart,
    OnFirstDifficultyChosen,
    OnFirstPaperSent,
    OnFirstPaperProcessed,
    OnFirstOverload,
    OnAllPapersSpawned,
    OnDayEnd,
    OnEmployeeFicheReached,
    OnShopOpened,
    OnFirstFridayScore,
    OnFirstFridayRanking,
    OnFirstFridayShop,
    OnDifficultyPanelOpened,
    OnDifficultyPanelClosed    // ← nouveau
}

[System.Serializable]
public class TutorialStepData
{
    [TextArea(3, 6)]
    public string text;
    public TutorialStepType stepType;
    public bool pauseOnShow;
    public bool unpauseOnClose;

    [Header("Image du bouton Suivant")]
    public bool overrideButtonSprite;
    public Sprite buttonSprite;
    public Sprite pressedButtonSprite;

    [Header("Taille du bouton Suivant")]
    public bool overrideButtonSize;
    public Vector2 buttonSize;

    [Header("Position du bouton Suivant")]
    public bool overrideButtonPosition;
    public Vector2 buttonPosition;

    [Header("Bouton Précédent")]
    public bool showBackButton;

    [Header("Panel Popup")]
    public bool overridePanelSize;
    public Vector2 panelSize;

    public bool overridePanelPosition;
    public Vector2 panelPosition;
}

[CreateAssetMenu(fileName = "TutorialSequence", menuName = "Scriptable Objects/TutorialSequence")]
public class TutorialStep : ScriptableObject
{
    public TutorialTriggerType triggerType;
    public float triggerDelay;
    public List<TutorialStepData> steps;
}
