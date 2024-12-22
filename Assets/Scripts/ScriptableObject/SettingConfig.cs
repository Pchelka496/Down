using System;
using Types.record;
using UnityEngine;

[CreateAssetMenu(fileName = "SettingConfig", menuName = "Scriptable Objects/SettingConfig")]
public class SettingConfig : UnityEngine.ScriptableObject, IHaveDataForSave, ILanguageContainer, IAudioSettingContainer
{
    [SerializeField] EnumLanguage _selectedLanguage;
    [SerializeField] bool _soundEnabledFlag;
    [SerializeField][Range(0f, 1f)] float _maxVolume = 0.7f;
    [SerializeField][Range(0f, 1f)] float _musicVolume = 0.5f;

    Action<IHaveDataForSave> _saveAction;
    event Action<EnumLanguage> OnLanguageChanged;// ILanguageContainer
    event Action<bool> OnSoundEnabledChanged;
    event Action<float> OnMaxVolumeChanged;
    event Action<float> OnMusicVolumeChanged;

    EnumLanguage ILanguageContainer.Language => _selectedLanguage;

    bool IAudioSettingContainer.SoundEnabledFlag => _soundEnabledFlag;
    float IAudioSettingContainer.MusicVolume => UnityEngine.Mathf.Clamp(_musicVolume, 0f, _maxVolume);
    float IAudioSettingContainer.MaxVolume => _maxVolume;

    public bool SoundEnabledFlag
    {
        get => _soundEnabledFlag;
        set
        {
            if (value == _soundEnabledFlag) return;

            _soundEnabledFlag = value;
            OnSoundEnabledChanged?.Invoke(value);
        }
    }

    public float MusicVolume
    {
        get
        {
            return UnityEngine.Mathf.Clamp(_musicVolume, 0f, _maxVolume);
        }
        set
        {
            if (UnityEngine.Mathf.Abs(value - _musicVolume) < 0.001f) return;

            _musicVolume = value;
            OnMusicVolumeChanged?.Invoke(MusicVolume);
        }
    }

    public float MaxVolume
    {
        get => _maxVolume;
        set
        {
            if (UnityEngine.Mathf.Abs(value - _maxVolume) < 0.001f) return;

            _maxVolume = value;
            OnMaxVolumeChanged?.Invoke(value);
        }
    }

    void IHaveDataForSave.LoadSaveData(SaveData saveData)
    {
        if (saveData == null)
        {
            Debug.LogError($"SaveData is null. {GetType()}");
            return;
        }

        var settingData = saveData.SettingConfigData;

        if (settingData == null)
        {
            Debug.LogError($"saveData.SettingConfigData is null. {GetType()}");
            return;
        }

        if (Enum.TryParse(settingData.Language, out EnumLanguage parsedLanguage))
        {
            _selectedLanguage = parsedLanguage;
        }
        else
        {
            Debug.LogWarning($"Invalid language value: {settingData.Language}. Defaulting to English.");
            _selectedLanguage = EnumLanguage.English;
        }
    }

    void IHaveDataForSave.SaveToSaveData(SaveData saveData)
    {
        if (saveData == null)
        {
            Debug.LogError($"SaveData is null. {GetType()}");
            return;
        }

        saveData.SettingConfigData = new()
        {
            Language = _selectedLanguage.ToString(),
        };
    }

    Action IHaveDataForSave.SubscribeWithUnsubscribe(Action<IHaveDataForSave> saveAction)
    {
        _saveAction = saveAction;

        return () => _saveAction = null;
    }

    public void ChangeSelectedLanguage(EnumLanguage language)
    {
        var oldLanguage = _selectedLanguage;
        _selectedLanguage = language;

        if (oldLanguage != language)
        {
            _saveAction?.Invoke(this);
        }

        if (OnLanguageChanged == null) return;

        foreach (var handler in OnLanguageChanged.GetInvocationList())
        {
            try
            {
                ((Action<EnumLanguage>)handler)?.Invoke(_selectedLanguage);
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"Error in OnLanguageChanged handler: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }

    void ILanguageContainer.SubscribeToChangeLanguageEvent(Action<EnumLanguage> callback) => OnLanguageChanged += callback;
    void ILanguageContainer.UnsubscribeFromChangeLanguageEvent(Action<EnumLanguage> callback) => OnLanguageChanged -= callback;

    void IAudioSettingContainer.SubscribeToChangeSoundEnableEvent(Action<bool> callback) => OnSoundEnabledChanged += callback;
    void IAudioSettingContainer.UnsubscribeFromChangeSoundEnableEvent(Action<bool> callback) => OnSoundEnabledChanged -= callback;

    void IAudioSettingContainer.SubscribeToChangeMaxVolumeEvent(Action<float> callback) => OnMaxVolumeChanged += callback;
    void IAudioSettingContainer.UnsubscribeFromChangeMaxVolumeEvent(Action<float> callback) => OnMaxVolumeChanged -= callback;

    void IAudioSettingContainer.SubscribeToChangeMusicVolumeEvent(Action<float> callback) => OnMusicVolumeChanged += callback;
    void IAudioSettingContainer.UnsubscribeFromChangeMusicVolumeEvent(Action<float> callback) => OnMusicVolumeChanged -= callback;

    [System.Serializable]
    public record SettingData
    {
        [field: SerializeField] public string Language { get; set; }
    }
}
