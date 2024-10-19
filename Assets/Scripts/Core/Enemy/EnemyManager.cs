using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;
using static EnemyManagerConfig;

public class EnemyManager : MonoBehaviour
{
    static readonly Vector3 _enemySpawnPosition = new Vector3(float.MaxValue / 2, float.MaxValue / 2, 0f);

    EnemyManagerConfig _config;
    EnemyCore[] _enemyCore;
    EnemyController _enemyController;
    CharacterController _player;

    AsyncOperationHandle<GameObject>[] _loadedHandles;

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
        _enemyController.Initialize(_config.EnemyCount);

        var enemyControllerRoundStart = await CreateEnemies();

        _enemyController.RoundStart(enemyControllerRoundStart.Transforms,
                                    enemyControllerRoundStart.Speeds,
                                    enemyControllerRoundStart.EnumMotionPatterns,
                                    enemyControllerRoundStart.MotionCharacteristic,
                                    enemyControllerRoundStart.IsolationDistance
                                    );
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
            var enemyCore = installer.InstantiatePrefabForComponent<EnemyCore>(enemyGameObjectPrefab, _enemySpawnPosition, Quaternion.identity, transform);

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

    private async UniTask EnemyRegionUpdater()
    {
        var enemyRegions = _config.EnemyRegions;
        EnemyRegion currentRegion = null;

        while (enemyRegions.Count > 0)
        {
            currentRegion = enemyRegions.Pop();

            await UniTask.WaitUntil(() => CharacterPositionMeter.YPosition <= currentRegion.StartHeight);

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

        await UniTask.CompletedTask;
    }


    private async void UpdateEnemyRegion(EnemyManagerConfig.EnemyRegion enemyRegion)
    {
        var enemies = enemyRegion.Enemies;

        int[] enemyCounts = CalculateEnemyCounts(enemies, _enemyCore.Length);

        var enemyVisualParts = await LoadAllEnemyVisualParts(enemies, enemyCounts);

        AssignEnemySettings(enemies, enemyCounts, enemyVisualParts);
    }

    private int[] CalculateEnemyCounts(Enemy[] enemies, int totalEnemies)
    {
        int[] enemyCounts = new int[enemies.Length];

        for (int i = 0; i < enemies.Length; i++)
        {
            enemyCounts[i] = Mathf.RoundToInt(totalEnemies * enemies[i].RelativeAmount);
        }

        int sum = 0;
        for (int i = 0; i < enemyCounts.Length; i++)
        {
            sum += enemyCounts[i];
        }
        if (sum != totalEnemies)
        {
            enemyCounts[enemyCounts.Length - 1] += totalEnemies - sum;
        }

        return enemyCounts;
    }

    private async UniTask<GameObject[]> LoadAllEnemyVisualParts(Enemy[] enemies, int[] enemyCounts)
    {
        var enemyVisualPartAddresses = new List<string>();

        for (int i = 0; i < enemies.Length; i++)
        {
            for (int j = 0; j < enemyCounts[i]; j++)
            {
                enemyVisualPartAddresses.Add(enemies[i].EnemyAddress);
            }
        }

        return await LoadEnemyVisualPart(enemyVisualPartAddresses.ToArray());
    }

    private void AssignEnemySettings(Enemy[] enemies, int[] enemyCounts, GameObject[] enemyVisualParts)
    {
        int visualPartIndex = 0;

        for (int i = 0; i < enemies.Length; i++)
        {
            for (int j = 0; j < enemyCounts[i]; j++)
            {
                var enemyCharacteristics = enemies[i];

                EnemySettings(visualPartIndex,
                              _enemyCore[visualPartIndex],
                              enemyVisualParts[visualPartIndex],
                              enemyCharacteristics.Speed,
                              enemyCharacteristics.MotionPattern,
                              enemyCharacteristics.MotionCharacteristic,
                               enemyCharacteristics.IsolationDistance
                              );

                visualPartIndex++;
            }
        }
    }

    private void EnemySettings(int index, EnemyCore enemy, GameObject visualPart, float speed, EnumMotionPattern motionPattern, float2 motionCharacteristic, Vector2 isolationDistance)
    {
#if UNITY_EDITOR
        if (!EditorApplication.isPlaying)
        {
            DestroyImmediate(visualPart);
            return;
        }
#endif

        enemy.Initialize(visualPart);
        _enemyController.UpdateEnemyValues(index, speed, motionPattern, motionCharacteristic, isolationDistance);
    }

    private async UniTask<GameObject[]> LoadEnemyVisualPart(string[] addresses)
    {
        await ReleaseLoadedResources();

        var installer = GameplaySceneInstaller.DiContainer;
        var loadedObjects = new GameObject[addresses.Length];

        _loadedHandles = new AsyncOperationHandle<GameObject>[addresses.Length];

        for (int i = 0; i < addresses.Length; i++)
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(addresses[i]);
            await handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                loadedObjects[i] = installer.InstantiatePrefab(handle.Result);
                _loadedHandles[i] = handle;
            }
            else
            {
                Debug.LogError($"Failed to load asset at {addresses[i]}");
            }
        }

        return loadedObjects;
    }

    private UniTask ReleaseLoadedResources()
    {
        if (_loadedHandles == null) return UniTask.CompletedTask;

        foreach (var handle in _loadedHandles)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }

        _loadedHandles = null;

        return UniTask.CompletedTask;
    }

    public void RoundStart(LevelManager levelManager)
    {
        EnemyRegionUpdater().Forget();
    }

    private void OnDestroy()
    {
        _enemyController.Dispose();
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
