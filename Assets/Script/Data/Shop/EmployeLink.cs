
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EmployeLink : MonoBehaviour
{
    [SerializeField] public Employe myemp;
    [SerializeField] public UpgradeSetUp upgradeSet;
    [SerializeField] public Image myCase;
    [SerializeField] public List<Image> allempImage;

    public void OnClick()
    {
        myCase.color = Color.white;
        foreach (var image in allempImage)
        {
            image.color = Color.gray;
        }
        upgradeSet.chosenEmploye(this);
        
    }

    public void Start()
    {
        
    }



}
