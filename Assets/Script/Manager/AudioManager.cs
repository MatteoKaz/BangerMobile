using System.IO;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gère le volume global du jeu via AudioListener.
/// Les options audio sont sauvegardées automatiquement dans un fichier dédié à chaque changement.
/// </summary>
public class AudioManager : MonoBehaviour
{
    private const float ThresholdHigh     = 0.6f;
    private const float ThresholdMedium   = 0.2f;
    private const string AudioSaveFileName = "audio_options.json";

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
    private bool  _isMuted;

    private static AudioManager _instance;

    private string AudioSavePath => Path.Combine(Application.persistentDataPath, AudioSaveFileName);

    // ── Propriétés publiques ──────────────────────────────────────────────────

    public float CurrentVolume  => _volumeBeforeMute;
    public bool  CurrentIsMuted => _isMuted;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        LoadAudioOptions();
    }

    private void Start()
    {
        InitUI();
        ApplyVolume();
    }

    // ── UI ────────────────────────────────────────────────────────────────────

    private void InitUI()
    {
        if (volumeSlider != null)
        {
            volumeSlider.minValue = 0f;
            volumeSlider.maxValue = 1f;
            volumeSlider.value    = _volumeBeforeMute;
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
        _isMuted          = false;
        _volumeBeforeMute = value;

        ApplyVolume();
        RefreshMuteButtonSprite();
        SaveAudioOptions();
    }

    /// <summary>Appelé par le bouton mute OnClick.</summary>
    public void ToggleMute()
    {
        _isMuted = !_isMuted;

        if (!_isMuted && _volumeBeforeMute <= 0f)
            _volumeBeforeMute = 1f;

        ApplyVolume();
        RefreshMuteButtonSprite();
        SaveAudioOptions();
    }

    // ── Persistance audio ─────────────────────────────────────────────────────

    /// <summary>Sauvegarde les options audio dans un fichier dédié.</summary>
    private void SaveAudioOptions()
    {
        AudioSaveData data = new AudioSaveData
        {
            audioVolume  = _volumeBeforeMute,
            audioIsMuted = _isMuted
        };

        File.WriteAllText(AudioSavePath, JsonUtility.ToJson(data, true));
        Debug.Log("[AudioManager] Options audio sauvegardées.");
    }

    /// <summary>Charge les options audio depuis le fichier dédié.</summary>
    private void LoadAudioOptions()
    {
        if (!File.Exists(AudioSavePath)) return;

        AudioSaveData data = JsonUtility.FromJson<AudioSaveData>(File.ReadAllText(AudioSavePath));
        _volumeBeforeMute = data.audioVolume;
        _isMuted          = data.audioIsMuted;

        Debug.Log("[AudioManager] Options audio chargées.");
    }

    // ── Interne ───────────────────────────────────────────────────────────────

    private void ApplyVolume()
    {
        AudioListener.volume = _isMuted ? 0f : _volumeBeforeMute;
    }

    private void SyncSlider()
    {
        if (volumeSlider == null) return;

        volumeSlider.onValueChanged.RemoveListener(SetVolume);
        volumeSlider.value = _volumeBeforeMute;
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

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
}
