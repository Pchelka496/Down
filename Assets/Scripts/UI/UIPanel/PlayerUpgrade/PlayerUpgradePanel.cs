using Creatures.Player;
using UnityEngine;
using static Core.Installers.GameplaySceneInstaller;

namespace UI.UIPanel.PlayerUpgrade
{
    [RequireComponent(typeof(UpgradePanelVisualController))]
    public class PlayerUpgradePanel : MonoBehaviour, IUIPanel
    {
        [SerializeField] RectTransform _baseRectTransform;
        [SerializeField] UpgradePanelVisualController _visualController;

        [SerializeField] SoundPlayerRandomPitch _unsuccessfulSoundPlayer;
        [SerializeField] SoundPlayerRandomPitch _successfulSoundPlayer;

        [SerializeField] EngineUpdater _engineUpdater;
        [SerializeField] HealthModuleUpdater _healthModuleUpdater;
        [SerializeField] PickerModuleUpdater _pieceModuleUpdater;
        [SerializeField] AirBrakeUpdater _airBrakeUpdater;
        [SerializeField] RotationModuleUpdater _stabilizationModuleUpdater;
        [SerializeField] EmergencyBrakeUpdater _emergencyBrakeUpdater;

        [SerializeField] RectTransform _playerViewUpgradePosition;

        PlayerController _player;
        OriginalPlayerModuleConfigs _configs;
        PlayerResourcedKeeper _rewardKeeper;

        public UpgradePanelVisualController VisualController => _visualController;
        public PlayerController Player => _player;
        public Vector2 PlayerViewUpgradePosition => _playerViewUpgradePosition.position;

        [Zenject.Inject]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:",Justification = "<>")]
        private void Construct(PlayerResourcedKeeper rewardKeeper,
                               OriginalPlayerModuleConfigs configs,
                               AudioSourcePool audioSourcePool,
                               PlayerController player)
        {
            _unsuccessfulSoundPlayer.Initialize(audioSourcePool);
            _successfulSoundPlayer.Initialize(audioSourcePool);

            _rewardKeeper = rewardKeeper;
            _configs = configs;
            _player = player;
        }

        private void Start()
        {
            _engineUpdater.Initialize(_configs.EngineModuleConfig, this);
            _healthModuleUpdater.Initialize(_configs.HealthModuleConfig, this);
            _pieceModuleUpdater.Initialize(_configs.PickerModuleConfig, this);
            _airBrakeUpdater.Initialize(_configs.AirBrakeModuleConfig, this);
            _stabilizationModuleUpdater.Initialize(_configs.StabilizationModuleConfig, this);
            _emergencyBrakeUpdater.Initialize(_configs.EmergencyBrakeModuleConfig, this);
        }

        void IUIPanel.Open()
        {
            gameObject.SetActive(true);
            _player.OpenUIPanel();

            MovePlayerToUpgradeView();
        }

        void IUIPanel.Close()
        {
            gameObject.SetActive(false);
            _player.CloseUIPanel();
        }

        private void MovePlayerToUpgradeView()
        {
            _player.Rb.velocity = Vector3.zero;

            _player.transform.position = _playerViewUpgradePosition.position;
        }

        public void OnBackButtonClick()
        {
            switch (_visualController.CurrentViewMode)
            {
                case UpgradePanelVisualController.ViewMode.Basic:
                    {
                        ((IUIPanel)this).Close();

                        break;
                    }
                case UpgradePanelVisualController.ViewMode.Detailed:
                    {
                        _visualController.EndViewDetailedInformation();

                        break;
                    }
            }
        }

        public bool UpgradeLevelCheck(int pointsNeeded, int currentLevel, int maxLevel)
        {
            if (_rewardKeeper.TryDecreaseMoney(pointsNeeded, false) && currentLevel < maxLevel)
            {
                HandleSuccessfulUpgrade(pointsNeeded);
                return true;
            }
            else
            {
                HandleUnsuccessfulUpgrade();
                return false;
            }
        }

        public bool DowngradeLevelCheck(int currentLevel)
        {
            if (currentLevel > 0)
            {
                HandleSuccessfulUpgrade(0);
                return true;
            }
            else
            {
                HandleUnsuccessfulUpgrade();
                return false;
            }
        }

        private void HandleSuccessfulUpgrade(int pointsNeeded)
        {
            _successfulSoundPlayer.PlayNextSound();

            _rewardKeeper.TryDecreaseMoney(pointsNeeded);
        }

        private void HandleUnsuccessfulUpgrade()
        {
            _unsuccessfulSoundPlayer.PlayNextSound();
        }
    }
}