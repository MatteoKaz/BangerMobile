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
    OnFirstDayStart,       // Tutos 1 & 2 — DayManager.FirstDayInitialization
    OnFirstPaperSent,      // Tuto 3 — premier swipe vers un tuyau
    OnFirstPaperProcessed, // Tuto 4 — délai après tuto 3
    OnFirstOverload,       // Tuto 5 — premier seuil de surcharge atteint
    OnDayEnd               // Tuto 6 — DayManager.DayEnd
}

[System.Serializable]
public class TutorialStepData
{
    [TextArea(3, 6)]
    public string text;
    public TutorialStepType stepType;
    public bool pauseOnShow;
    public bool unpauseOnClose;
}

[CreateAssetMenu(fileName = "TutorialSequence", menuName = "Scriptable Objects/TutorialSequence")]
public class TutorialStep : ScriptableObject
{
    public TutorialTriggerType triggerType;
    public float triggerDelay;
    public List<TutorialStepData> steps;
}