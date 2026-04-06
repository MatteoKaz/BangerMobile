using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class MVPButton : MonoBehaviour
{
    [SerializeField] private RankingManager rankingManager;
    [SerializeField] private DayManager dayManager;
    [SerializeField] private AudioEventDispatcher audioEventDispatcher;
    [SerializeField] private ClickZonePopup clickZonePopup;

    private Button _button;
    private EmployeFicheInfo _ficheInfo;
    private bool _mvpAlreadySetToday = false;

    private void Awake()
    {
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
        _button.onClick.RemoveListener(OnMVPButtonClicked);
    }

    /// <summary>Réinitialise le guard et réautorise le tampon à chaque début de journée.</summary>
    private void ResetMVPGuard()
    {
        _mvpAlreadySetToday = false;
        clickZonePopup?.Unblock();
    }

    /// <summary>Envoie l'employé de cette fiche au RankingManager comme nouveau MVP choisi.</summary>
    private void OnMVPButtonClicked()
    {
        if (_ficheInfo == null)
        {
            Debug.LogWarning("MVPButton : aucun EmployeFicheInfo trouvé dans les parents.", this);
            return;
        }

        if (_mvpAlreadySetToday) return;

        _mvpAlreadySetToday = true;
        clickZonePopup?.Block();
        rankingManager.SetChooseMVP(_ficheInfo.LinkedEmploye);
    }
}