using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class FireButton : MonoBehaviour
{
    [SerializeField] private FireManager fireManager;
    [SerializeField] private DayManager dayManager;
    [SerializeField] private EmployeFicheInfo ficheInfo;
    [SerializeField] private ClickZonePopup clickZonePopup;

    private static readonly List<FireButton> _allInstances = new List<FireButton>();
    private static bool _anyFiredToday = false;

    private Button _button;

    private void Awake()
    {
        _allInstances.Add(this);
        _button = GetComponent<Button>();
        _button.onClick.AddListener(OnFireButtonClicked);
    }

    private void OnEnable()
    {
        dayManager.DayBegin += ResetFireGuard;
    }

    private void OnDisable()
    {
        dayManager.DayBegin -= ResetFireGuard;
    }

    private void OnDestroy()
    {
        _allInstances.Remove(this);
        _button.onClick.RemoveListener(OnFireButtonClicked);
    }

    /// <summary>Réinitialise le guard partagé et réautorise tous les tampons Virer à chaque début de journée.</summary>
    private void ResetFireGuard()
    {
        _anyFiredToday = false;
        foreach (FireButton instance in _allInstances)
            instance.clickZonePopup?.Unblock();
    }

    /// <summary>Tente de lancer le licenciement de l'employé affiché sur la fiche.</summary>
    private void OnFireButtonClicked()
    {
        if (ficheInfo == null)
        {
            Debug.LogWarning("FireButton : aucun EmployeFicheInfo assigné.", this);
            return;
        }

        if (_anyFiredToday) return;

        _anyFiredToday = true;
        foreach (FireButton instance in _allInstances)
            instance.clickZonePopup?.Block();

        fireManager.Click(ficheInfo);
    }
}