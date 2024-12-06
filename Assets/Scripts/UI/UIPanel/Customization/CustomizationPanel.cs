using Creatures.Player;
using ScriptableObject;
using UI.UIPanel.Customization.Skins;
using UnityEngine;

public class CustomizationPanel : MonoBehaviour, IUIPanel
{
    [SerializeField] RectTransform _playerPosition;
    [SerializeField] ControllerCustomizer _controllerCustomizer;
    [SerializeField] SkinsCustomizer _skinsCustomizer;
    [SerializeField] GameObject _mainMenu;

    PlayerController _player;
    EnumCustomizationPanelState _currentState;

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:������� �������������� �������� �����", Justification = "<��������>")]
    private void Construct(CustomizerConfig config, PlayerController player)
    {
        _controllerCustomizer.Initialize(config);
        _skinsCustomizer.Initialize(config, config);

        _player = player;

        _skinsCustomizer.gameObject.SetActive(false);
        _controllerCustomizer.gameObject.SetActive(false);
    }

    public void Open()
    {
        ChangeState(EnumCustomizationPanelState.MainMenu);
        _player.OpenPanel();
        _player.transform.position = _playerPosition.position;
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
        _player.ClosePanel();
    }

    public void OpenControllerCustomizer() => ChangeState(EnumCustomizationPanelState.ControllerCustomization);

    public void OpenSkinsCustomizer() => ChangeState(EnumCustomizationPanelState.SkinCustomization);

    public void BackButton()
    {
        switch (_currentState)
        {
            case EnumCustomizationPanelState.MainMenu:
                {
                    Close();
                    break;
                }
            default:
                {
                    ChangeState(EnumCustomizationPanelState.MainMenu);
                    break;
                }
        }
    }

    private void ChangeState(EnumCustomizationPanelState newState)
    {
        _currentState = newState;

        switch (_currentState)
        {
            case EnumCustomizationPanelState.MainMenu:
                {
                    _mainMenu.SetActive(true);
                    _controllerCustomizer.gameObject.SetActive(false);
                    _skinsCustomizer.gameObject.SetActive(false);

                    break;
                }
            case EnumCustomizationPanelState.SkinCustomization:
                {
                    _skinsCustomizer.gameObject.SetActive(true);
                    _controllerCustomizer.gameObject.SetActive(false);
                    _mainMenu.SetActive(false);

                    break;
                }
            case EnumCustomizationPanelState.ControllerCustomization:
                {
                    _controllerCustomizer.gameObject.SetActive(true);
                    _skinsCustomizer.gameObject.SetActive(false);
                    _mainMenu.SetActive(false);

                    break;
                }
            default:
                {
                    Debug.LogError("Unknown state");
                    break;
                }
        }
    }

    public enum EnumCustomizationPanelState
    {
        SkinCustomization,
        ControllerCustomization,
        MainMenu
    }

}
