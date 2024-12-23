using System.Runtime.CompilerServices;
using Core.Installers;
using Creatures.Player;
using Cysharp.Threading.Tasks;
using ScriptableObject.Enemy;
using UnityEngine;
using Zenject;

namespace Core.Enemy
{
    public class EnemySystemCoordinator : System.IDisposable
    {
        public const int ENEMY_LAYER_INDEX = 8;
        public const float ENEMY_SPAWN_X_POSITION = PlayerController.PLAYER_START_X_POSITION + 99000f;
        public const float ENEMY_SPAWN_Y_POSITION = PlayerController.PLAYER_START_Y_POSITION + 1000f;

        const float DELAY_BEFORE_START_CHALLENGE_SPAWNING = 3f;
        const float DELAY_BEFORE_CHALLENGE_RESPAWN = 2f;

        readonly Transform _enemyParent;

        EnemyManagerConfig _config;
        EnemyRegionUpdater _enemyRegionUpdater;
        EnemyCoreFactory _enemyCoreFactory;
        EnemyVisualPartController _enemyVisualPartFactory;
        EnemyVisualPartController _challengeVisualPartFactory;
        EnemyMovementController _enemyMovementController;

        EnemyCore[] _enemyCore;

        event System.Action DisposeEvents;

        public EnemySystemCoordinator(Transform enemyParent)
        {
            _enemyParent = enemyParent;
        }

        [Inject]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
        private void Construct(GlobalEventsManager globalEventsManager)
        {
            globalEventsManager.SubscribeToRoundStarted(RoundStart);
            globalEventsManager.SubscribeToRoundEnded(RoundEnd);

            DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundStarted(RoundStart);
            DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundEnded(RoundEnd);
        }

        public async UniTaskVoid Initialize(EnemyManagerConfig config)
        {
            _config = config;
            _enemyCore = new EnemyCore[_config.AllEnemyCount];

            var diContainer = GameplaySceneInstaller.DiContainer;

            _enemyCoreFactory = diContainer.Instantiate<EnemyCoreFactory>();
            _enemyCoreFactory.Initialize(config, _enemyCore, _enemyParent);

            _challengeVisualPartFactory = diContainer.Instantiate<EnemyVisualPartController>();
            _challengeVisualPartFactory.Initialize(_config.ChallengeCount);

            _enemyVisualPartFactory = diContainer.Instantiate<EnemyVisualPartController>();
            _enemyVisualPartFactory.Initialize(_config.EnemyCount);

            _enemyRegionUpdater = diContainer.Instantiate<EnemyRegionUpdater>();
            _enemyRegionUpdater.Initialize(_config, OnEnemyRegionUpdated);

            _enemyMovementController = diContainer.Instantiate<EnemyMovementController>();

            var enemyControllerRoundStart = await _enemyCoreFactory.CreateEnemies();

            _enemyMovementController.Initialize(
                _config.AllEnemyCount,
                enemyControllerRoundStart.Transforms,
                enemyControllerRoundStart.Speeds,
                enemyControllerRoundStart.EnumMotionPatterns,
                enemyControllerRoundStart.MotionCharacteristic,
                enemyControllerRoundStart.IsolationDistance
                );
        }

        private void RoundStart()
        {
            CreateChallenge().Forget();
        }

        private async UniTaskVoid CreateChallenge()
        {
            await UniTask.WaitForSeconds(DELAY_BEFORE_START_CHALLENGE_SPAWNING);

            _challengeVisualPartFactory.SetEnemies(_config.ChallengeEnemies);

            for (int i = 0; i < _challengeVisualPartFactory.EnemyCount; i++)
            {
                var enemyInfo = await _challengeVisualPartFactory.UpdateVisualPart(i);
                var indexInManager = _config.AllEnemyCount - 1 - i;

                EnemySettings(indexInManager,
                              _enemyCore[indexInManager],
                              enemyInfo.EnemyVisualPart,
                              enemyInfo.Enemy
                              );
            }
        }

        private void RoundEnd()
        {
            ReleaseLoadedResources();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResetAllEnemyPosition()
        {
            Vector2 spawnPosition = new(ENEMY_SPAWN_X_POSITION, ENEMY_SPAWN_Y_POSITION);

            foreach (var enemyCore in _enemyCore)
            {
                enemyCore.transform.position = spawnPosition;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ResetEnemyPosition(int index)
        {
            _enemyCore[index].transform.position = new(ENEMY_SPAWN_X_POSITION, ENEMY_SPAWN_Y_POSITION);
        }

        private void OnEnemyRegionUpdated(EnemyRegionConfig enemyRegion) => UpdateEnemyRegion(enemyRegion).Forget();

        private async UniTaskVoid UpdateEnemyRegion(EnemyRegionConfig enemyRegion)
        {
            var enemies = _enemyVisualPartFactory.UpdateEnemyRegion(enemyRegion.Enemies);

            var enemyVisualParts = await _enemyVisualPartFactory.LoadAllEnemyVisualParts();

            AssignEnemySettings(enemies, enemyVisualParts);
        }

        private void AssignEnemySettings(EnemyConfig.Enemy[] enemies, EnemyVisualPart[] enemyVisualParts)
        {
            for (int i = 0; i < enemies.Length; i++)
            {
                var enemyCharacteristics = enemies[i];

                EnemySettings(
                    index: i,
                    enemyCore: _enemyCore[i],
                    visualPart: enemyVisualParts[i],
                    enemyCharacteristics: enemyCharacteristics
                    );
            }

            ResetAllEnemyPosition();
        }

        private void EnemySettings(int index,
                                   EnemyCore enemyCore,
                                   EnemyVisualPart visualPart,
                                   EnemyConfig.Enemy enemyCharacteristics)
        {
            if (visualPart != null)
            {
                visualPart.IndexInManager = index;
            }

            enemyCore.Initialize(visualPart);

            _enemyMovementController.UpdateEnemyValues(
                index,
                enemyCharacteristics.Speed,
                enemyCharacteristics.MotionPattern,
                enemyCharacteristics.MotionCharacteristic,
                enemyCharacteristics.IsolateDistance
                );
        }

        public async UniTaskVoid UpdateChallenge(EnemyVisualPart visualPart)
        {
            var indexInManager = visualPart.IndexInManager;

            var enemyInfo = await _challengeVisualPartFactory.UpdateVisualPart(
                index: visualPart.IndexInFactory,
                spawnDelay: DELAY_BEFORE_CHALLENGE_RESPAWN
                );

            ResetEnemyPosition(indexInManager);

            EnemySettings(
                indexInManager,
                _enemyCore[indexInManager],
                enemyInfo.EnemyVisualPart,
                enemyInfo.Enemy
                );
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ReleaseLoadedResources()
        {
            _enemyVisualPartFactory.ReleaseAllLoadedResources().Forget();
            _challengeVisualPartFactory.ReleaseAllLoadedResources().Forget();
        }

        public void Dispose()
        {
            _enemyMovementController.Dispose();

            ReleaseLoadedResources();
            DisposeEvents?.Invoke();
        }
    }
}
