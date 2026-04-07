using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gère le classement hebdomadaire des employés, le MVP choisi par le joueur,
/// et applique des bonus/malus en fin de semaine.
/// </summary>
public class RankingManager : MonoBehaviour
{
    [SerializeField] private Employe[] employes;
    [SerializeField] private RankObject[] objects;
    [SerializeField] public Image Besticone;
    [SerializeField] public DayManager DayManager;
    [SerializeField] public GameObject rankingFolder;

    [Header("MVP")]
    /// <summary>Employé désigné MVP par le joueur pendant la semaine.</summary>
    public Employe ChooseMVP;
    /// <summary>MVP de la semaine précédente — celui qui a les bonus actifs.</summary>
    public Employe CurrentMVP;
    public Employe CurrentTopEarner;

    [Header("Valeurs de bonus MVP")]
    [SerializeField] private float mvpWorkRateBonus = 0.5f;
    [SerializeField] private float mvpBonusPaperDone = 1f;

    [Header("Valeurs de malus top earner")]
    [SerializeField] private float topEarnerErrorMalus = 0.1f;
    [SerializeField] private float topEarnerWorkRateMalus = 0.5f;

    [SerializeField] ParticleSystem paper1;
    [SerializeField] ParticleSystem paper2;
    private void OnEnable()
    {
        DayManager.RankingDay += SetRankingOrder;
    }

    private void OnDisable()
    {
        DayManager.RankingDay -= SetRankingOrder;
    }

    /// <summary>
    /// Appelé par le bouton MVP sur la fiche d'un employé.
    /// Remplace l'ancien ChooseMVP par le nouvel employé sélectionné
    /// et met à jour l'icône MVP immédiatement.
    /// </summary>
    public void SetChooseMVP(Employe employe)
    {
        ChooseMVP = employe;

        if (Besticone != null)
            Besticone.sprite = employe.employeImage.sprite;

        Debug.Log($"[MVP] Nouveau MVP choisi : {employe.employeName}");
    }

    /// <summary>
    /// Applique les bonus de MVP à l'employé du mois :
    /// vitesse de travail améliorée et papiers bonus.
    /// </summary>
    private void SetBonus(Employe employe)
    {
        employe.employeWorkRateBonus_MVP += mvpWorkRateBonus;
        employe.BonusPaperDone_MVP += 1;
        employe.couronne.SetActive(true);
    }

    /// <summary>
    /// Applique les malus à l'employé ayant rapporté le plus d'argent :
    /// taux d'erreur augmenté et ralentissement du travail.
    /// </summary>
    private void SetMalus(Employe employe)
    {
        employe.RankingPercentMalus += topEarnerErrorMalus;
        employe.RankingWorkRateMalus += topEarnerWorkRateMalus;
    }

    /// <summary>
    /// Réinitialise les bonus et malus liés au statut MVP.
    /// Appelé avant d'appliquer les nouveaux bonus pour repartir d'un état propre.
    /// </summary>
    private void ResetMVPStats(Employe employe)
    {
        if (employe == null) return;

        employe.employeWorkRateBonus_MVP = 0f;
        employe.BonusPaperDone_MVP = 0f;
        employe.ResetRankingMalus();
        employe.couronne.SetActive(false);
    }

    /// <summary>
    /// Trie les employés par argent gagné, met à jour l'affichage du classement,
    /// change l'icône principale par celle du MVP, retire le MVP de la liste affichée,
    /// applique bonus au MVP et malus au top earner (sauf si identiques),
    /// puis fait avancer ChooseMVP → CurrentMVP.
    /// Si aucun MVP n'a été choisi par le joueur, le top earner gagne automatiquement la place et le bonus.
    /// </summary>
    public void SetRankingOrder()
    {
        // 1 — Réinitialiser les stats de l'ancien MVP avant tout
        ResetMVPStats(CurrentMVP);
        ResetMVPStats(CurrentTopEarner); 
        if (CurrentMVP != null)
            Debug.Log($"[MVP] Reset des stats de l'ancien MVP : {CurrentMVP.employeName}");

        // 2 — Trier les employés par argent rapporté cette semaine (décroissant)
        List<Employe> ranked = new List<Employe>(employes);
        ranked.Sort((a, b) => b.WeekmoneyMake.CompareTo(a.WeekmoneyMake));

        // 3 — Identifier le top earner (premier du classement)
        Employe topEarner = ranked.Count > 0 ? ranked[0] : null;
        if (topEarner != null)
            Debug.Log($"[MVP] Top earner cette semaine : {topEarner.employeName} ({topEarner.WeekmoneyMake}$)");

        // 3b — Si aucun MVP n'a été choisi, le top earner gagne automatiquement la place et le bonus
        if (ChooseMVP == null && topEarner != null)
        {
            ChooseMVP = topEarner;
            if (Besticone != null)
                Besticone.sprite = topEarner.employeImage.sprite;
            Debug.Log($"[MVP] Aucun choix du joueur — MVP automatique : {topEarner.employeName} (top earner)");
        }

        // 4 — Construire la liste d'affichage en retirant le MVP choisi
        List<Employe> displayList = new List<Employe>(ranked);
        if (ChooseMVP != null)
            displayList.Remove(ChooseMVP);

        // 5 — Remplir les RankObjects avec la liste filtrée
        for (int i = 0; i < objects.Length; i++)
        {
            if (i < displayList.Count)
            {
                objects[i].NameToShow = displayList[i].employeName;
                objects[i].SucceedPaper = displayList[i].WeeksucceedPaper;
                objects[i].TotalPaper = displayList[i].WeeknumberOfPaperDone;
                objects[i].TotalMoney = displayList[i].WeekmoneyMake;
                objects[i].icone.sprite = displayList[i].employeImage.sprite;
            }
            objects[i].SetText();
        }

        // 6 — Changer l'icône principale par celle du MVP choisi
        if (ChooseMVP != null && Besticone != null)
            Besticone.sprite = ChooseMVP.employeImage.sprite;

        // 7 — Appliquer SetBonus au MVP choisi
        if (ChooseMVP != null)
        {
            Debug.Log($"[MVP] BONUS appliqué à : {ChooseMVP.employeName}");
            SetBonus(ChooseMVP);
        }

        // 8 — Appliquer SetMalus au top earner, sauf si c'est le même que le MVP
        if (topEarner != null && topEarner != ChooseMVP)
        {
            Debug.Log($"[MVP] MALUS appliqué à : {topEarner.employeName} (top earner != MVP)");
            SetMalus(topEarner);
        }
        else if (topEarner != null && topEarner == ChooseMVP)
        {
            Debug.Log($"[MVP] Top earner = MVP ({topEarner.employeName}) — pas de malus appliqué.");
        }

        // 9 — Faire passer ChooseMVP → CurrentMVP pour la prochaine semaine
        Debug.Log($"[MVP] Fin de semaine — CurrentMVP devient : {(ChooseMVP != null ? ChooseMVP.employeName : "aucun")}");
        CurrentMVP = ChooseMVP;
        CurrentTopEarner = (topEarner != ChooseMVP) ? topEarner : null;
        ChooseMVP = null;

        if (DayManager.currentWeek == 1)
            TutorialManager.NotifyFirstFridayRanking();

    }

    public void AnimRanking()
    {
        paper1.Play();
        paper2.Play(); 
    }
}
