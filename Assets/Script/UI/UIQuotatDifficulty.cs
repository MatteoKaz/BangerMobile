using TMPro;
using UnityEngine;

/// <summary>
/// Displays the quota and score bonus percentage for the currently selected difficulty.
/// Updates when the difficulty slider changes or when quotas are recalculated.
/// </summary>
public class UIQuotatDifficulty : MonoBehaviour
{
    [SerializeField] private QuotatManager    quotatManager;
    [SerializeField] private ScoreManager     scoreManager;
    [SerializeField] private DifficultySlider difficultySlider;
    [SerializeField] private TextMeshProUGUI  label;

    [Tooltip("Format du texte. {0} = quota, {1} = bonus en %. Ex: 'Quota : {0}\\nBonus score : +{1}%'")]
    [SerializeField] private string format = "Quota : {0}\nBonus score : +{1}%";

    private int _currentDifficultyIndex = 0;

    private void OnEnable()
    {
        quotatManager.CalculQuotat         += RefreshText;
        difficultySlider.DifficultyChanged += OnDifficultyChanged;
        OnDifficultyChanged(difficultySlider.CurrentDifficulty);
    }

    private void OnDisable()
    {
        quotatManager.CalculQuotat         -= RefreshText;
        difficultySlider.DifficultyChanged -= OnDifficultyChanged;
    }

    /// <summary>Called when the slider snaps to a new difficulty.</summary>
    private void OnDifficultyChanged(int index)
    {
        _currentDifficultyIndex = index;
        RefreshText();
    }

    /// <summary>Recalculates and updates the label for the current difficulty.</summary>
    private void RefreshText()
    {
        int   quota = GetQuota(_currentDifficultyIndex);
        float bonus = GetBonusPercent(_currentDifficultyIndex);

        label.text = string.Format(format, quota, bonus.ToString("0.#"));
    }

    private int GetQuota(int index) => index switch
    {
        0 => quotatManager.quotatEasy,
        1 => quotatManager.quotatMid,
        _ => quotatManager.quotatHard
    };

    private float GetBonusPercent(int index) => index switch
    {
        0 => scoreManager.bonusPoleEasy * 100f,
        1 => scoreManager.bonusPoleMid  * 100f,
        _ => scoreManager.bonusPoleHard * 100f
    };
}
