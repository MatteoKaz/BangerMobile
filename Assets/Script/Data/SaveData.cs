using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int currentDay;
    public int currentWeek;
    public int playerMoney;
    public List<EmployeSaveData> employes = new List<EmployeSaveData>();
    public List<UpgradeSaveData> upgrades = new List<UpgradeSaveData>();

}

[System.Serializable]
public class AudioSaveData
{
    public float audioVolume  = 1f;
    public bool  audioIsMuted = false;
}

[System.Serializable]
public class EmployeSaveData
{
    // Identité
    public int sceneEmployeIndex;
    public int employeIndex;
    public int timeInEntreprise;

    // Placement dans les poles
    public PoleType poleType;
    public int slotIndex;

    // Stats semaine
    public int weekNumberOfPaperDone;
    public int weekSucceedPaper;
    public int weekMoneyMake;

    // Upgrades
    public float workRateBonus;
    public float errorPercentBonus;
    public float stressBonus;
    public float BonusPaperDone;
    public List<UpgradeCountData> upgradeCounts = new List<UpgradeCountData>();
}

[System.Serializable]
public class UpgradeSaveData
{
    public int upgradeIndex;
    public int currentPrice;
    
}
[System.Serializable]
public class UpgradeCountData
{
    public int upgradeIndex; // index dans shopUpgrade.allUpgrade
    public int count;
}

