using UnityEngine;

public class SettingPanel : MonoBehaviour, IUIPanel
{
    //LanguageChangeModule _languageChangeModule;

    public void Close()
    {
        gameObject.SetActive(false);
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }
}
