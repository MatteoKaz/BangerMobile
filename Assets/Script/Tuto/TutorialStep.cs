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
    OnFirstDifficultyChosen,
    OnFirstPaperSent,
    OnFirstPaperProcessed,
    OnFirstOverload,
    OnDayEnd,
    OnEmployeeFicheReached,
    OnShopOpened
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
}

[CreateAssetMenu(fileName = "TutorialSequence", menuName = "Scriptable Objects/TutorialSequence")]
public class TutorialStep : ScriptableObject
{
    public TutorialTriggerType triggerType;
    public float triggerDelay;
    public List<TutorialStepData> steps;
}