using NUnit.Framework;
using UnityEngine;

public class Employe : MonoBehaviour
{
    public EmployeObject employeObject;

    [Header("IdentitéPersoValue")]
    public TypeOfEmploye employeType;
    public string employeName;
    public string employeDescription;
    public string employeFireDescription;
    public float employeWorkRate;
    public float errorPercent;
    public int timeInEntreprise;
    public Sprite employeSprite;
    public int employeIndex;


    [Header("Upgrade modifier")]
    public float employeWorkRateBonus;
    public float employeErrorPercenBonus;
    public float StressBonus;





    public void InitialSetIdentity()
    {
        if (employeObject != null)
        {
            employeIndex = Random.Range(0, employeObject.allEmploye.Count);
            employeType = employeObject.allEmploye[employeIndex].type;
            employeName = employeObject.allEmploye[employeIndex].EmployeName;
            employeDescription = employeObject.allEmploye[employeIndex].description;
            employeFireDescription = employeObject.allEmploye[employeIndex].fireDescription;
            employeWorkRate = employeObject.allEmploye[employeIndex].workRythme;
            errorPercent  =  employeObject.allEmploye[employeIndex].errorPercent;
            timeInEntreprise = employeObject.allEmploye[employeIndex].timeInEntreprise;


        }
    }
        

     public void SetIdentity(int index)
    {
        employeIndex = index;
        employeType = employeObject.allEmploye[employeIndex].type;
        employeName = employeObject.allEmploye[employeIndex].EmployeName;
        employeDescription = employeObject.allEmploye[employeIndex].description;
        employeFireDescription = employeObject.allEmploye[employeIndex].fireDescription;
        employeWorkRate = employeObject.allEmploye[employeIndex].workRythme;
        errorPercent = employeObject.allEmploye[employeIndex].errorPercent;
        timeInEntreprise = employeObject.allEmploye[employeIndex].timeInEntreprise;

    }

    public void Working()
    {

    }

}
