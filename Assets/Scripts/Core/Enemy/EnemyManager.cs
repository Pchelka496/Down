using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;
using static EnemyManagerConfig;

public class EnemyManager : MonoBehaviour
{
    public const int ENEMY_LAYER_INDEX = 8;
    public const float ENEMY_SPAWN_X_POSITION = LevelManager.PLAYER_START_X_POSITION + 123456789f;
    public const float ENEMY_SPAWN_Y_POSITION = LevelManager.PLAYER_START_Y_POSITION * 2f;

    EnemyManagerConfig _config;
    EnemyCore[] _enemyCore;
    EnemyController _enemyController;
    CharacterController _player;

    AsyncOperationHandle<GameObject>[] _loadedHandles;
    EnemyVisualPart[] _enemyVisualParts;
    CancellationTokenSource _enemyRegionUpdaterCts;

    [Inject]
    private void Construct(CharacterController player, LevelManager levelManager)
    {
        _player = player;
        levelManager.SubscribeToRoundStart(RoundStart);
    }

    public async void Initialize(EnemyManagerConfig config)
    {
        _config = config;
        _enemyController = GameplaySceneInstaller.DiContainer.Instantiate<EnemyController>();

        _enemyVisualParts = new EnemyVisualPart[_config.EnemyCount];

        var enemyControllerRoundStart = await CreateEnemies();
        _enemyController.Initialize(_config.EnemyCount,
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

        levelManager.SubscribeToRoundEnd(RoundEnd);
    }

    public void RoundEnd(LevelManager levelManager, EnumRoundResults results)
    {
        _enemyRegionUpdaterCts?.Cancel();

        ReleaseLoadedResources().Forget();

        levelManager.SubscribeToRoundStart(RoundStart);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ResetEnemyPosition()
    {
        Vector2 spawnPosition = new Vector2(ENEMY_SPAWN_X_POSITION, ENEMY_SPAWN_Y_POSITION);

        foreach (var enemyCore in _enemyCore)
        {
            enemyCore.transform.position = spawnPosition;
        }
    }

    private async UniTask<EnemyControllerRoundStart> CreateEnemies()
    {
        var enemyCorePrefab = await LoadEnemyCorePrefabs(_config.EnemyCorePrefab);
        var enemyCount = _config.EnemyCount;

        _enemyCore = new EnemyCore[enemyCount];

        var enemyTransforms = new Transform[enemyCount];

        var enemySpeeds = new float[enemyCount];
        var motionPatterns = new EnumMotionPattern[enemyCount];
        var motionCharacteristic = new float2[enemyCount];
        var isolationDistance = new Vector2[enemyCount];

        var installer = GameplaySceneInstaller.DiContainer;
        var enemyGameObjectPrefab = enemyCorePrefab.gameObject;

        for (var i = 0; i < enemyCount; i++)
        {
            var enemyCore = installer.InstantiatePrefabForComponent<EnemyCore>(enemyGameObjectPrefab, new(ENEMY_SPAWN_X_POSITION, ENEMY_SPAWN_Y_POSITION), Quaternion.identity, transform);

            if (enemyCore == null)
            {
                Debug.LogError("EnemyCore component missing in the instantiated prefab.");
                continue;
            }

            enemyTransforms[i] = enemyCore.transform;
            _enemyCore[i] = enemyCore;
        }

        return new EnemyControllerRoundStart(enemyTransforms, enemySpeeds, motionPatterns, motionCharacteristic, isolationDistance);
    }

    private async UniTask<EnemyCore> LoadEnemyCorePrefabs(string address)
    {
        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(address);

        await handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            if (handle.Result.TryGetComponent<EnemyCore>(out var enemyCore))
            {
                return enemyCore;
            }
            else
            {
                Debug.LogError("Error get component EnemyCore.");
                return default;
            }
        }
        else
        {
            Debug.LogError("Error loading via Addressables.");
            return default;
        }
    }

    private async UniTask EnemyRegionUpdater(CancellationToken token)
    {
        var enemyRegions = _config.EnemyRegions;
        EnemyRegion currentRegion = null;

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

    private async void UpdateEnemyRegion(EnemyManagerConfig.EnemyRegion enemyRegion)
    {
        var enemies = CalculateEnemyVisualPartIndex(enemyRegion.Enemies, _enemyCore.Length);

        var enemyVisualParts = await LoadAllEnemyVisualParts(enemies);

        AssignEnemySettings(enemies, enemyVisualParts);
    }

    private Enemy[] CalculateEnemyVisualPartIndex(Enemy[] enemies, int totalEnemies)
    {
        List<Enemy> calculatedEnemies = new List<Enemy>();
        int currentCount = 0;

        for (int i = 0; i < enemies.Length; i++)
        {
            int count = Mathf.RoundToInt(totalEnemies * enemies[i].RelativeAmount);
            currentCount += count;

            for (int j = 0; j < count; j++)
            {
                calculatedEnemies.Add(enemies[i]);
            }
        }

        if (currentCount < totalEnemies)
        {
            int remainingCount = totalEnemies - currentCount;
            Enemy emptyEnemy = Enemy.EmptyEnemy();

            for (int i = 0; i < remainingCount; i++)
            {
                calculatedEnemies.Add(emptyEnemy);
            }
        }

        return calculatedEnemies.Take(totalEnemies).ToArray();
    }

    private async UniTask<EnemyVisualPart[]> LoadAllEnemyVisualParts(Enemy[] enemies)
    {
        var enemyVisualPartAddresses = new string[enemies.Length];

        for (int i = 0; i < enemies.Length; i++)
        {
            enemyVisualPartAddresses[i] = enemies[i].EnemyAddress;
        }

        return await LoadEnemyVisualPart(enemyVisualPartAddresses);
    }

    private void AssignEnemySettings(Enemy[] enemies, EnemyVisualPart[] enemyVisualParts)
    {
        for (int i = 0; i < enemies.Length; i++)
        {
            var enemyCharacteristics = enemies[i];

            EnemySettings(i,
                          _enemyCore[i],
                          enemyVisualParts[i],
                          enemyCharacteristics.Speed,
                          enemyCharacteristics.MotionPattern,
                          enemyCharacteristics.MotionCharacteristic
                         );
        }

        ResetEnemyPosition();
    }

    private void EnemySettings(int index, EnemyCore enemy, EnemyVisualPart visualPart, float speed, EnumMotionPattern motionPattern, float2 motionCharacteristic)
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            DestroyImmediate(visualPart);
            return;
        }
#endif

        enemy.Initialize(visualPart);
        _enemyController.UpdateEnemyValues(index, speed, motionPattern, motionCharacteristic);
    }

    private async UniTask<EnemyVisualPart[]> LoadEnemyVisualPart(string[] addresses)
    {
        await ReleaseLoadedResources();

        var installer = GameplaySceneInstaller.DiContainer;
        var loadedObjects = new EnemyVisualPart[addresses.Length];

        _loadedHandles = new AsyncOperationHandle<GameObject>[addresses.Length];

        for (int i = 0; i < addresses.Length; i++)
        {
            if (addresses[i] == null)
            {
                continue;
            }

            var handle = Addressables.LoadAssetAsync<GameObject>(addresses[i]);
            await handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var enemyVisualPart = installer.InstantiatePrefabForComponent<EnemyVisualPart>(handle.Result);
                loadedObjects[i] = enemyVisualPart;
                _loadedHandles[i] = handle;
                _enemyVisualParts[i] = enemyVisualPart;
            }
            else
            {
                Debug.LogError($"Failed to load asset at {addresses[i]}");
            }
        }

        return loadedObjects;
    }

    private async UniTask ReleaseLoadedResources()
    {
        if (_loadedHandles == null) return;

        foreach (var enemy in _enemyVisualParts)
        {
            Destroy(enemy.gameObject);
        }

        await UniTask.DelayFrame(1);

        foreach (var handle in _loadedHandles)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }

        _loadedHandles = null;
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

    private void OnDestroy()
    {
        _enemyController.Dispose();
        ClearToken(ref _enemyRegionUpdaterCts);
        ReleaseLoadedResources().Forget();
    }

    private readonly struct EnemyControllerRoundStart
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
