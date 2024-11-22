using Cysharp.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Zenject;
using static EnemyConfig;

public class EnemyManager : System.IDisposable
{
    public const int ENEMY_LAYER_INDEX = 8;
    public const float ENEMY_SPAWN_X_POSITION = LevelManager.PLAYER_START_X_POSITION + 99000f;
    public const float ENEMY_SPAWN_Y_POSITION = LevelManager.PLAYER_START_Y_POSITION + 1000f;
    public const float DELAY_BEFORE_START_CHALLENGE_SPAWNING = 3f;
    public const float DELAY_BEFORE_CHALLENGE_RESPAWN = 2f;

    readonly Transform _enemyParent;
    EnemyManagerConfig _config;
    EnemyCoreFactory _enemyCoreFactory;
    EnemyVisualPartController _enemyVisualPartFactory;
    EnemyVisualPartController _challengeVisualPartFactory;
    EnemyMovementController _enemyController;
    EnemyCore[] _enemyCore;

    CancellationTokenSource _enemyRegionUpdaterCts;

    public EnemyManager(Transform enemyParent)
    {
        _enemyParent = enemyParent;
    }

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Удалите неиспользуемые закрытые члены", Justification = "<Ожидание>")]
    private void Construct(LevelManager levelManager)
    {
        levelManager.SubscribeToRoundStart(RoundStart);
    }

    public async void Initialize(EnemyManagerConfig config)
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

        _enemyController = diContainer.Instantiate<EnemyMovementController>();

        var enemyControllerRoundStart = await _enemyCoreFactory.CreateEnemies();
        _enemyController.Initialize(_config.AllEnemyCount,
                                    enemyControllerRoundStart.Transforms,
                                    enemyControllerRoundStart.Speeds,
                                    enemyControllerRoundStart.EnumMotionPatterns,
                                    enemyControllerRoundStart.MotionCharacteristic,
                                    enemyControllerRoundStart.IsolationDistance);

        _enemyController.StartEnemyMoving();
    }

    public void RoundStart(LevelManager levelManager)
    {
        ClearToken(ref _enemyRegionUpdaterCts);
        _enemyRegionUpdaterCts = new CancellationTokenSource();

        EnemyRegionUpdater(_enemyRegionUpdaterCts.Token).Forget();
        CreateChallenge();

        levelManager.SubscribeToRoundEnd(RoundEnd);
    }

    private async void CreateChallenge()
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

    public void RoundEnd(LevelManager levelManager, EnumRoundResults results)
    {
        _enemyRegionUpdaterCts?.Cancel();

        ReleaseLoadedResources();

        levelManager.SubscribeToRoundStart(RoundStart);
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
        _enemyCore[index].transform.position = new Vector2(ENEMY_SPAWN_X_POSITION, ENEMY_SPAWN_Y_POSITION);
    }

    private async UniTask EnemyRegionUpdater(CancellationToken token)
    {
        var enemyRegions = _config.EnemyRegions;
        EnemyRegionConfig currentRegion = null;

        while (enemyRegions.Count > 0 && !token.IsCancellationRequested)
        {
            currentRegion = enemyRegions.Pop();

            await UniTask.WaitUntil(() => CharacterPositionMeter.YPosition <= currentRegion.StartHeight, cancellationToken: token);

            if (enemyRegions.Count > 0)
            {
                var nextRegion = enemyRegions.Peek();

                if (CharacterPositionMeter.YPosition < nextRegion.StartHeight)
                {
                    continue;
                }
            }

            UpdateEnemyRegion(currentRegion);

            currentRegion = null;
        }
    }

    private async void UpdateEnemyRegion(EnemyRegionConfig enemyRegion)
    {
        var enemies = _enemyVisualPartFactory.UpdateEnemyRegion(enemyRegion.Enemies);

        var enemyVisualParts = await _enemyVisualPartFactory.LoadAllEnemyVisualParts();

        AssignEnemySettings(enemies, enemyVisualParts);
    }

    private void AssignEnemySettings(Enemy[] enemies, EnemyVisualPart[] enemyVisualParts)
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            var enemyCharacteristics = enemies[i];

            EnemySettings(i,
                          _enemyCore[i],
                          enemyVisualParts[i],
                          enemyCharacteristics
                         );
        }

        ResetAllEnemyPosition();
    }

    private void EnemySettings(int index, EnemyCore enemyCore, EnemyVisualPart visualPart, Enemy enemyCharacteristics)
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            MonoBehaviour.DestroyImmediate(visualPart);
            return;
        }
#endif
        if (visualPart != null)
        {
            visualPart.IndexInManager = index;
        }

        enemyCore.Initialize(visualPart);

        if (visualPart != null)
        {
            visualPart.Initialize(enemyCore);
        }

        _enemyController.UpdateEnemyValues(index, enemyCharacteristics.Speed, enemyCharacteristics.MotionPattern, enemyCharacteristics.MotionCharacteristic, enemyCharacteristics.IsolateDistance);
    }

    public async void UpdateChallenge(EnemyVisualPart visualPart)
    {
        var indexInManager = visualPart.IndexInManager;

        var enemyInfo = await _challengeVisualPartFactory.UpdateVisualPart(visualPart.IndexInFactory, DELAY_BEFORE_CHALLENGE_RESPAWN);

        ResetEnemyPosition(indexInManager);

        EnemySettings(indexInManager,
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

    private void ClearToken(ref CancellationTokenSource cts)
    {
        if (cts == null) return;

        if (!cts.IsCancellationRequested)
        {
            cts.Cancel();
        }

        cts.Dispose();
        cts = null;
    }

    public void Dispose()
    {
        _enemyController.Dispose();

        ClearToken(ref _enemyRegionUpdaterCts);
        ReleaseLoadedResources();
    }

    public readonly struct EnemyControllerRoundStart
    {
        public readonly Transform[] Transforms;
        public readonly float[] Speeds;
        public readonly EnumMotionPattern[] EnumMotionPatterns;
        public readonly float2[] MotionCharacteristic;
        public readonly Vector2[] IsolationDistance;

        public EnemyControllerRoundStart(Transform[] transforms, float[] speeds, EnumMotionPattern[] enumMotionPatterns, float2[] motionCharacteristic, Vector2[] isolationDistance) : this()
        {
            Transforms = transforms;
            Speeds = speeds;
            EnumMotionPatterns = enumMotionPatterns;
            MotionCharacteristic = motionCharacteristic;
            IsolationDistance = isolationDistance;
        }

    }

}
