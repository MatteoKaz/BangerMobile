using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// À placer sur le bouton MVP d'une fiche employé.
/// Remonte automatiquement vers le EmployeFicheInfo parent pour récupérer l'employé,
/// puis notifie le RankingManager du choix.
/// </summary>
[RequireComponent(typeof(Button))]
public class MVPButton : MonoBehaviour
{
    [SerializeField] private RankingManager rankingManager;

    private Button _button;
    private EmployeFicheInfo _ficheInfo;

    private void Awake()
    {
        _button    = GetComponent<Button>();
        _ficheInfo = GetComponentInParent<EmployeFicheInfo>();

        _button.onClick.AddListener(OnMVPButtonClicked);
    }

    private void OnDestroy()
    {
        _button.onClick.RemoveListener(OnMVPButtonClicked);
    }

    /// <summary>
    /// Envoie l'employé de cette fiche au RankingManager comme nouveau MVP choisi.
    /// </summary>
    private void OnMVPButtonClicked()
    {
        if (_ficheInfo == null)
        {
            Debug.LogWarning("MVPButton : aucun EmployeFicheInfo trouvé dans les parents.", this);
            return;
        }

        rankingManager.SetChooseMVP(_ficheInfo.LinkedEmploye);
    }
}