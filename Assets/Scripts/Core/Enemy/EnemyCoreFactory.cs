using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

public class EnemyCoreFactory
{
    EnemyManager _enemyManager;
    EnemyManagerConfig _managerConfig;
    EnemyCore[] _enemyCore;

    public void Initialize(EnemyManager enemyManager, EnemyManagerConfig managerConfig, EnemyCore[] enemyCore)
    {
        _managerConfig = managerConfig;
        _enemyCore = enemyCore;
        _enemyManager = enemyManager;
    }

    public async UniTask<EnemyManager.EnemyControllerRoundStart> CreateEnemies()
    {
        var enemyCorePrefab = await LoadEnemyCorePrefabs(_managerConfig.EnemyCorePrefab);
        var enemyCount = _managerConfig.AllEnemyCount;

        var enemyTransforms = new Transform[enemyCount];

        var enemySpeeds = new float[enemyCount];
        var motionPatterns = new EnumMotionPattern[enemyCount];
        var motionCharacteristic = new float2[enemyCount];
        var isolationDistance = new Vector2[enemyCount];

        var installer = GameplaySceneInstaller.DiContainer;
        var enemyGameObjectPrefab = enemyCorePrefab.gameObject;

        var spawnPosition = new Vector2(EnemyManager.ENEMY_SPAWN_X_POSITION, EnemyManager.ENEMY_SPAWN_Y_POSITION);

        for (var i = 0; i < enemyCount; i++)
        {
            var enemyCore = installer.InstantiatePrefabForComponent<EnemyCore>(enemyGameObjectPrefab,
                                                                               spawnPosition,
                                                                               Quaternion.identity,
                                                                               _enemyManager.transform);

            if (enemyCore == null)
            {
                Debug.LogError("EnemyCore component missing in the instantiated prefab.");
                continue;
            }

            enemyTransforms[i] = enemyCore.transform;
            _enemyCore[i] = enemyCore;
        }

        return new EnemyManager.EnemyControllerRoundStart(enemyTransforms, enemySpeeds, motionPatterns, motionCharacteristic, isolationDistance);
    }

    private async UniTask<EnemyCore> LoadEnemyCorePrefabs(AssetReference enemyCoreReference)
    {
        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(enemyCoreReference);

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

}

