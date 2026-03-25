using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PoleLink : MonoBehaviour
{
    [SerializeField] public Pole pole;
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
        upgradeSet.chosenPole(pole);
    }
}
