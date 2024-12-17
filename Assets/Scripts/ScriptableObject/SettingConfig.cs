using System;
using Types.record;
using UnityEngine;
using UnityEngine.AdaptivePerformance;

[CreateAssetMenu(fileName = "SettingConfig", menuName = "Scriptable Objects/SettingConfig")]
public class SettingConfig : UnityEngine.ScriptableObject, IHaveDataForSave, ILanguageContainer
{
    [SerializeField] EnumLanguage _selectedLanguage;

    public Action<IHaveDataForSave> _saveAction;
    event Action<EnumLanguage> OnLanguageChanged;

    EnumLanguage ILanguageContainer.Language => _selectedLanguage;

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

    void ILanguageContainer.SubscribeToChangeLanguageEvent(Action<EnumLanguage> action) => OnLanguageChanged += action;

    void ILanguageContainer.UnsubscribeFromChangeLanguageEvent(Action<EnumLanguage> action) => OnLanguageChanged -= action;

    [System.Serializable]
    public record SettingData
    {
        [field: SerializeField] public string Language { get; set; }
    }
}
