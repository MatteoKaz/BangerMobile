using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MVPButton : MonoBehaviour
{
    [SerializeField] private RankingManager rankingManager;
    [SerializeField] private DayManager dayManager;
    [SerializeField] private ClickZonePopup clickZonePopup;

    private static readonly List<MVPButton> _allInstances = new List<MVPButton>();
    private static bool _anyMVPSetToday = false;

    private Button _button;
    private EmployeFicheInfo _ficheInfo;

    private void Awake()
    {
        _allInstances.Add(this);
        _button    = GetComponent<Button>();
        _ficheInfo = GetComponentInParent<EmployeFicheInfo>();
        _button.onClick.AddListener(OnMVPButtonClicked);
    }

    private void OnEnable()
    {
        dayManager.DayBegin += ResetMVPGuard;
    }

    private void OnDisable()
    {
        dayManager.DayBegin -= ResetMVPGuard;
    }

    private void OnDestroy()
    {
        _allInstances.Remove(this);
        _button.onClick.RemoveListener(OnMVPButtonClicked);
    }

    /// <summary>Réinitialise le guard partagé et réautorise tous les tampons MVP à chaque début de journée.</summary>
    private void ResetMVPGuard()
    {
        _anyMVPSetToday = false;
        foreach (MVPButton instance in _allInstances)
            instance.clickZonePopup?.Unblock();
    }

    /// <summary>Envoie l'employé de cette fiche au RankingManager comme nouveau MVP choisi.</summary>
    private void OnMVPButtonClicked()
    {
        if (_ficheInfo == null)
        {
            Debug.LogWarning("MVPButton : aucun EmployeFicheInfo trouvé dans les parents.", this);
            return;
        }

        if (_anyMVPSetToday) return;

        _anyMVPSetToday = true;
        foreach (MVPButton instance in _allInstances)
            instance.clickZonePopup?.Block();

        rankingManager.SetChooseMVP(_ficheInfo.LinkedEmploye);
    }
}