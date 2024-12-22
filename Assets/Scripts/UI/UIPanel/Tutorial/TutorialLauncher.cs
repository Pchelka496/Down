using UnityEngine;

public class TutorialLauncher : MonoBehaviour
{
    const string TUTORIAL_COMPLETED_KEY = "TutorialCompleted";
    LobbyUIPanelFacade _lobbyUIPanel;

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(LobbyUIPanelFacade lobbyUIPanel)
    {
        _lobbyUIPanel = lobbyUIPanel;
    }

    private void Awake()
    {
        if (PlayerPrefs.HasKey(TUTORIAL_COMPLETED_KEY))
        {
            Destroy(gameObject);
        }
        else
        {
            _lobbyUIPanel.OpenTutorialPanel();

            PlayerPrefs.SetInt(TUTORIAL_COMPLETED_KEY, 1);
            PlayerPrefs.Save();
        }
    }

#if UNITY_EDITOR
    [ContextMenu("Reset Tutorial Key")]
    private void ResetTutorialKey()
    {
        if (PlayerPrefs.HasKey(TUTORIAL_COMPLETED_KEY))
        {
            PlayerPrefs.DeleteKey(TUTORIAL_COMPLETED_KEY);
            PlayerPrefs.Save();
            Debug.Log($"The key ‘{TUTORIAL_COMPLETED_KEY}’ has been removed from PlayerPrefs. The tutorial will be restarted.");
        }
        else
        {
            Debug.Log($"The key ‘{TUTORIAL_COMPLETED_KEY}’ is already missing from PlayerPrefs.");
        }
    }
#endif
}
