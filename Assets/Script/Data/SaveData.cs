using System.Collections.Generic;
 
 [System.Serializable]
 public class SaveData
 {
     public int currentDay;
     public int currentWeek;
     public int playerMoney;
     public List<EmployeSaveData> employes = new List<EmployeSaveData>();
 }
 
 [System.Serializable]
 public class EmployeSaveData
 {
     // Identité
     public int sceneEmployeIndex;  // position dans le tableau Employes de SaveManager
     public int employeIndex;       // index dans EmployeObject.allEmploye
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
 }