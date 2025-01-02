using Nenn.InspectorEnhancements.Runtime.Attributes;
using UnityEngine;
using UnityEngine.UI;

public class TutorialOpenButtonExpansion : MonoBehaviour
{
    [Required][SerializeField] Button _button;
    LobbyUIPanelFacade _lobbyUIPanel;

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(LobbyUIPanelFacade lobbyUIPanel)
    {
        _lobbyUIPanel = lobbyUIPanel;
    }

    private void Start()
    {
        _button.onClick.AddListener(_lobbyUIPanel.OpenTutorialPanel);
    }

    private void Reset()
    {
        _button = gameObject.GetComponent<Button>();
    }
}
