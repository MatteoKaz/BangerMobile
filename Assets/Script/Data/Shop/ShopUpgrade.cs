using System.Collections.Generic;
using UnityEngine;

public enum TypeOfUpgrade
{
    BoostSpeed,
    BoostEfficiency,
    BoostSurchargeResistance,
    

}

[System.Serializable]
public class UpgradeData
{
    public string UpgradeName;
    public int price ;
    public TypeOfUpgrade type;
    public string upgradeValue;
    public Sprite icone;
    public int inflation;
    public string Description;
}

[CreateAssetMenu(fileName = "Upgrades", menuName = "Scriptable Objects/ShopUpgrade")]
public class ShopUpgrade : ScriptableObject
{
    public List<UpgradeData> allUpgrade;
}

