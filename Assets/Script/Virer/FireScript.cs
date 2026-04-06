using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class FireButton : MonoBehaviour
{
    [SerializeField] private FireManager fireManager;
    [SerializeField] private DayManager dayManager;
    [SerializeField] private EmployeFicheInfo ficheInfo;
    [SerializeField] private AudioEventDispatcher audioEventDispatcher;
    [SerializeField] private ClickZonePopup clickZonePopup;

    private Button _button;
    private bool _firedAlreadyToday = false;

    private void Awake()
    {
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
        _button.onClick.RemoveListener(OnFireButtonClicked);
    }

    /// <summary>Réinitialise le guard et réautorise le tampon à chaque début de journée.</summary>
    private void ResetFireGuard()
    {
        _firedAlreadyToday = false;
        clickZonePopup?.Unblock();
    }

    /// <summary>Tente de lancer le licenciement de l'employé affiché sur la fiche.</summary>
    private void OnFireButtonClicked()
    {
        if (ficheInfo == null)
        {
            Debug.LogWarning("FireButton : aucun EmployeFicheInfo assigné.", this);
            return;
        }

        if (_firedAlreadyToday) return;

        _firedAlreadyToday = true;
        clickZonePopup?.Block();
        fireManager.Click(ficheInfo);
    }
}