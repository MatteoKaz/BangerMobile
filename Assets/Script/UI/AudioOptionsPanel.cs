using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// À attacher sur le panel options. Relie les références UI à l'AudioManager persistant à l'activation.
/// </summary>
public class AudioOptionsPanel : MonoBehaviour
{
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Button muteButton;

    private void OnEnable()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.BindUI(volumeSlider, muteButton);
    }
}