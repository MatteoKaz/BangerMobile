using TMPro;
using UnityEngine;

/// <summary>
/// Displays a text that updates live as the difficulty slider changes.
/// Each difficulty has its own configurable text, set in the Inspector.
/// </summary>
public class UICurrentDifficultyLabel : MonoBehaviour
{
    [SerializeField] private DifficultySlider difficultySlider;
    [SerializeField] private TextMeshProUGUI label;

    [Header("Labels per difficulty")]
    [SerializeField] private string textEasy   = "Facile";
    [SerializeField] private string textMedium = "Normal";
    [SerializeField] private string textHard   = "Difficile";

    private void OnEnable()
    {
        difficultySlider.DifficultyChanged += OnDifficultyChanged;
        OnDifficultyChanged(difficultySlider.CurrentDifficulty);
    }

    private void OnDisable()
    {
        difficultySlider.DifficultyChanged -= OnDifficultyChanged;
    }

    /// <summary>Updates the label when the selected difficulty changes.</summary>
    private void OnDifficultyChanged(int index)
    {
        label.text = index switch
        {
            0 => textEasy,
            1 => textMedium,
            _ => textHard
        };
    }
}