// Assets/Script/Data/TutorialSequenceData.cs
using UnityEngine;

[CreateAssetMenu(fileName = "TutorialSequence", menuName = "Tutorial/Sequence")]
public class TutorialSequenceData : ScriptableObject
{
    public TutorialStepData[] steps;
}