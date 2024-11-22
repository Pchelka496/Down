using UnityEngine;
using UnityEngine.AddressableAssets;

public class Customizer
{
    const string CUSTOMIZER_CONFIG_ADDRESS = "";
    ScreenTouchController _screenTouchController;
    CharacterController _player;

    [Zenject.Inject]
    private void Construct(ScreenTouchController screenTouchController, CharacterController player)
    {

    }

}
