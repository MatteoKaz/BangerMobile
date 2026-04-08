using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Sauvegarde et charge l'état du jeu à chaque début de nouvelle journée.
/// Poles et employés découverts automatiquement dans la scène.
/// Les options audio sont gérées indépendamment par AudioManager.
/// Réinitialise les prefs tutoriel si le jeu est lancé au lundi semaine 1.
/// </summary>
public class SaveManager : MonoBehaviour
{
    private const string SaveFileName = "save.json";

    [SerializeField] private DayManager dayManager;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private ShopUpgrade shopUpgrade;
    [SerializeField] private PoleLink[] _poleLinks;
    [SerializeField] private SwatManager swatManager;

    private Pole[] _poles;
    private Employe[] _employes;
    private bool _initialized = false;
    private bool _shouldRelaunchTutorial = false;
    private int[] _basePrices;

    private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    private void Awake()
    {
        
        

        GatherReferences();
        CheckAndResetTutorialIfMonday();
    }

    private void OnEnable()
    {
        dayManager.DayBegin += SaveGame;
    }

    private void OnDisable()
    {
        dayManager.DayBegin -= SaveGame;
    }

    private void Start()
    {
        if (HasSave())
        {
            LoadGame();

            if (_shouldRelaunchTutorial)
                StartCoroutine(RelaunchTutorialNextFrame());
        }
        else
        {
            InitNewGame();
        }

        _initialized = true;
    }

    // ── Tutoriel ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Vérifie si le jeu démarre sans save (nouvelle partie)
    /// et réinitialise les prefs tutoriel uniquement dans ce cas.
    /// Une save existante n'est jamais touchée, même au jour 1 semaine 1.
    /// </summary>
    private void CheckAndResetTutorialIfMonday()
    {
        if (HasSave())
        {
            SaveData peekData = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));

            if (peekData.currentDay == 1 && peekData.currentWeek == 1)
            {
                // Save au jour 1 semaine 1 : on relance l'init du premier jour
                // mais on ne touche PAS aux prefs tutoriel déjà enregistrés.
                _shouldRelaunchTutorial = true;
                Debug.Log("[SaveManager] Lundi S1 détecté (save) → init premier jour relancée sans reset tutoriel.");
            }
            else
            {
                dayManager.skipFirstLaunch = true;
            }
        }
        else
        {
            // Nouvelle partie uniquement : reset les prefs tutoriel.
            TutorialManager.ResetTutorialPrefsStatic();
            Debug.Log("[SaveManager] Nouvelle partie → prefs tutoriel réinitialisés.");
        }
    }


    /// <summary>
    /// Attend deux frames pour laisser tous les Start() s'exécuter
    /// avant de relancer l'initialisation du premier jour.
    /// </summary>
    private IEnumerator RelaunchTutorialNextFrame()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        StartCoroutine(dayManager.LaunchFirstDayInit());
    }

    // ── Initialisation ────────────────────────────────────────────────────────

    /// <summary>Initialise une toute nouvelle partie sans save.</summary>
    private void InitNewGame()
    {
        // Réservé pour toute initialisation spécifique à une nouvelle partie.
    }

    /// <summary>Découvre automatiquement tous les Pole et Employe de la scène.</summary>
    private void GatherReferences()
    {
        Pole[] foundPoles = FindObjectsByType<Pole>(FindObjectsSortMode.None);
        System.Array.Sort(foundPoles, (a, b) =>
            string.Compare(GetScenePath(a.transform), GetScenePath(b.transform)));
        _poles = foundPoles;

        Employe[] foundEmployes = FindObjectsByType<Employe>(FindObjectsSortMode.None);
        System.Array.Sort(foundEmployes, (a, b) =>
            string.Compare(GetScenePath(a.transform), GetScenePath(b.transform)));
        _employes = foundEmployes;

        Debug.Log($"[SaveManager] {_poles.Length} poles, {_employes.Length} employés trouvés.");
    }

    // ── Save ──────────────────────────────────────────────────────────────────

    /// <summary>Sauvegarde l'état complet du jeu dans un fichier JSON.</summary>
    public void SaveGame()
    {
        if (!_initialized) return;

        SaveData data = new SaveData
        {
            currentDay = dayManager.currentDay,
            currentWeek = dayManager.currentWeek,
            playerMoney = scoreManager.playerMoney
        };

        for (int i = 0; i < _employes.Length; i++)
        {
            Employe emp = _employes[i];
            if (emp == null) continue;

            InventorySlot slot = FindSlotOfEmploye(emp);

            EmployeSaveData empSaveData = new EmployeSaveData
            {
                sceneEmployeIndex = i,
                employeIndex = emp.employeIndex,
                timeInEntreprise = emp.timeInEntreprise,
                poleType = emp.mypole != null ? emp.mypole.type : PoleType.RedPole,
                slotIndex = slot != null ? slot.slotIndex : 0,
                weekNumberOfPaperDone = emp.WeeknumberOfPaperDone,
                weekSucceedPaper = emp.WeeksucceedPaper,
                weekMoneyMake = emp.WeekmoneyMake,
                workRateBonus = emp.employeWorkRateBonus,
                errorPercentBonus = emp.employeErrorPercenBonus,
                stressBonus = emp.StressBonus,
                isMVP = emp.couronne != null && emp.couronne.activeSelf,
                BonusPaperDone = emp.BonusPaperDone_Shop,
                employeWorkRateBonus_MV = emp.employeWorkRateBonus_MVP,
                BonusPaperDone_MVP = emp.BonusPaperDone_MVP
            };

            foreach (var kvp in emp.upgradeCounts)
            {
                for (int j = 0; j < shopUpgrade.allUpgrade.Count; j++)
                {
                    if (shopUpgrade.allUpgrade[j].icone == kvp.Key)
                    {
                        empSaveData.upgradeCounts.Add(new UpgradeCountData
                        {
                            upgradeIndex = j,
                            count = kvp.Value
                        });
                        break;
                    }
                }
            }

            data.employes.Add(empSaveData);
        }

        foreach (PoleLink poleLink in _poleLinks)
        {
            PoleSaveData poleSaveData = new PoleSaveData
            {
                poleType = poleLink.pole.type,
                boostEmployeSpeed = poleLink.pole.BoostEmployeSpeed,
                boostEmployeError = poleLink.pole.BoostEmployeError,
                bonusRevenus = poleLink.pole.BonusRevenus,
                boostTimeForSurcharge = poleLink.pole.BoostTimeForSurcharge
            };

            foreach (var t in poleLink.timedUpgrades)
            {
                for (int j = 0; j < shopUpgrade.allUpgrade.Count; j++)
                {
                    if (shopUpgrade.allUpgrade[j].icone == t.icon)
                    {
                        poleSaveData.timedUpgrades.Add(new TimedUpgradeSaveData
                        {
                            upgradeIndex = j,
                            daysRemaining = t.daysRemaining
                        });
                        break;
                    }
                }
            }

            foreach (var kvp in poleLink.pole.upgradeCounts)
            {
                for (int j = 0; j < shopUpgrade.allUpgrade.Count; j++)
                {
                    if (shopUpgrade.allUpgrade[j].icone == kvp.Key)
                    {
                        poleSaveData.upgradeCounts.Add(new UpgradeCountData
                        {
                            upgradeIndex = j,
                            count = kvp.Value
                        });
                        break;
                    }
                }
            }

            data.poles.Add(poleSaveData);
        }

        data.swatUtilisation = swatManager.numberOfUtilisation;

        for (int i = 0; i < shopUpgrade.allUpgrade.Count; i++)
        {
            data.upgrades.Add(new UpgradeSaveData
            {
                upgradeIndex = i,
                currentPrice = shopUpgrade.allUpgrade[i].price
            });
        }

        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
        Debug.Log($"[SaveManager] Sauvegarde : Jour {data.currentDay} — Semaine {data.currentWeek}");
    }

    // ── Load ──────────────────────────────────────────────────────────────────

    /// <summary>Charge la sauvegarde et restaure l'état complet du jeu.</summary>
    public void LoadGame()
    {
        if (!HasSave()) return;

        SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));

        dayManager.RestoreDay(data.currentDay, data.currentWeek);
        scoreManager.playerMoney = data.playerMoney;

        foreach (EmployeSaveData empData in data.employes)
        {
            if (empData.sceneEmployeIndex < 0 || empData.sceneEmployeIndex >= _employes.Length)
                continue;

            Employe emp = _employes[empData.sceneEmployeIndex];
            if (emp == null) continue;

            emp.SetIdentity(empData.employeIndex);
            emp.timeInEntreprise = empData.timeInEntreprise;
            emp.WeeknumberOfPaperDone = empData.weekNumberOfPaperDone;
            emp.WeeksucceedPaper = empData.weekSucceedPaper;
            emp.WeekmoneyMake = empData.weekMoneyMake;
            emp.employeWorkRateBonus = empData.workRateBonus;
            emp.employeErrorPercenBonus = empData.errorPercentBonus;
            emp.StressBonus = empData.stressBonus;
            emp.BonusPaperDone_Shop = empData.BonusPaperDone;
            emp.employeWorkRateBonus_MVP = empData.employeWorkRateBonus_MV;
            emp.BonusPaperDone_MVP = empData.BonusPaperDone_MVP;

            if (emp.couronne != null)
                emp.couronne.SetActive(empData.isMVP);

            InventorySlot targetSlot = FindSlot(empData.poleType, empData.slotIndex);
            if (targetSlot == null) continue;

            DraggableItems draggable = FindDraggableForEmploye(emp);
            if (draggable == null) continue;

            draggable.transform.SetParent(targetSlot.transform, false);
            draggable.transform.localPosition = Vector3.zero;
            draggable.parentAfterDrag = targetSlot.transform;

            if (targetSlot.linkedPole != null)
                emp.mypole = targetSlot.linkedPole;

            foreach (UpgradeCountData ucd in empData.upgradeCounts)
            {
                Sprite spr = shopUpgrade.allUpgrade[ucd.upgradeIndex].icone;
                emp.upgradeCounts[spr] = ucd.count;

                if (!emp.upgradesImages.Contains(spr))
                    emp.upgradesImages.Add(spr);
            }
        }

        foreach (UpgradeSaveData upData in data.upgrades)
            shopUpgrade.allUpgrade[upData.upgradeIndex].price = upData.currentPrice;

        foreach (PoleSaveData poleData in data.poles)
        {
            PoleLink poleLink = System.Array.Find(_poleLinks, p => p.pole.type == poleData.poleType);
            if (poleLink == null) continue;

            poleLink.pole.BoostEmployeSpeed = poleData.boostEmployeSpeed;
            poleLink.pole.BoostEmployeError = poleData.boostEmployeError;
            poleLink.pole.BonusRevenus = poleData.bonusRevenus;
            poleLink.pole.BoostTimeForSurcharge = poleData.boostTimeForSurcharge;

            poleLink.timedUpgrades.Clear();
            foreach (TimedUpgradeSaveData tData in poleData.timedUpgrades)
            {
                var upgrade = shopUpgrade.allUpgrade[tData.upgradeIndex];
                poleLink.timedUpgrades.Add(new PoleLink.TimedUpgrade
                {
                    icon = upgrade.icone,
                    type = upgrade.type,
                    value = upgrade.upgradeValue,
                    daysRemaining = tData.daysRemaining
                });
            }

            poleLink.pole.upgradeCounts.Clear();
            foreach (UpgradeCountData ucd in poleData.upgradeCounts)
            {
                Sprite spr = shopUpgrade.allUpgrade[ucd.upgradeIndex].icone;
                poleLink.pole.upgradeCounts[spr] = ucd.count;
            }

            poleLink.SetIcone();
        }

        foreach (Pole pole in _poles)
            pole.RebuildEmployeList();

        swatManager.numberOfUtilisation = data.swatUtilisation;
        Debug.Log("[SaveManager] Chargement terminé.");
    }

    // ── Utilitaires publics ───────────────────────────────────────────────────

    /// <summary>Supprime le fichier de sauvegarde et réinitialise le tutoriel.</summary>
    public void DeleteSave()
    {
        if (!HasSave()) return;

        File.Delete(SavePath);
        Debug.Log("[SaveManager] Save supprimée.");

        for (int i = 0; i < _basePrices.Length; i++)
            shopUpgrade.allUpgrade[i].price = shopUpgrade.allUpgrade[i].basePrice;

        if (TutorialManager.Instance != null)
            TutorialManager.Instance.ResetTutorialPrefs();
        else
            TutorialManager.ResetTutorialPrefsStatic();

        Debug.Log("[SaveManager] Tutoriel réinitialisé suite à la suppression de la save.");
    }

    /// <summary>Retourne true si un fichier de sauvegarde existe.</summary>
    public bool HasSave() => File.Exists(SavePath);

    // ── Helpers privés ────────────────────────────────────────────────────────

    private InventorySlot FindSlotOfEmploye(Employe emp)
    {
        DraggableItems draggable = FindDraggableForEmploye(emp);
        if (draggable == null) return null;
        return draggable.transform.parent?.GetComponent<InventorySlot>();
    }

    private DraggableItems FindDraggableForEmploye(Employe emp)
    {
        DraggableItems[] all = FindObjectsByType<DraggableItems>(FindObjectsSortMode.None);
        foreach (DraggableItems d in all)
            if (d.linkedEmploye == emp) return d;
        return null;
    }

    private InventorySlot FindSlot(PoleType poleType, int slotIndex)
    {
        foreach (Pole pole in _poles)
        {
            if (pole.type != poleType || pole.contentparent == null) continue;

            foreach (Transform child in pole.contentparent.transform)
            {
                InventorySlot slot = child.GetComponent<InventorySlot>();
                if (slot != null && slot.slotIndex == slotIndex)
                    return slot;
            }
        }
        return null;
    }

    private string GetScenePath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
