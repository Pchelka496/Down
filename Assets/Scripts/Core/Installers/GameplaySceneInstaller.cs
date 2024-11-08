using UnityEngine;
using Zenject;

public class GameplaySceneInstaller : MonoInstaller
{
    [SerializeField] CharacterController _playerController;
    [SerializeField] LevelManager _levelManager;
    [SerializeField] EnemyManager _enemyManager;
    [SerializeField] MapController _mapController;
    [SerializeField] BackgroundController _backgroundController;
    [SerializeField] RewardManager _rewardManager;
    [SerializeField] CamerasController _camerasController;
    [SerializeField] AirTrailController _airTrailController;
    [SerializeField] Camera _mainCamera;
    [SerializeField] RewardCounter _rewardCounter;
    [SerializeField] UpgradePanelController _upgradePanelController;
    [SerializeField] AudioSourcePool _audioSourcePool;
    [SerializeField] EffectController _effectController;
    [SerializeField] ScreenFader _screenFader;
    [SerializeField] BoosterIndicator _boosterIndicator;
    [SerializeField] EnumLanguage _enumLanguage;
    [SerializeField] PlayerModuleConfigs _playerModulesConfig;

    public static DiContainer DiContainer { get; private set; }

    public override void InstallBindings()
    {
        InitializeOrCleanInstaller();
        Container.Bind<Controls>().FromNew().AsSingle().NonLazy();

        Container.Bind<CharacterController>().FromInstance(_playerController).AsSingle().NonLazy();
        Container.Bind<MapController>().FromInstance(_mapController).AsSingle().NonLazy();
        Container.Bind<BackgroundController>().FromInstance(_backgroundController).AsSingle().NonLazy();
        Container.Bind<EnemyManager>().FromInstance(_enemyManager).AsSingle().NonLazy();
        Container.Bind<RewardManager>().FromInstance(_rewardManager).AsSingle().NonLazy();
        Container.Bind<CamerasController>().FromInstance(_camerasController).AsSingle().NonLazy();
        Container.Bind<RewardCounter>().FromInstance(_rewardCounter).AsSingle().NonLazy();
        Container.Bind<UpgradePanelController>().FromInstance(_upgradePanelController).AsSingle().NonLazy();
        Container.Bind<AudioSourcePool>().FromInstance(_audioSourcePool).AsSingle().NonLazy();
        Container.Bind<EffectController>().FromInstance(_effectController).AsSingle().NonLazy();
        Container.Bind<AirTrailController>().FromInstance(_airTrailController).AsSingle().NonLazy();
        Container.Bind<Camera>().FromInstance(_mainCamera).AsSingle().NonLazy();
        Container.Bind<EnumLanguage>().FromInstance(_enumLanguage).AsSingle().NonLazy();
        Container.Bind<ScreenFader>().FromInstance(_screenFader).AsSingle().NonLazy();
        Container.Bind<BoosterIndicator>().FromInstance(_boosterIndicator).AsSingle().NonLazy();
        Container.Bind<LevelManager>().FromInstance(_levelManager).AsSingle().NonLazy();
        Container.Bind<PlayerModuleConfigs>().FromInstance(_playerModulesConfig).AsSingle().NonLazy();

        BindPlayerModuleConfigs();
    }

    private void InitializeOrCleanInstaller()
    {
        if (DiContainer == null)
        {
            DiContainer = Container;
        }
        else
        {
            if (gameObject.TryGetComponent<GameplaySceneInstaller>(out var thisComponent))
            {
                Destroy(thisComponent);
            }
        }
    }

    private void BindPlayerModuleConfigs()
    {
        Container.Bind<EngineModulePowerConfig>()
            .FromInstance(ScriptableObject.Instantiate(_playerModulesConfig.EngineModulePowerConfig))
            .AsSingle().NonLazy();

        Container.Bind<PickerModuleConfig>()
            .FromInstance(ScriptableObject.Instantiate(_playerModulesConfig.PickerModuleConfig))
            .AsSingle().NonLazy();

        Container.Bind<EngineModuleAccelerationSpeedConfig>()
           .FromInstance(ScriptableObject.Instantiate(_playerModulesConfig.EngineModuleAccelerationSpeedConfig))
           .AsSingle().NonLazy();

        Container.Bind<StabilizationModuleConfig>()
            .FromInstance(ScriptableObject.Instantiate(_playerModulesConfig.StabilizationModuleConfig))
            .AsSingle().NonLazy();

        Container.Bind<HealthModuleConfig>()
            .FromInstance(ScriptableObject.Instantiate(_playerModulesConfig.HealthModuleConfig))
            .AsSingle().NonLazy();

        Container.Bind<EmergencyBrakeModuleConfig>()
            .FromInstance(ScriptableObject.Instantiate(_playerModulesConfig.EmergencyBrakeModuleConfig))
            .AsSingle().NonLazy();

        Container.Bind<AirBrakeModuleConfig>()
            .FromInstance(ScriptableObject.Instantiate(_playerModulesConfig.AirBrakeModuleConfig))
            .AsSingle().NonLazy();

        Container.Bind<BoosterModuleConfig>()
            .FromInstance(ScriptableObject.Instantiate(_playerModulesConfig.BoosterModuleConfig))
            .AsSingle().NonLazy();
    }

    [System.Serializable]
    public record PlayerModuleConfigs
    {
        [field: SerializeField] public EngineModulePowerConfig EngineModulePowerConfig { get; private set; }
        [field: SerializeField] public PickerModuleConfig PickerModuleConfig { get; private set; }
        [field: SerializeField] public EngineModuleAccelerationSpeedConfig EngineModuleAccelerationSpeedConfig { get; private set; }
        [field: SerializeField] public StabilizationModuleConfig StabilizationModuleConfig { get; private set; }
        [field: SerializeField] public HealthModuleConfig HealthModuleConfig { get; private set; }
        [field: SerializeField] public EmergencyBrakeModuleConfig EmergencyBrakeModuleConfig { get; private set; }
        [field: SerializeField] public AirBrakeModuleConfig AirBrakeModuleConfig { get; private set; }
        [field: SerializeField] public BoosterModuleConfig BoosterModuleConfig { get; private set; }

        public BaseModuleConfig[] GetAllConfigsAsBase()
        {
            return new BaseModuleConfig[]
            {
            EngineModulePowerConfig,
            EngineModuleAccelerationSpeedConfig,
            StabilizationModuleConfig,
            HealthModuleConfig,
            EmergencyBrakeModuleConfig,
            AirBrakeModuleConfig,
            BoosterModuleConfig,
            PickerModuleConfig
            };
        }

    }

}

