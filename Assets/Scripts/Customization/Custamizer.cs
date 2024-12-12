using Creatures.Player;
using ScriptableObject;

namespace Customization
{
    public class Customizer : System.IDisposable
    {
        CustomizerConfig _config;
        IHaveControllerGradient _controllerGradient;
        IHaveSkin _player;

        [Zenject.Inject]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
        private void Construct(ScreenTouchController screenTouchController, PlayerController player, CustomizerConfig config)
        {
            _config = config;
            _controllerGradient = screenTouchController;
            _player = player.PlayerVisualPart;

            UpdateSkin();
            ChangeControllerGradient();

            _config.SubscribeToOnSkinChangedEvent(UpdateSkin);
            _config.SubscribeToOnControllerGradientChangedEvent(ChangeControllerGradient);
        }

        private void UpdateSkin()
        {
            _player.Skin = _config.Skin;
        }

        private void ChangeControllerGradient()
        {
            _controllerGradient.ControllerGradient = _config.ControllerGradient;
        }

        public void Dispose()
        {
            _config.UnsubscribeToOnSkinChangedEvent(UpdateSkin);
            _config.UnsubscribeToControllerGradientChangedEvent(ChangeControllerGradient);
        }
    }
}
