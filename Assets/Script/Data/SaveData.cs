using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int currentDay;
    public int currentWeek;
    public int playerMoney;
    public int swatUtilisation;
    public List<EmployeSaveData> employes = new List<EmployeSaveData>();
    public List<UpgradeSaveData> upgrades = new List<UpgradeSaveData>();
    public List<PoleSaveData> poles = new List<PoleSaveData>();
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
    public bool isMVP;

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
    public float employeWorkRateBonus_MV;
    public float BonusPaperDone_MVP;
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

[System.Serializable]
public class PoleSaveData
{
    public PoleType poleType;
    public float boostEmployeSpeed;
    public float boostEmployeError;
    public float bonusRevenus;
    public float boostTimeForSurcharge;
    public List<TimedUpgradeSaveData> timedUpgrades = new List<TimedUpgradeSaveData>();
    public List<UpgradeCountData> upgradeCounts = new List<UpgradeCountData>();
}


[System.Serializable]
public class TimedUpgradeSaveData
{
    public int upgradeIndex; // index dans shopUpgrade.allUpgrade pour retrouver icon/type/value
    public int daysRemaining;
}

