using UnityEngine;
using Zenject;

public class GameplaySceneInstaller : MonoInstaller
{
    [SerializeField] CharacterController _playerController;
    [SerializeField] OptionalPlayerModuleLoader _optionalPlayerModuleLoader;
    [SerializeField] LevelManager _levelManager;
    [SerializeField] EnemyManager _enemyManager;
    [SerializeField] MapController _mapController;
    [SerializeField] BackgroundController _backgroundController;
    [SerializeField] PickUpItemManager _rewardManager;
    [SerializeField] CamerasController _camerasController;
    [SerializeField] AirTrailController _airTrailController;
    [SerializeField] Camera _mainCamera;
    [SerializeField] RewardCounter _rewardCounter;
    [SerializeField] UpgradePanelController _upgradePanelController;
    [SerializeField] AudioSourcePool _audioSourcePool;
    [SerializeField] EffectController _effectController;
    [SerializeField] ScreenFader _screenFader;
    [SerializeField] RepairKitIndicator _repairKitIndicator;
    [SerializeField] HealthIndicator _healthIndicator;
    [SerializeField] ScreenTouchController _screenTouchController;
    [SerializeField] EnumLanguage _enumLanguage;
    [SerializeField] PlayerModuleConfigs _playerModulesConfig;

    public static DiContainer DiContainer { get; private set; }

    public override void InstallBindings()
    {
        InitializeOrCleanInstaller();
        Container.Bind<Controls>().FromNew().AsSingle().NonLazy();

        Container.Bind<CharacterController>().FromInstance(_playerController).AsSingle().NonLazy();
        Container.Bind<OptionalPlayerModuleLoader>().FromInstance(_optionalPlayerModuleLoader).AsSingle().NonLazy();
        Container.Bind<MapController>().FromInstance(_mapController).AsSingle().NonLazy();
        Container.Bind<BackgroundController>().FromInstance(_backgroundController).AsSingle().NonLazy();
        Container.Bind<EnemyManager>().FromInstance(_enemyManager).AsSingle().NonLazy();
        Container.Bind<PickUpItemManager>().FromInstance(_rewardManager).AsSingle().NonLazy();
        Container.Bind<CamerasController>().FromInstance(_camerasController).AsSingle().NonLazy();
        Container.Bind<RewardCounter>().FromInstance(_rewardCounter).AsSingle().NonLazy();
        Container.Bind<UpgradePanelController>().FromInstance(_upgradePanelController).AsSingle().NonLazy();
        Container.Bind<AudioSourcePool>().FromInstance(_audioSourcePool).AsSingle().NonLazy();
        Container.Bind<EffectController>().FromInstance(_effectController).AsSingle().NonLazy();
        Container.Bind<AirTrailController>().FromInstance(_airTrailController).AsSingle().NonLazy();
        Container.Bind<Camera>().FromInstance(_mainCamera).AsSingle().NonLazy();
        Container.Bind<EnumLanguage>().FromInstance(_enumLanguage).AsSingle().NonLazy();
        Container.Bind<ScreenFader>().FromInstance(_screenFader).AsSingle().NonLazy();
        Container.Bind<RepairKitIndicator>().FromInstance(_repairKitIndicator).AsSingle().NonLazy();
        Container.Bind<HealthIndicator>().FromInstance(_healthIndicator).AsSingle().NonLazy();
        Container.Bind<ScreenTouchController>().FromInstance(_screenTouchController).AsSingle().NonLazy();
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
        Container.Bind<EngineModuleConfig>()
            .FromInstance(ScriptableObject.Instantiate(_playerModulesConfig.EngineModuleConfig))
            .AsSingle().NonLazy();

        Container.Bind<PickerModuleConfig>()
            .FromInstance(ScriptableObject.Instantiate(_playerModulesConfig.PickerModuleConfig))
            .AsSingle().NonLazy();

        Container.Bind<RotationModuleConfig>()
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
    }

    [System.Serializable]
    public record PlayerModuleConfigs
    {
        [field: SerializeField] public EngineModuleConfig EngineModuleConfig { get; private set; }
        [field: SerializeField] public PickerModuleConfig PickerModuleConfig { get; private set; }
        [field: SerializeField] public RotationModuleConfig StabilizationModuleConfig { get; private set; }
        [field: SerializeField] public HealthModuleConfig HealthModuleConfig { get; private set; }
        [field: SerializeField] public EmergencyBrakeModuleConfig EmergencyBrakeModuleConfig { get; private set; }
        [field: SerializeField] public AirBrakeModuleConfig AirBrakeModuleConfig { get; private set; }

        public BaseModuleConfig[] GetAllConfigsAsBase()
        {
            return new BaseModuleConfig[]
            {
            EngineModuleConfig,
            StabilizationModuleConfig,
            HealthModuleConfig,
            EmergencyBrakeModuleConfig,
            AirBrakeModuleConfig,
            PickerModuleConfig
            };
        }

    }

}

