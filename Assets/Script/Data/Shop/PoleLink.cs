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
    public List<Sprite> upgradesImages = new List<Sprite>();
    [SerializeField] public Image[] imagesUpgrades;

    public void OnEnable()
    {
        upgradeSet.PoleSet += MyIdentity;
        
    }
    public void OnDisable()
    {
        upgradeSet.PoleSet -= MyIdentity;
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
        upgradeSet.chosenPole(this);
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
