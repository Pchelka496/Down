using Core.Enemy;
using Core.SaveSystem;
using Creatures.Player;
using Creatures.Player.Any;
using Customization;
using ScriptableObject;
using ScriptableObject.ModulesConfig;
using ScriptableObject.ModulesConfig.FlightModule;
using ScriptableObject.ModulesConfig.SupportModules;
using ScriptableObject.PickUpItem;
using System.Linq;
using UI;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

namespace Core.Installers
{
    public class GameplaySceneInstaller : MonoInstaller
    {
        [Header("Core Components")]
        [SerializeField] PlayerController _playerController;
        [SerializeField] CameraFacade _camerasController;
        [SerializeField] AirTrailController _airTrailController;
        [SerializeField] Camera _mainCamera;
        [SerializeField] LobbyUIPanelFacade _upgradePanelController;

        [Header("Configuration")]
        [SerializeField] OriginalPlayerModuleConfigs _originalPlayerModulesConfig;
        [SerializeField] PlayerResourcedKeeperConfig _playerResourcedKeeperConfig;
        [SerializeField] CustomizerConfig _customizerConfig;
        [SerializeField] SettingConfig _settingConfig;
        [SerializeField] AssetReference _surfaceController;

        [Header("Audio & Visuals")]
        [SerializeField] PostProcessingController.Initializer _postProcessingControllerInitializer;
        [SerializeField] MusicManager.Initializer _audioManagerInitializer;

        [Header("UI Components")]
        [SerializeField] RewardCounter _rewardCounter;
        [SerializeField] ScreenFader _screenFader;
        [SerializeField] ScreenTouchController _screenTouchController;
        [SerializeField] RepairKitIndicator _repairKitIndicator;
        [SerializeField] HealthIndicator _healthIndicator;
        [SerializeField] BoosterIndicator _boosterIndicator;
        [SerializeField] EmergencyBrakeModuleIndicator _emergencyBrakeModuleIndicator;
        [SerializeField] PlayerResourcedIndicator _playerResourcedIndicator;
        [SerializeField] HUDController _hudController;
        [SerializeField] MainMenu _mainMenu;

        Customizer _customizer;
        GameBootstrapper _gameBootstrapper;
        OptionalPlayerModuleLoader _optionalPlayerModuleLoader;
        MapController _mapController;
        BackgroundController _backgroundController;
        EnemySystemCoordinator _enemyManager;
        PickUpItemManager _pickUpItemManager;
        AudioSourcePool _audioSourcePool;
        EffectController _effectController;
        PlayerPositionMeter _characterPositionMeter;
        PlayerResourcedKeeper _playerResourcedKeeper;
        SaveSystemController _saveSystemController;
        GlobalEventsManager _globalEventsManager;
        PostProcessingController _postProcessingController;
        GameAnalytics _gameAnalytics;
        MusicManager _audioManager;
        PerformanceManager _performanceManager;

        public static DiContainer DiContainer { get; private set; }

        public override void InstallBindings()
        {
            InitializeOrCleanInstaller();
            InitializeDependencies();

            BindCoreComponents();
            BindUIComponents();
            BindConfiguration();
            BindAudioAndVisuals();
            BindAnalytics();
            BindPlayerModuleConfigs();
            BindInterfaces();
        }

        private void InitializeOrCleanInstaller()
        {
            if (DiContainer == null)
            {
                DiContainer = Container;
            }
            else
            {
                if (gameObject.TryGetComponent(out GameplaySceneInstaller existingInstaller))
                {
                    Destroy(existingInstaller);
                }
            }
        }

        private void BindCoreComponents()
        {
            Container.Bind<Controls>().FromNew().AsSingle().NonLazy();
            Container.Bind<PlayerController>().FromInstance(_playerController).AsSingle().NonLazy();
            Container.Bind<MapController>().FromInstance(_mapController).AsSingle().NonLazy();
            Container.Bind<BackgroundController>().FromInstance(_backgroundController).AsSingle().NonLazy();
            Container.Bind<EnemySystemCoordinator>().FromInstance(_enemyManager).AsSingle().NonLazy();
            Container.Bind<PickUpItemManager>().FromInstance(_pickUpItemManager).AsSingle().NonLazy();
            Container.Bind<CameraFacade>().FromInstance(_camerasController).AsSingle().NonLazy();
            Container.Bind<AirTrailController>().FromInstance(_airTrailController).AsSingle().NonLazy();
            Container.Bind<Camera>().FromInstance(_mainCamera).AsSingle().NonLazy();
            Container.Bind<GlobalEventsManager>().FromInstance(_globalEventsManager).AsSingle().NonLazy();
            Container.Bind<PerformanceManager>().FromInstance(_performanceManager).AsSingle().NonLazy();
            Container.Bind<PlayerResourcedKeeper>().FromInstance(_playerResourcedKeeper).AsSingle().NonLazy();
            Container.Bind<SaveSystemController>().FromInstance(_saveSystemController).AsSingle().NonLazy();
            Container.Bind<PlayerPositionMeter>().FromInstance(_characterPositionMeter).AsSingle().NonLazy();
            Container.Bind<OptionalPlayerModuleLoader>().FromInstance(_optionalPlayerModuleLoader).AsSingle().NonLazy();
            Container.Bind<Customizer>().FromInstance(_customizer).AsSingle().NonLazy();
        }

        private void BindUIComponents()
        {
            Container.Bind<RewardCounter>().FromInstance(_rewardCounter).AsSingle().NonLazy();
            Container.Bind<LobbyUIPanelFacade>().FromInstance(_upgradePanelController).AsSingle().NonLazy();
            Container.Bind<ScreenFader>().FromInstance(_screenFader).AsSingle().NonLazy();
            Container.Bind<RepairKitIndicator>().FromInstance(_repairKitIndicator).AsSingle().NonLazy();
            Container.Bind<HealthIndicator>().FromInstance(_healthIndicator).AsSingle().NonLazy();
            Container.Bind<BoosterIndicator>().FromInstance(_boosterIndicator).AsSingle().NonLazy();
            Container.Bind<ScreenTouchController>().FromInstance(_screenTouchController).AsSingle().NonLazy();
            Container.Bind<EmergencyBrakeModuleIndicator>().FromInstance(_emergencyBrakeModuleIndicator).AsSingle().NonLazy();
            Container.Bind<PlayerResourcedIndicator>().FromInstance(_playerResourcedIndicator).AsSingle().NonLazy();
            Container.Bind<HUDController>().FromInstance(_hudController).AsSingle().NonLazy();
            Container.Bind<MainMenu>().FromInstance(_mainMenu).AsSingle().NonLazy();
        }

        private void BindConfiguration()
        {
            Container.Bind<CustomizerConfig>().FromInstance(_customizerConfig).AsSingle().NonLazy();
            Container.Bind<OriginalPlayerModuleConfigs>().FromInstance(_originalPlayerModulesConfig).AsSingle().NonLazy();
            Container.Bind<PlayerResourcedKeeperConfig>().FromInstance(_playerResourcedKeeperConfig).AsSingle().NonLazy();
            Container.Bind<SettingConfig>().FromInstance(_settingConfig).AsSingle().NonLazy();
        }

        private void BindAudioAndVisuals()
        {
            Container.Bind<AudioSourcePool>().FromInstance(_audioSourcePool).AsSingle().NonLazy();
            Container.Bind<EffectController>().FromInstance(_effectController).AsSingle().NonLazy();
            Container.Bind<PostProcessingController>().FromInstance(_postProcessingController).AsSingle().NonLazy();
            Container.Bind<MusicManager>().FromInstance(_audioManager).AsSingle().NonLazy();
        }

        private void BindAnalytics()
        {
            Container.Bind<GameAnalytics>().FromInstance(_gameAnalytics).AsSingle().NonLazy();
        }

        private void BindPlayerModuleConfigs()
        {
            Container.Bind<EngineModuleConfig>().FromInstance(_originalPlayerModulesConfig.EngineModuleConfig).AsSingle().NonLazy();
            Container.Bind<PickerModuleConfig>().FromInstance(_originalPlayerModulesConfig.PickerModuleConfig).AsSingle().NonLazy();
            Container.Bind<RotationModuleConfig>().FromInstance(_originalPlayerModulesConfig.StabilizationModuleConfig).AsSingle().NonLazy();
            Container.Bind<HealthModuleConfig>().FromInstance(_originalPlayerModulesConfig.HealthModuleConfig).AsSingle().NonLazy();
            Container.Bind<EmergencyBrakeModuleConfig>().FromInstance(_originalPlayerModulesConfig.EmergencyBrakeModuleConfig).AsSingle().NonLazy();
            Container.Bind<AirBrakeModuleConfig>().FromInstance(_originalPlayerModulesConfig.AirBrakeModuleConfig).AsSingle().NonLazy();
            Container.Bind<FastTravelModuleConfig>().FromInstance(_originalPlayerModulesConfig.FastTravelModuleConfig).AsSingle().NonLazy();
        }

        private void BindInterfaces()
        {
            Container.Bind<IAdManager>().To<AdModManager>().AsTransient().Lazy();
            Container.Bind<IAnalyticsManager>().To<UnityAnalyticsManager>().AsSingle().NonLazy();
            Container.Bind<IAudioSettingContainer>().FromInstance(_settingConfig).AsSingle().NonLazy();
            Container.Bind<ILanguageContainer>().FromInstance(_settingConfig).AsSingle().NonLazy();
            Container.Bind<IRoundResultTracker>().FromInstance(_globalEventsManager).AsSingle().NonLazy();
        }

        private void InitializeDependencies()
        {
            var dependenciesObject = new GameObject("++Dependencies++");

            _customizer = new();
            _gameBootstrapper = new();
            _optionalPlayerModuleLoader = new();
            _mapController = new(_surfaceController);
            _backgroundController = new();
            _enemyManager = new(AttachToGameObject(dependenciesObject, "++Enemies++").transform);
            _pickUpItemManager = new(AttachToGameObject(dependenciesObject, "++PickUpItems++").transform);
            _audioSourcePool = new(AttachToGameObject(dependenciesObject, "++Audio++").transform);
            _effectController = new(AttachToGameObject(dependenciesObject, "++Effects++").transform);
            _characterPositionMeter = new();
            _playerResourcedKeeper = new(_playerResourcedKeeperConfig, _playerResourcedIndicator);
            _saveSystemController = new(GetAllDataForSave());
            _globalEventsManager = new(_screenFader);
            _postProcessingController = new(_postProcessingControllerInitializer);
            _gameAnalytics = new(
                moneyTracker: _rewardCounter,
                collisionTracker: _playerController.HealthModule,
                resultTracker: _globalEventsManager
            );
            _audioManager = new(_audioManagerInitializer);
            _performanceManager = new();
        }

        private IHaveDataForSave[] GetAllDataForSave()
        {
            var playerModules = _originalPlayerModulesConfig.GetAllConfigsAsBase()
                .OfType<IHaveDataForSave>()
                .ToArray();

            return new IHaveDataForSave[]
            {
             _playerResourcedKeeperConfig,
             _customizerConfig,
             _settingConfig
            }
            .Concat(playerModules)
            .ToArray();
        }

        private GameObject AttachToGameObject(GameObject parent, string gameObjectName)
        {
            var instanceGameObject = new GameObject(gameObjectName);
            instanceGameObject.transform.SetParent(parent.transform);

            return instanceGameObject;
        }

        private void Awake()
        {
            Container.Inject(_gameBootstrapper);
            Container.Inject(_optionalPlayerModuleLoader);
            Container.Inject(_mapController);
            Container.Inject(_backgroundController);
            Container.Inject(_enemyManager);
            Container.Inject(_pickUpItemManager);
            Container.Inject(_audioSourcePool);
            Container.Inject(_effectController);
            Container.Inject(_characterPositionMeter);
            Container.Inject(_customizer);
            Container.Inject(_playerResourcedKeeper);
            Container.Inject(_saveSystemController);
            Container.Inject(_globalEventsManager);
            Container.Inject(_postProcessingController);
            Container.Inject(_gameAnalytics);
            Container.Inject(_audioManager);
            Container.Inject(_performanceManager);
        }

        private void OnDestroy()
        {
            _gameBootstrapper?.Dispose();
            _customizer?.Dispose();
            _optionalPlayerModuleLoader?.Dispose();
            _mapController?.Dispose();
            _backgroundController?.Dispose();
            _enemyManager?.Dispose();
            _playerResourcedKeeper?.Dispose();
            _saveSystemController?.Dispose();
            _pickUpItemManager?.Dispose();
            _postProcessingController?.Dispose();
            _gameAnalytics?.Dispose();
            _audioManager?.Dispose();
            _audioSourcePool?.Dispose();
            _performanceManager?.Dispose();
        }

        [System.Serializable]
        public record OriginalPlayerModuleConfigs
        {
            [field: SerializeField] public EngineModuleConfig EngineModuleConfig { get; private set; }
            [field: SerializeField] public PickerModuleConfig PickerModuleConfig { get; private set; }
            [field: SerializeField] public RotationModuleConfig StabilizationModuleConfig { get; private set; }
            [field: SerializeField] public HealthModuleConfig HealthModuleConfig { get; private set; }
            [field: SerializeField] public EmergencyBrakeModuleConfig EmergencyBrakeModuleConfig { get; private set; }
            [field: SerializeField] public AirBrakeModuleConfig AirBrakeModuleConfig { get; private set; }
            [field: SerializeField] public FastTravelModuleConfig FastTravelModuleConfig { get; private set; }

            public BaseModuleConfig[] GetAllConfigsAsBase()
            {
                return new BaseModuleConfig[]
                {
                    EngineModuleConfig,
                    StabilizationModuleConfig,
                    HealthModuleConfig,
                    EmergencyBrakeModuleConfig,
                    AirBrakeModuleConfig,
                    PickerModuleConfig,
                    FastTravelModuleConfig
                };
            }
        }
    }
}
