using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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


    public Image icone;
}

[CreateAssetMenu(fileName = "Employes", menuName = "Scriptable Objects/EmployeObject")]
public class EmployeObject : ScriptableObject
{
    public List<EmployeData> allEmploye;
}
