using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Représente une case affichée dans la roulette verticale.
/// </summary>
public class RouletteSlot : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private Image           iconImage;

    [Tooltip("Image de fond du slot, utilisée pour le highlight de sélection.")]
    [SerializeField] private Image backgroundImage;

    private Color _defaultBackgroundColor;

    private void Awake()
    {
        if (backgroundImage != null)
            _defaultBackgroundColor = backgroundImage.color;
    }

    /// <summary>Remplit la case avec les données d'un employé.</summary>
    public void Setup(EmployeDataz data)
    {
        if (nameLabel != null)
            nameLabel.text = data.EmployeName;

        if (iconImage != null)
        {
            iconImage.sprite  = data.icone;
            iconImage.enabled = data.icone != null;
        }
    }

    /// <summary>Active ou désactive la mise en surbrillance du slot.</summary>
    public void SetHighlight(bool active, Color color = default)
    {
        if (backgroundImage == null) return;

        backgroundImage.color = active ? color : _defaultBackgroundColor;
    }
}