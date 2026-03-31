using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Enables or disables the difficulty menu lights based on UiManager events.
/// Lights turn on when the difficulty menu becomes visible, and off when it closes.
/// </summary>
public class DifficultyLightController : MonoBehaviour
{
    [SerializeField] private UiManager uiManager;
    [SerializeField] private Light2D[] lights;

    private void Awake()
    {
        SetLightsEnabled(false);
    }

    private void OnEnable()
    {
        SetLightsEnabled(false);
        uiManager.DifficultyShownAnim += TurnOnLights;
        uiManager.DifficultyChosenAnim += TurnOffLights;
    }

    private void OnDisable()
    {
        uiManager.DifficultyShownAnim -= TurnOnLights;
        uiManager.DifficultyChosenAnim -= TurnOffLights;
    }

    /// <summary>Turns all difficulty lights on.</summary>
    private void TurnOnLights() => SetLightsEnabled(true);

    /// <summary>Turns all difficulty lights off.</summary>
    private void TurnOffLights() => SetLightsEnabled(false);

    private void SetLightsEnabled(bool enabled)
    {
        foreach (Light2D light in lights)
        {
            if (light != null)
                light.enabled = enabled;
        }
    }
}