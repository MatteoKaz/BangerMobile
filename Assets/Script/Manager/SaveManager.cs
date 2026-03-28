using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Sauvegarde et charge l'état du jeu à chaque début de nouvelle journée.
/// Poles et employés découverts automatiquement dans la scène.
/// Les options audio sont gérées indépendamment par AudioManager.
/// </summary>
public class SaveManager : MonoBehaviour
{
    private const string SaveFileName = "save.json";

    [SerializeField] private DayManager dayManager;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] ShopUpgrade shopUpgrade;
    private Pole[]    _poles;
    private Employe[] _employes;
    private bool      _initialized = false;

    private string SavePath => Path.Combine(Application.persistentDataPath, SaveFileName);

    private void Awake()
    {
        GatherReferences();

        if (HasSave())
            dayManager.skipFirstLaunch = true;
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
            Debug.Log("[SaveManager] Save trouvée → chargement.");
            LoadGame();
        }
        else
        {
            Debug.Log("[SaveManager] Aucune save → nouvelle partie.");
            InitNewGame();
        }

        _initialized = true;
    }

    // ── Initialisation ────────────────────────────────────────────────────────

    /// <summary>Initialise une toute nouvelle partie sans save.</summary>
    private void InitNewGame()
    {
        scoreManager.playerMoney = 0;
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
            currentDay  = dayManager.currentDay,
            currentWeek = dayManager.currentWeek,
            playerMoney = scoreManager.playerMoney
        };

        // Employés
        for (int i = 0; i < _employes.Length; i++)
        {
            Employe emp = _employes[i];
            if (emp == null) continue;

            InventorySlot slot = FindSlotOfEmploye(emp);

            EmployeSaveData empSaveData = new EmployeSaveData
            {
                sceneEmployeIndex     = i,
                employeIndex          = emp.employeIndex,
                timeInEntreprise      = emp.timeInEntreprise,
                poleType              = emp.mypole != null ? emp.mypole.type : PoleType.RedPole,
                slotIndex             = slot != null ? slot.slotIndex : 0,
                weekNumberOfPaperDone = emp.WeeknumberOfPaperDone,
                weekSucceedPaper      = emp.WeeksucceedPaper,
                weekMoneyMake         = emp.WeekmoneyMake,
                workRateBonus         = emp.employeWorkRateBonus,
                errorPercentBonus     = emp.employeErrorPercenBonus,
                stressBonus           = emp.StressBonus

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

        // Employés
        foreach (EmployeSaveData empData in data.employes)
        {
            if (empData.sceneEmployeIndex < 0 || empData.sceneEmployeIndex >= _employes.Length)
                continue;

            Employe emp = _employes[empData.sceneEmployeIndex];
            if (emp == null) continue;

            emp.SetIdentity(empData.employeIndex);
            emp.timeInEntreprise = empData.timeInEntreprise;

            emp.WeeknumberOfPaperDone = empData.weekNumberOfPaperDone;
            emp.WeeksucceedPaper      = empData.weekSucceedPaper;
            emp.WeekmoneyMake         = empData.weekMoneyMake;

            emp.employeWorkRateBonus    = empData.workRateBonus;
            emp.employeErrorPercenBonus = empData.errorPercentBonus;
            emp.StressBonus             = empData.stressBonus;

            InventorySlot targetSlot = FindSlot(empData.poleType, empData.slotIndex);
            if (targetSlot == null) continue;

            DraggableItems draggable = FindDraggableForEmploye(emp);
            if (draggable == null) continue;

            draggable.transform.SetParent(targetSlot.transform, false);
            draggable.transform.localPosition = Vector3.zero;
            draggable.parentAfterDrag         = targetSlot.transform;

            if (targetSlot.linkedPole != null)
                emp.mypole = targetSlot.linkedPole;
            foreach (UpgradeCountData ucd in empData.upgradeCounts)
            {
                Sprite spr = shopUpgrade.allUpgrade[ucd.upgradeIndex].icone;
                emp.upgradeCounts[spr] = ucd.count;

                // restore les images si pas déjà présentes
                if (!emp.upgradesImages.Contains(spr))
                    emp.upgradesImages.Add(spr);
            }

        }
        foreach (UpgradeSaveData upData in data.upgrades)
        {
            shopUpgrade.allUpgrade[upData.upgradeIndex].price = upData.currentPrice;
        }
        foreach (Pole pole in _poles)
            pole.RebuildEmployeList();

        Debug.Log("[SaveManager] Chargement terminé.");
    }

    // ── Utilitaires publics ───────────────────────────────────────────────────

    /// <summary>Supprime le fichier de sauvegarde.</summary>
    public void DeleteSave()
    {
        if (HasSave())
        {
            File.Delete(SavePath);
            Debug.Log("[SaveManager] Save supprimée.");
        }
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
            t    = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
