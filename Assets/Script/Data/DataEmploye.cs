using System.Collections.Generic;
using UnityEngine;

public enum TypeOfEmployez
{
    SpeedRunner,
    Perfectionist,
    Basic,

}

[System.Serializable]
public class EmployeDataz
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


public class DataEmploye : ScriptableObject
{
    public List<EmployeDataz> allEmploye;
}
