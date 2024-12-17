using UnityEngine;

public class SettingPanel : MonoBehaviour, IUIPanel
{
    SettingConfig _settingConfig;

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(SettingConfig settingConfig)
    {
        _settingConfig = settingConfig;
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void SetRussianLanguage() => ChangeLanguage(EnumLanguage.Russian);
    public void SetEnglishLanguage() => ChangeLanguage(EnumLanguage.English);
    public void SetUkrainianLanguage() => ChangeLanguage(EnumLanguage.Ukrainian);
    public void SetChineseLanguage() => ChangeLanguage(EnumLanguage.Chinese);

    private void ChangeLanguage(EnumLanguage language)
    {
        _settingConfig.ChangeSelectedLanguage(language);
    }
}
