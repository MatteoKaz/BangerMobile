
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
    [SerializeField] public Image myTypeSprite;
    [SerializeField] public TypeOfEmployez myType;
    [SerializeField] public string MyName;
    [SerializeField] public List<Image> allempImage;
    [SerializeField] Image MySlot;
    public List<Sprite> upgradesImages = new List<Sprite>();
    [SerializeField] public Image[] imagesUpgrades;
    public Dictionary<int, int> upgradeCounts = new Dictionary<int, int>();
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
        
       
        MyName = myemp.employeName;
        nameUi.text = MyName;
        myTypeSprite.sprite = myemp.typeImage.sprite;
        myCase.sprite = myemp.employeImage.sprite;
        MySlot.color = Color.gray;
        upgradesImages = new List<Sprite>(myemp.upgradesImages);
        SetIcone();

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
