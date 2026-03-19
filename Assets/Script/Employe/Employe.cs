using JetBrains.Annotations;
using NUnit.Framework;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Employe : MonoBehaviour
{
    public EmployeObject employeObject;
    [SerializeField] private PoleManager polemanager;
    [SerializeField] public Pole mypole;


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


    [Header("Ui")]
    [SerializeField] Slider workAdvancement;


    public float timeBeetwennWork = 0.2f;

    public bool iamWorking = false;




    // premier set d'identité
    public void InitialSetIdentity()
    {
        Debug.Log("EmployeSetUp");
        if (employeObject == null || employeObject.allEmploye == null || employeObject.allEmploye.Count == 0)
            return;

        if (polemanager == null || polemanager.TakenEmployeIndex == null)
            return;

        // Empêche boucle infinie
        if (polemanager.TakenEmployeIndex.Count >= employeObject.allEmploye.Count)
        {
            Debug.LogWarning("Tous les employés ont déjà été pris !");
            return;
        }

        do
        {
            employeIndex = Random.Range(0, employeObject.allEmploye.Count);
        } while (polemanager.TakenEmployeIndex.Contains(employeIndex));

        polemanager.TakenEmployeIndex.Add(employeIndex);

        SetIdentity(employeIndex, addToTaken: false);
    }


    // Second set d'identité lorsqu'on ameliore le gars
    public void SetIdentity(int index, bool addToTaken = true)
    {
        if (employeObject == null || employeObject.allEmploye == null || index < 0 || index >= employeObject.allEmploye.Count)
            return;

        if (polemanager != null && polemanager.TakenEmployeIndex != null && employeIndex>=0)
            polemanager.TakenEmployeIndex.Remove(employeIndex);

        employeIndex = index;
        var employe = employeObject.allEmploye[employeIndex];

        employeType = employe.type;
        employeName = employe.EmployeName;
        employeDescription = employe.description;
        employeFireDescription = employe.fireDescription;
        employeWorkRate = employe.workRythme;
        errorPercent = employe.errorPercent;
        timeInEntreprise = employe.timeInEntreprise;

        if (addToTaken && polemanager != null && polemanager.TakenEmployeIndex != null && !polemanager.TakenEmployeIndex.Contains(employeIndex))
            polemanager.TakenEmployeIndex.Add(employeIndex);

        Debug.Log("SetEmploye");
    }


    // fonction à lancer lorsqu'il commence a work 
    public void Working()
    {
        if (mypole.waitingPaper > 0 && iamWorking == false)
        {
            StartCoroutine(Work());
           
        }
        
        else
        {
            iamWorking = false;
            
        }
    }

    public IEnumerator Work()
    {
        iamWorking = true;
        float t = 0f;
       
        while (t < 1)
        {
            t += Time.deltaTime / employeWorkRate;
            workAdvancement.value = Mathf.Lerp(0, 1, t);
            yield return null;

        }
        float Succeed = Random.Range(0f, 1f);
        if (errorPercent > Succeed)
            mypole.WinMoney();

       
        workAdvancement.value = 0;
        Debug.Log("workDone");
        yield return new WaitForSeconds(timeBeetwennWork);
        mypole.DecrementPaper();
        iamWorking = false;
        Working();


    }

    public void SwitchPole(Pole pole)
    {
        iamWorking = false;
        mypole = pole;
        StopCoroutine(Work());

        Working();
    }


    public void Malus()
    {

    }

}
