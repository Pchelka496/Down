using UnityEngine;

public class LanguageChangeModule : MonoBehaviour
{
    SettingConfig _settingConfig;

    [Zenject.Inject]
    public void Construct(SettingConfig settingConfig)
    {
        _settingConfig = settingConfig;
    }

    public void SetRussianLanguage() => ChangeLanguage(EnumLanguage.Russian);
    public void SetEnglishLanguage() => ChangeLanguage(EnumLanguage.English);
    public void SetUkrainianLanguage() => ChangeLanguage(EnumLanguage.Ukrainian);
    public void SetChineseLanguage() => ChangeLanguage(EnumLanguage.Chinese);

    public void ChangeLanguage(EnumLanguage language)
    {
        _settingConfig.ChangeSelectedLanguage(language);
    }
}
