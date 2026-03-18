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
    public TypeOfEmploye type;
    public float workRythme;      // RythmeDeTravail
    public float errorPercent;   
    public Sprite icone;
}

[CreateAssetMenu(fileName = "EmployeData", menuName = "Scriptable Objects/EmployeObject")]
public class EmployeObject : ScriptableObject
{
    public List<EmployeData> allEmploye;
}
