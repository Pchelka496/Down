using UnityEngine;
using UnityEngine.UI;

public class AudioChangeModule : MonoBehaviour
{
    [SerializeField] Toggle _enabledFlag;
    [SerializeField] Slider _musicVolume;
    [SerializeField] Slider _maxVolume;

    SettingConfig _settingConfig;

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(SettingConfig settingConfig)
    {
        _settingConfig = settingConfig;
    }

    private void Start()
    {
        if (_enabledFlag != null)
        {
            _enabledFlag.isOn = _settingConfig.SoundEnabledFlag;
            _enabledFlag.onValueChanged.AddListener((bool call) => ChangeAudioEnabledFlag());
        }
        if (_musicVolume != null)
        {
            _musicVolume.value = _settingConfig.MusicVolume;
            _musicVolume.onValueChanged.AddListener((float call) => ChangeMusicVolume());
        }
        if (_maxVolume != null)
        {
            _maxVolume.value = _settingConfig.MaxVolume;
            _maxVolume.onValueChanged.AddListener((float call) => ChangeMaxVolume());
        }

        AudioEnabledFlagProcessing(_settingConfig.SoundEnabledFlag);
    }

    private void AudioEnabledFlagProcessing(bool flag)
    {
        if (_musicVolume != null)
        {
            _musicVolume.gameObject.SetActive(flag);
        }
        if (_maxVolume != null)
        {
            _maxVolume.gameObject.SetActive(flag);
        }
    }

    private void ChangeAudioEnabledFlag()
    {
        var flag = _enabledFlag.isOn;

        _settingConfig.SoundEnabledFlag = flag;

        AudioEnabledFlagProcessing(flag);
    }

    private void ChangeMusicVolume()
    {
        var newVolume = _musicVolume.value;

        _settingConfig.MusicVolume = newVolume;
    }

    private void ChangeMaxVolume()
    {
        var maxVolume = _maxVolume.value;

        _settingConfig.MaxVolume = maxVolume;
    }
}
