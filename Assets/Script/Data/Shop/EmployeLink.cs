
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EmployeLink : MonoBehaviour
{
    [SerializeField] public Employe myemp;
    [SerializeField] public UpgradeSetUp upgradeSet;
    [SerializeField] public Image myCase;
    [SerializeField] public TextMeshProUGUI nameUi;
    [SerializeField] public TextMeshProUGUI myTypeUi;
    [SerializeField] public TypeOfEmployez myType;
    [SerializeField] public string MyName;
    [SerializeField] public List<Image> allempImage;
    [SerializeField] Image MySlot;

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
        
        myType = myemp.employeType;
        MyName = myemp.employeName;
        nameUi.text = MyName;
        myTypeUi.text = $"Type:{myType}";
        myCase.sprite = myemp.employeImage.sprite;
        MySlot.color = Color.gray;

    }
    public void OnClick()
    {
        myCase.sprite = myemp.employeImage.sprite;
        foreach (var image in allempImage)
        {
            image.color = Color.gray;
        }
        MySlot.color = Color.white;
        upgradeSet.chosenEmploye(this);
        
    }

    public void Start()
    {
        
    }



}
