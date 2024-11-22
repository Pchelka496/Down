using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using static EnemyConfig;
using UnityEngine;
using System.Linq;
using Zenject;

public class EnemyVisualPartController
{
    EnemyVisualPart[] _allEnemyVisualParts;
    Enemy[] _enemies;

    int _visualPartCurrentIndex;
    int _enemyCount;
    Vector2 _spawnPosition;
    AsyncOperationHandle<GameObject>[] _allVisualPartsLoadedHandles;

    public int EnemyCount => _enemyCount;

    public void Initialize(int enemyCount)
    {
        _enemyCount = enemyCount;
        _allEnemyVisualParts = new EnemyVisualPart[enemyCount];
        _spawnPosition = new(EnemyManager.ENEMY_SPAWN_X_POSITION, EnemyManager.ENEMY_SPAWN_Y_POSITION);

        _allVisualPartsLoadedHandles = new AsyncOperationHandle<GameObject>[enemyCount];
    }

    public Enemy[] UpdateEnemyRegion(Enemy[] enemies)
    {
        var trueEnemies = CalculateTrueEnemyVisualPart(enemies);
        _enemies = trueEnemies;

        return trueEnemies;
    }

    public void SetEnemies(Enemy[] enemies) => _enemies = enemies;

    private Enemy[] CalculateTrueEnemyVisualPart(Enemy[] enemies)
    {
        List<Enemy> calculatedEnemies = new();
        int currentCount = 0;

        currentCount = AddRequiredEnemies(enemies, _enemyCount, calculatedEnemies, currentCount);

        if (currentCount < _enemyCount)
        {
            currentCount = AddRelativeEnemies(enemies, _enemyCount, currentCount, calculatedEnemies);
        }

        FillWithEmptyEnemies(_enemyCount, currentCount, calculatedEnemies);

        return calculatedEnemies.Take(_enemyCount).ToArray();
    }

    private int AddRequiredEnemies(Enemy[] enemies, int totalEnemies, List<Enemy> calculatedEnemies, int currentCount)
    {
        foreach (var enemy in enemies)
        {
            int requiredCount = Mathf.Min(enemy.RequiredAmount, totalEnemies - currentCount);

            for (int i = 0; i < requiredCount; i++)
            {
                calculatedEnemies.Add(enemy);
            }

            currentCount += requiredCount;

            if (currentCount >= totalEnemies)
                break;
        }

        return currentCount;
    }

    private int AddRelativeEnemies(Enemy[] enemies, int totalEnemies, int currentCount, List<Enemy> calculatedEnemies)
    {
        int remainingEnemies = totalEnemies - currentCount;

        foreach (var enemy in enemies)
        {
            int relativeCount = Mathf.RoundToInt(remainingEnemies * enemy.RelativeAmount);

            if (currentCount + relativeCount > totalEnemies)
                relativeCount = totalEnemies - currentCount;

            for (int i = 0; i < relativeCount; i++)
            {
                calculatedEnemies.Add(enemy);
            }

            currentCount += relativeCount;

            if (currentCount >= totalEnemies)
                break;
        }

        return currentCount;
    }

    private void FillWithEmptyEnemies(int totalEnemies, int currentCount, List<Enemy> calculatedEnemies)
    {
        while (currentCount < totalEnemies)
        {
            calculatedEnemies.Add(Enemy.EmptyEnemy());
            currentCount++;
        }
    }

    public async UniTask<EnemyVisualPart[]> LoadAllEnemyVisualParts()
    {
        var regularEnemies = _enemies.ToArray();
        var regularVisualParts = await LoadEnemyVisualPart(regularEnemies.Select(e => e.EnemyAddress).ToArray());

        return regularVisualParts;
    }

    private async UniTask<EnemyVisualPart[]> LoadEnemyVisualPart(AssetReference[] enemyReference)
    {
        await ReleaseAllLoadedResources();

        var installer = GameplaySceneInstaller.DiContainer;
        var loadedObjects = new EnemyVisualPart[_enemyCount];

        for (int i = 0; i < _enemyCount; i++)
        {
            if (enemyReference[i] == null)
            {
                continue;
            }

            loadedObjects[i] = await LoadVisualPart(enemyReference[i], installer, i);
            _visualPartCurrentIndex++;
        }

        return loadedObjects;
    }

    private async UniTask<EnemyVisualPart> LoadVisualPart(AssetReference enemyReference, DiContainer installer, int index)
    {
        var loadOperationData = await AddressableLouderHelper.LoadAssetAsync<GameObject>(enemyReference);

        var enemyVisualPart = installer.InstantiatePrefabForComponent<EnemyVisualPart>(loadOperationData.Handle.Result,
                                                                                       _spawnPosition,
                                                                                       Quaternion.identity,
                                                                                       null
                                                                                       );

        _allVisualPartsLoadedHandles[index] = loadOperationData.Handle;
        _allEnemyVisualParts[index] = enemyVisualPart;
        enemyVisualPart.IndexInFactory = index;

        return enemyVisualPart;
    }

    public async UniTask<EnemyInfo> UpdateVisualPart(int index, float spawnDelay = 0)
    {
        var enemy = GetVisualPartAddress();

        await ReleaseLoadedElement(_allVisualPartsLoadedHandles[index], _allEnemyVisualParts[index]);

        await UniTask.WaitForSeconds(spawnDelay);

        var visualPart = await LoadVisualPart(enemy.EnemyAddress, GameplaySceneInstaller.DiContainer, index);

        return new(enemy, visualPart);
    }

    private Enemy GetVisualPartAddress()
    {
        if (_visualPartCurrentIndex >= _enemies.Length)
        {
            _visualPartCurrentIndex = 0;
        }

        var enemy = _enemies[_visualPartCurrentIndex];

        _visualPartCurrentIndex++;

        return enemy;
    }

    public async UniTask ReleaseAllLoadedResources()
    {
        await ReleaseLoadedResources(_allVisualPartsLoadedHandles, _allEnemyVisualParts);
    }

    private async UniTask ReleaseLoadedResources(AsyncOperationHandle<GameObject>[] handles, EnemyVisualPart[] visualParts)
    {
        if (handles == null) return;

        var releaseTasks = new List<UniTask>();

        for (int i = 0; i < handles.Length; i++)
        {
            releaseTasks.Add(ReleaseLoadedElement(handles[i], visualParts[i]));
        }

        await UniTask.WhenAll(releaseTasks);
    }

    private async UniTask ReleaseLoadedElement(AsyncOperationHandle<GameObject> handle, EnemyVisualPart visualParts)
    {
        if (visualParts != null && visualParts.gameObject != null)
        {
            MonoBehaviour.Destroy(visualParts.gameObject);

            await UniTask.DelayFrame(1);
        }

        if (handle.IsValid() &&
            handle.Status == AsyncOperationStatus.Succeeded)
        {
            Addressables.Release(handle);
        }

        await UniTask.CompletedTask;
    }

    public readonly struct EnemyInfo
    {
        public readonly Enemy Enemy;
        public readonly EnemyVisualPart EnemyVisualPart;

        public EnemyInfo(Enemy enemy, EnemyVisualPart enemyVisualPart)
        {
            Enemy = enemy;
            EnemyVisualPart = enemyVisualPart;
        }
    }

}