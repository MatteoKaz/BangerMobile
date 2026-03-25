using System.Collections.Generic;
using UnityEngine;

public enum TypeOfUpgrade
{
    BoostSpeed,
    BoostErrorRate,
    BoostSurchargeResistance,
    PrimePole,
    CigarettePole,
    

}
public enum CategoryUpgrade
{
    Employe,
    Pole,
    Usable,
}

[System.Serializable]
public class UpgradeData
{
    public string UpgradeName;
    public int price ;
    public TypeOfUpgrade type;
    public CategoryUpgrade category;
    public int upgradeValue;
    public Sprite icone;
    public int inflation;
    public string Description;
    public int duration;
}

[CreateAssetMenu(fileName = "Upgrades", menuName = "Scriptable Objects/ShopUpgrade")]
public class ShopUpgrade : ScriptableObject
{
    public List<UpgradeData> allUpgrade;
}

