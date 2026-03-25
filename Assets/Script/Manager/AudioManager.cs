using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gère le volume global du jeu via AudioListener.
/// Persiste entre les scènes et sauvegarde les préférences dans PlayerPrefs.
/// </summary>
public class AudioManager : MonoBehaviour
{
    private const string VolumeKey = "MasterVolume";
    private const string MuteKey   = "IsMuted";

    private const float ThresholdHigh   = 0.6f;
    private const float ThresholdMedium = 0.2f;

    [Header("UI")]
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Button muteButton;

    [Header("Icônes slider (100 % / 60 % / 20 % / 0 %)")]
    [SerializeField] private Sprite spriteHigh;
    [SerializeField] private Sprite spriteMedium;
    [SerializeField] private Sprite spriteLow;
    [SerializeField] private Sprite spriteMutedSlider;

    [Header("Icône bouton mute actif")]
    [SerializeField] private Sprite spriteMutedButton;

    private Image _muteButtonImage;
    private float _volumeBeforeMute = 1f;
    private bool _isMuted;

    private static AudioManager _instance;

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        LoadPreferences();
    }

    private void Start()
    {
        InitUI();
        ApplyVolume();
    }

    // ── UI ───────────────────────────────────────────────────────────────────

    private void InitUI()
    {
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value = _volumeBeforeMute;
            volumeSlider.onValueChanged.AddListener(SetVolume);
        }

        if (muteButton != null)
        {
            _muteButtonImage = muteButton.GetComponent<Image>();
            muteButton.onClick.AddListener(ToggleMute);
        }

        RefreshMuteButtonSprite();
    }

    // ── API publique ──────────────────────────────────────────────────────────

    /// <summary>Appelé par le slider OnValueChanged.</summary>
    public void SetVolume(float value)
    {
        // Bouger le slider désactive le mute bouton
        _isMuted = false;
        _volumeBeforeMute = value;

        ApplyVolume();
        RefreshMuteButtonSprite();
        SavePreferences();
    }

    /// <summary>Appelé par le bouton mute OnClick. Ne touche pas au slider.</summary>
    public void ToggleMute()
    {
        _isMuted = !_isMuted;

        if (!_isMuted && _volumeBeforeMute <= 0f)
            _volumeBeforeMute = 1f;

        ApplyVolume();
        RefreshMuteButtonSprite();
        SavePreferences();
    }

    // ── Interne ───────────────────────────────────────────────────────────────

    private void ApplyVolume()
    {
        AudioListener.volume = _isMuted ? 0f : _volumeBeforeMute;
    }

    /// <summary>
    /// Priorité : mute bouton actif → spriteMutedButton.
    /// Sinon, suit la valeur du slider :
    /// ≥ 60 % → High | ≥ 20 % → Medium | > 0 % → Low | 0 % → MutedSlider
    /// </summary>
    private void RefreshMuteButtonSprite()
    {
        if (_muteButtonImage == null) return;

        if (_isMuted)
        {
            _muteButtonImage.sprite = spriteMutedButton;
            return;
        }

        _muteButtonImage.sprite = _volumeBeforeMute switch
        {
            >= ThresholdHigh   => spriteHigh,
            >= ThresholdMedium => spriteMedium,
            > 0f               => spriteLow,
            _                  => spriteMutedSlider
        };
    }

    private void SavePreferences()
    {
        PlayerPrefs.SetFloat(VolumeKey, _volumeBeforeMute);
        PlayerPrefs.SetInt(MuteKey, _isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void LoadPreferences()
    {
        _volumeBeforeMute = PlayerPrefs.GetFloat(VolumeKey, 1f);
        _isMuted          = PlayerPrefs.GetInt(MuteKey, 0) == 1;
    }
}
