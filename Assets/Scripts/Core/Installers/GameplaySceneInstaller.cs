using UnityEngine;
using Zenject;

public class GameplaySceneInstaller : MonoInstaller
{
    [SerializeField] PlayerController _playerController;
    [SerializeField] CameraFacade _camerasController;
    [SerializeField] AirTrailController _airTrailController;
    [SerializeField] Camera _mainCamera;
    [SerializeField] LobbyUIPanelFacade _upgradePanelController;
    [SerializeField] EnumLanguage _language;
    [SerializeField] PlayerModuleConfigs _playerModulesConfig;
    [SerializeField] RewardKeeperConfig _rewardKeeperConfig;
    [Header("UI")]
    [SerializeField] RewardCounter _rewardCounter;
    [SerializeField] ScreenFader _screenFader;
    [SerializeField] ScreenTouchController _screenTouchController;
    [SerializeField] RepairKitIndicator _repairKitIndicator;
    [SerializeField] HealthIndicator _healthIndicator;
    [SerializeField] BoosterIndicator _boosterIndicator;
    [SerializeField] CustomizerConfig _customizerConfig;

    Customizer _customizer;
    LevelManager _levelManager;
    OptionalPlayerModuleLoader _optionalPlayerModuleLoader;
    MapController _mapController;
    BackgroundController _backgroundController;
    EnemyManager _enemyManager;
    PickUpItemManager _pickUpItemManager;
    AudioSourcePool _audioSourcePool;
    EffectController _effectController;
    CharacterPositionMeter _characterPositionMeter;
    RewardKeeper _rewardKeeper;
    SaveSystemController _saveSystemController;

    public static DiContainer DiContainer { get; private set; }

    public override void InstallBindings()
    {
        InitializeOrCleanInstaller();
        InitializeDependencies();

        Container.Bind<Controls>().FromNew().AsSingle().NonLazy();

        Container.Bind<PlayerController>().FromInstance(_playerController).AsSingle().NonLazy();
        Container.Bind<MapController>().FromInstance(_mapController).AsSingle().NonLazy();
        Container.Bind<BackgroundController>().FromInstance(_backgroundController).AsSingle().NonLazy();
        Container.Bind<EnemyManager>().FromInstance(_enemyManager).AsSingle().NonLazy();
        Container.Bind<PickUpItemManager>().FromInstance(_pickUpItemManager).AsSingle().NonLazy();
        Container.Bind<CameraFacade>().FromInstance(_camerasController).AsSingle().NonLazy();
        Container.Bind<RewardCounter>().FromInstance(_rewardCounter).AsSingle().NonLazy();
        Container.Bind<LobbyUIPanelFacade>().FromInstance(_upgradePanelController).AsSingle().NonLazy();
        Container.Bind<AudioSourcePool>().FromInstance(_audioSourcePool).AsSingle().NonLazy();
        Container.Bind<EffectController>().FromInstance(_effectController).AsSingle().NonLazy();
        Container.Bind<AirTrailController>().FromInstance(_airTrailController).AsSingle().NonLazy();
        Container.Bind<Camera>().FromInstance(_mainCamera).AsSingle().NonLazy();
        Container.Bind<EnumLanguage>().FromInstance(_language).AsSingle().NonLazy();
        Container.Bind<ScreenFader>().FromInstance(_screenFader).AsSingle().NonLazy();
        Container.Bind<RepairKitIndicator>().FromInstance(_repairKitIndicator).AsSingle().NonLazy();
        Container.Bind<HealthIndicator>().FromInstance(_healthIndicator).AsSingle().NonLazy();
        Container.Bind<BoosterIndicator>().FromInstance(_boosterIndicator).AsSingle().NonLazy();
        Container.Bind<ScreenTouchController>().FromInstance(_screenTouchController).AsSingle().NonLazy();
        Container.Bind<LevelManager>().FromInstance(_levelManager).AsSingle().NonLazy();
        Container.Bind<PlayerModuleConfigs>().FromInstance(_playerModulesConfig).AsSingle().NonLazy();
        Container.Bind<OptionalPlayerModuleLoader>().FromInstance(_optionalPlayerModuleLoader).AsSingle().NonLazy();
        Container.Bind<CustomizerConfig>().FromInstance(_customizerConfig).AsSingle().NonLazy();
        Container.Bind<Customizer>().FromInstance(_customizer).AsSingle().NonLazy();
        Container.Bind<CharacterPositionMeter>().FromInstance(_characterPositionMeter).AsSingle().NonLazy();
        Container.Bind<RewardKeeper>().FromInstance(_rewardKeeper).AsSingle().NonLazy();

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

    private void InitializeDependencies()
    {
        var dependenciesObject = new GameObject("Dependencies");

        _optionalPlayerModuleLoader = new();
        _mapController = new();
        _backgroundController = new();
        _enemyManager = new(AttachToGameObject(dependenciesObject, "++Enemies++").transform);
        _levelManager = new();
        _pickUpItemManager = new(AttachToGameObject(dependenciesObject, "++PickUpItems++").transform);
        _audioSourcePool = new(AttachToGameObject(dependenciesObject, "++Audio++").transform);
        _effectController = new(AttachToGameObject(dependenciesObject, "++Effects++").transform);
        _customizer = new();
        _characterPositionMeter = new();
        _rewardKeeper = new(_rewardKeeperConfig);
        _saveSystemController = new(_rewardKeeperConfig, _customizerConfig);
    }

    private GameObject AttachToGameObject(GameObject parent, string name)
    {
        var instanceGameObject = new GameObject(name);
        instanceGameObject.transform.SetParent(parent.transform);

        return instanceGameObject;
    }

    private void Awake()
    {
        Container.Inject(_levelManager);
        Container.Inject(_optionalPlayerModuleLoader);
        Container.Inject(_mapController);
        Container.Inject(_backgroundController);
        Container.Inject(_enemyManager);
        Container.Inject(_pickUpItemManager);
        Container.Inject(_audioSourcePool);
        Container.Inject(_effectController);
        Container.Inject(_characterPositionMeter);
        Container.Inject(_customizer);
        Container.Inject(_rewardKeeper);
    }

    private void OnDestroy()
    {
        _backgroundController.Dispose();
        _enemyManager.Dispose();
        _customizer.Dispose();
        _saveSystemController.Dispose();
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

