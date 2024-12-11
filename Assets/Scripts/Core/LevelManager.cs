using Core.Enemy;
using Creatures.Player;
using Cysharp.Threading.Tasks;
using ScriptableObject;
using UI;
using UnityEngine;
using Zenject;

namespace Core
{
    public class LevelManager : System.IDisposable
    {
        public const float PLAYER_START_Y_POSITION = 99989.1f;
        public const float PLAYER_START_X_POSITION = 0;

        public const string CONFIG_ADDRESS = "ScriptableObject/LevelManager/LevelManagerConfig";
        int _targetFrameRate = 90;
        LevelManagerConfig _config;

        EnemyManager _enemyManager;
        MapController _mapController;
        BackgroundController _backgroundController;
        PickUpItemManager _rewardManager;
        ScreenFader _screenFader;
        event System.Action DisposeEvents;

        [Inject]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
        private void Construct(
            GlobalEventsManager globalEventsManager,
            MapController mapController,
            EnemyManager enemyManager,
            BackgroundController backgroundController,
            PickUpItemManager rewardManager,
            ScreenFader screenFader)
        {
            _mapController = mapController;
            _enemyManager = enemyManager;
            _backgroundController = backgroundController;
            _rewardManager = rewardManager;
            _screenFader = screenFader;

            LoadConfig().Forget();

            globalEventsManager.SubscribeToRoundStarted(RoundStart);
            globalEventsManager.SubscribeToRoundEnded(RoundEnd);
            globalEventsManager.SubscribeToWarpStarted(WarpStart);
            var taskId = globalEventsManager.AddTransitionTask(CleaningAndPlayerTransition, false);

            DisposeEvents += () => globalEventsManager?.UnsubscribeFromWarpStarted(WarpStart);
            DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundStarted(RoundStart);
            DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundEnded(RoundEnd);
            DisposeEvents += () => globalEventsManager?.RemoveTransitionTask(taskId);
        }

        private async UniTask LoadConfig()
        {
            var loadOperationData = await AddressableLouderHelper.LoadAssetAsync<LevelManagerConfig>(CONFIG_ADDRESS);

            _config = loadOperationData.Handle.Result;

            if (_config == null)
            {
                Debug.LogError("Failed to load OptionalPlayerModuleLoaderConfig.");
                return;
            }

            InitializeAnyComponents();
            FirstSettings();
        }

        private void FirstSettings()
        {
            _screenFader.FadeToClear().Forget();
            _targetFrameRate = _config.TargetFrameRate;

            Application.targetFrameRate = _targetFrameRate;
        }

        private void InitializeAnyComponents()
        {
            _mapController.Initialize(_config.MapControllerConfig);
            _backgroundController.Initialize(_config.BackgroundControllerConfig);
            _enemyManager.Initialize(_config.EnemyManagerConfig).Forget();
            _rewardManager.Initialize(_config.RewardManagerConfig);
        }

        private void RoundStart()
        {

        }

        private void RoundEnd()
        {

        }

        private UniTask CleaningAndPlayerTransition()
        {
            System.GC.Collect();
            System.GC.WaitForPendingFinalizers();

            return UniTask.CompletedTask;
        }

        private void WarpStart()
        {

        }

        public void Dispose()
        {
            DisposeEvents?.Invoke();
        }
    }
}