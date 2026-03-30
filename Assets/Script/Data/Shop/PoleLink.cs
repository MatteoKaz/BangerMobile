using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PoleLink : MonoBehaviour
{
    [SerializeField] public Pole pole;
    [SerializeField] public DayManager DayManager;
    [SerializeField] public UpgradeSetUp upgradeSet;
    [SerializeField] public Image myCase;
    [SerializeField] public List<Image> allempImage;
    public List<Sprite> upgradesImages = new List<Sprite>();
    [SerializeField] public Image[] imagesUpgrades;
    public int daysRemaining;


    [System.Serializable]
    public struct TimedUpgrade
    {
        public Sprite icon;
        public TypeOfUpgrade type;
        public float value;
        public int daysRemaining;
    }

    public List<TimedUpgrade> timedUpgrades = new List<TimedUpgrade>();
    public void OnEnable()
    {
        upgradeSet.PoleSet += MyIdentity;
        


    }
    public void Awake()
    {
        if (DayManager != null)
            DayManager.DayEnd += OnDayEnd;
    }

    public void OnDisable()
    {
        upgradeSet.PoleSet -= MyIdentity;
        
    }

    public void OnDestroy()
    {
        if (DayManager != null)
            DayManager.DayEnd -= OnDayEnd;
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


        // Reconstruit upgradesImages depuis timedUpgrades (dédoublonné, ordre stable)
        upgradesImages.Clear();
        pole.upgradesImages.Clear();
        foreach (var t in timedUpgrades)
        {
            if (!upgradesImages.Contains(t.icon))
            {
                upgradesImages.Add(t.icon);
                pole.upgradesImages.Add(t.icon);
            }
        }

        for (int i = 0; i < imagesUpgrades.Length; i++)
        {
            if (i < upgradesImages.Count)
            {
                imagesUpgrades[i].enabled = true;
                imagesUpgrades[i].sprite = upgradesImages[i];
            }
            else
            {
                imagesUpgrades[i].enabled = false;
            }
        }
    }
    public void OnDayEnd()
    {
        Debug.LogWarning("DecrementDay");
        for (int i = timedUpgrades.Count - 1; i >= 0; i--)
        {
            TimedUpgrade u = timedUpgrades[i];
            u.daysRemaining--;

            if (u.daysRemaining <= 0)
            {
                switch (u.type)
                {
                    case TypeOfUpgrade.BoostSpeedPole: pole.BoostEmployeSpeed -= u.value; break;
                    case TypeOfUpgrade.BoostErrorPole: pole.BoostEmployeError -= u.value; break;
                    case TypeOfUpgrade.PrimePole: pole.BonusRevenus -= u.value; break;
                    case TypeOfUpgrade.CigarettePole: pole.BoostTimeForSurcharge -= u.value; break;
                }
                if (pole.upgradeCounts.ContainsKey(u.icon))
                {
                    pole.upgradeCounts[u.icon]--;
                    if (pole.upgradeCounts[u.icon] <= 0)
                        pole.upgradeCounts.Remove(u.icon);
                }
                timedUpgrades.RemoveAt(i);
                SetIcone(); // reconstruit tout automatiquement

            }

            else
            {
                timedUpgrades[i] = u;
            }
        }
    }
}
