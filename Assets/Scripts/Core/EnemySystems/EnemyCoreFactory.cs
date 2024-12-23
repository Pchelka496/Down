using Core.Enemy;
using Core.Installers;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using ScriptableObject.Enemy;
using Unity.Mathematics;
using UnityEngine;

public class EnemyCoreFactory
{
    Transform _enemyParent;
    EnemyManagerConfig _managerConfig;
    EnemyCore[] _enemyCore;

    public void Initialize(EnemyManagerConfig managerConfig, EnemyCore[] enemyCore, Transform enemyParent)
    {
        _managerConfig = managerConfig;
        _enemyCore = enemyCore;
        _enemyParent = enemyParent;
    }

    public async UniTask<EnemyMovementController.Initializer> CreateEnemies()
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

        var spawnPosition = new Vector2(EnemySystemCoordinator.ENEMY_SPAWN_X_POSITION, EnemySystemCoordinator.ENEMY_SPAWN_Y_POSITION);

        for (var i = 0; i < enemyCount; i++)
        {
            var enemyCore = installer.InstantiatePrefabForComponent<EnemyCore>(enemyGameObjectPrefab,
                                                                               spawnPosition,
                                                                               Quaternion.identity,
                                                                               _enemyParent
                                                                               );

            if (enemyCore == null)
            {
                Debug.LogError("EnemyCore component missing in the instantiated prefab.");
                continue;
            }

            enemyTransforms[i] = enemyCore.transform;
            _enemyCore[i] = enemyCore;
        }

        return new(enemyTransforms, enemySpeeds, motionPatterns, motionCharacteristic, isolationDistance);
    }

    private async UniTask<EnemyCore> LoadEnemyCorePrefabs(AssetReference enemyCoreReference)
    {
        var loadOperationData = await AddressableLouderHelper.LoadAssetAsync<GameObject>(enemyCoreReference);

        if (loadOperationData.Handle.Result.TryGetComponent<EnemyCore>(out var enemyCore))
        {
            return enemyCore;
        }
        else
        {
            Debug.LogError("Error get component EnemyCore.");
            return default;
        }
    }

}

