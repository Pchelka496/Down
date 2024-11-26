using UnityEngine;
using UnityEngine.AddressableAssets;

public class Customizer
{
    const string CUSTOMIZER_CONFIG_ADDRESS = "";

    CustomizerConfig _config;
    IHaveControllerGradient _controllerGradient;
    CharacterController _player;

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Удалите неиспользуемые закрытые члены", Justification = "<Ожидание>")]
    private void Construct(ScreenTouchController screenTouchController, CharacterController player, CustomizerConfig config)
    {
        _controllerGradient = screenTouchController;
        _config = config;
    }

    public void UpdateValues()
    {

    }

}
