using UnityEngine;

public class CustomizationPanel : MonoBehaviour, IUIPanel
{
    [SerializeField] RectTransform _playerPosition;
    [SerializeField] ControllerCustomizer _controllerCustomizer;
    [SerializeField] SkinsCustomizer _skinsCustomizer;
    CharacterController _player;

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Удалите неиспользуемые закрытые члены", Justification = "<Ожидание>")]
    private void Construct(CustomizerConfig config, CharacterController player)
    {
        _controllerCustomizer.Initialize(config);
        _player = player;

        _skinsCustomizer.gameObject.SetActive(false);
        _controllerCustomizer.gameObject.SetActive(false);
    }

    public void Open()
    {
        gameObject.SetActive(true);
        _player.OpenPanel();
    }

    public void Close()
    {
        gameObject.SetActive(false);
        _player.ClosePanel();
    }

    public void OpenControllerCustomizer()
    {
        _skinsCustomizer.gameObject.SetActive(false);
        _controllerCustomizer.gameObject.SetActive(true);
    }

    public void OpenSkinsCustomizer()
    {
        _controllerCustomizer.gameObject.SetActive(false);
        _skinsCustomizer.gameObject.SetActive(true);
    }

}
