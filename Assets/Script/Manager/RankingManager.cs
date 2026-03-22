using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RankingManager : MonoBehaviour
{
   [SerializeField] private Employe[] employes;
   [SerializeField] private RankObject[] objects;
    [SerializeField] public Image Besticone;
    [SerializeField] public DayManager DayManager;
    [SerializeField] public GameObject rankingFolder;


    public void OnEnable()
    {
        DayManager.RankingDay += SetRankingOrder;
    }

    public void OnDisable()
    {
        DayManager.RankingDay -= SetRankingOrder;
    }
    public void SetRankingOrder()
    {
        List<Employe> ranked = new List<Employe>(employes);
        ranked.Sort((a, b) => b.WeekmoneyMake.CompareTo(a.WeekmoneyMake));

        for (int i = 0; i < ranked.Count; i++)
        {
            objects[i].NameToShow = ranked[i].employeName;
            objects[i].SucceedPaper = ranked[i].WeeksucceedPaper;
            objects[i].TotalPaper = ranked[i].WeeknumberOfPaperDone;
            objects[i].TotalMoney = ranked[i].WeekmoneyMake;
            objects[i].icone = ranked[i].employeImage;
        }
        Besticone = objects[0].icone;
        foreach (RankObject rankobject in objects)
        {
            rankobject.SetText();
        }
    }
}
