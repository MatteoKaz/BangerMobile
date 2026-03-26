using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PoleLink : MonoBehaviour
{
    [SerializeField] public Pole pole;
    [SerializeField] public UpgradeSetUp upgradeSet;
    [SerializeField] public Image myCase;
    [SerializeField] public List<Image> allempImage;


    public void OnEnable()
    {
        upgradeSet.EmployeSet += MyIdentity;
    }
    public void OnDisable()
    {
        upgradeSet.EmployeSet -= MyIdentity;
    }

    public void MyIdentity()
    {


        myCase.color = Color.gray;

    }
    public void OnClick()
    {
        myCase.color = Color.white;
        foreach (var image in allempImage)
        {
            image.color = Color.gray;
        }
        upgradeSet.chosenPole(pole);
    }
}
