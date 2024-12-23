using Core.Enemy;
using Cysharp.Threading.Tasks;
using ScriptableObject;
using UnityEngine;
using Zenject;

namespace Core
{
    public class GameBootstrapper : System.IDisposable
    {
        public const string CONFIG_ADDRESS = "Assets/Resources_moved/ScriptableObject/GameBootstrapper/GameBootstrapperConfig.asset";

        GameBootstrapperConfig _config;

        EnemySystemCoordinator _enemySystemCoordinator;
        MapController _mapController;
        BackgroundController _backgroundController;
        PickUpItemManager _pickUpItemManager;
        PerformanceManager _performanceManager;

        event System.Action DisposeEvents;

        [Inject]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
        private void Construct(
            MapController mapController,
            EnemySystemCoordinator enemyManager,
            BackgroundController backgroundController,
            PickUpItemManager rewardManager,
            PerformanceManager performanceManager)
        {
            _mapController = mapController;
            _enemySystemCoordinator = enemyManager;
            _backgroundController = backgroundController;
            _pickUpItemManager = rewardManager;
            _performanceManager = performanceManager;

            LoadConfig().Forget();
        }

        private async UniTask LoadConfig()
        {
            var loadOperationData = await AddressableLouderHelper.LoadAssetAsync<GameBootstrapperConfig>(CONFIG_ADDRESS);

            _config = loadOperationData.Handle.Result;

            if (_config == null)
            {
                Debug.LogError("Failed to load .");
                return;
            }

            InitializeAnyComponents();
        }

        private void InitializeAnyComponents()
        {
            _mapController.Initialize(_config.MapControllerConfig);
            _backgroundController.Initialize(_config.BackgroundControllerConfig);
            _enemySystemCoordinator.Initialize(_config.EnemyManagerConfig).Forget();
            _pickUpItemManager.Initialize(_config.RewardManagerConfig);
            _performanceManager.Initialize(_config.TargetFrameRate);
        }

        public void Dispose()
        {
            DisposeEvents?.Invoke();
        }
    }
}
