using System.Collections.Generic;
using UnityEngine;

public enum TypeOfEmployez
{
    SpeedRunner,
    Perfectionist,
    Basic,

}
[System.Serializable]
public class DialogueLine
{
    public string text;
    public float speed = 0.05f; // vitesse personnalisable par ligne
}
[System.Serializable]
public class EmployeDataz
{
    public string EmployeName;
    public string description;
    
    public TypeOfEmployez type;
    public string TypeText;
    public float workRythme;      // RythmeDeTravail
    public float errorPercent;
    public int timeInEntreprise;
    public Sprite typeSprite;
    public List<DialogueLine> firelines = new List<DialogueLine>();


    public Sprite icone;
    public Sprite Working;
    public Sprite Surcharge;
}


public class DataEmploye : ScriptableObject
{
    public List<EmployeDataz> allEmploye;
}
