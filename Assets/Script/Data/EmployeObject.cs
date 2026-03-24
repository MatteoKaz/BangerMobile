using System.Collections.Generic;
using UnityEngine;

public enum TypeOfEmploye
{
    SpeedRunner,
    Perfectionist,
    Basic,

}

[System.Serializable]
public class EmployeData
{
    public string EmployeName;
    public string description;
    public string fireDescription;
    public TypeOfEmploye type;
    public string TypeText;
    public float workRythme;      // RythmeDeTravail
    public float errorPercent;
    public int timeInEntreprise;

    

    public Sprite icone;
}


public class EmployeObject : ScriptableObject
{
    public List<EmployeData> allEmploye;
}
