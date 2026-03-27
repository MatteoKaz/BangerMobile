
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;

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
    public List<Sprite> upgradesImages = new List<Sprite>();
    [SerializeField] Image[] imagesUpgrades;

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

    public void SetIcone()
    {
       
       
        for (int i = 0; i < imagesUpgrades.Length; i++)
        {
            if (i < upgradesImages.Count)
            {
                Debug.LogWarning($"upgradesImages.Count: {upgradesImages.Count}");
                imagesUpgrades[i].enabled = true;
                imagesUpgrades[i].sprite = upgradesImages[i];
               
            }
            else
            {
                imagesUpgrades[i].enabled = false; 
            }
        }
    }



}
