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
    EnemyManager _enemyManager;
    EnemyVisualPart[] _allEnemyVisualParts;
    Enemy[] _enemies;

    int _visualPartCurrentIndex;
    int _enemyCount;
    Vector2 _spawnPosition;
    AsyncOperationHandle<GameObject>[] _allVisualPartsLoadedHandles;

    public int EnemyCount => _enemyCount;

    public void Initialize(EnemyManager enemyManager, int enemyCount)
    {
        _enemyCount = enemyCount;
        _allEnemyVisualParts = new EnemyVisualPart[enemyCount];
        _enemyManager = enemyManager;
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
        List<Enemy> calculatedEnemies = new List<Enemy>();
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
        var handle = Addressables.LoadAssetAsync<GameObject>(enemyReference);
        await handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            var enemyVisualPart = installer.InstantiatePrefabForComponent<EnemyVisualPart>(handle.Result, _spawnPosition, Quaternion.identity, null);

            _allVisualPartsLoadedHandles[index] = handle;
            _allEnemyVisualParts[index] = enemyVisualPart;
            enemyVisualPart.IndexInFactory = index;

            return enemyVisualPart;
        }
        else
        {
            Debug.LogError($"Failed to load asset at {enemyReference}");
            return default;
        }
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

        if (handle.IsValid())
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

//public class EnemyVisualPartController
//{
//    EnemyManager _enemyManager;
//    EnemyManagerConfig _managerConfig;
//    EnemyVisualPart[] _allEnemyVisualParts;
//    EnemyVisualPart[] _enemyChallengeVisualParts;
//    int[] _challengeIndexes;
//    int _challengeIndex = 0;

//    AsyncOperationHandle<GameObject>[] _allVisualPartsLoadedHandles;

//    public void Initialize(EnemyManager enemyManager, EnemyManagerConfig managerConfig, EnemyVisualPart[] enemyVisualParts)
//    {
//        _managerConfig = managerConfig;
//        _allEnemyVisualParts = enemyVisualParts;
//        _enemyManager = enemyManager;

//        _allVisualPartsLoadedHandles = new AsyncOperationHandle<GameObject>[managerConfig.AllEnemyCount];
//    }

//    public Enemy[] CalculateEnemyVisualPart(Enemy[] enemies, int totalEnemies)
//    {
//        List<Enemy> calculatedEnemies = new List<Enemy>();
//        int currentCount = 0;

//        currentCount = AddChallengeEnemies(totalEnemies, calculatedEnemies);

//        if (currentCount < totalEnemies)
//        {
//            currentCount = AddRequiredEnemies(enemies, totalEnemies, calculatedEnemies, currentCount);
//        }

//        if (currentCount < totalEnemies)
//        {
//            currentCount = AddRelativeEnemies(enemies, totalEnemies, currentCount, calculatedEnemies);
//        }

//        FillWithEmptyEnemies(totalEnemies, currentCount, calculatedEnemies);

//        return calculatedEnemies.Take(totalEnemies).ToArray();
//    }

//    private int AddChallengeEnemies(int totalEnemies, List<Enemy> calculatedEnemies)
//    {
//        int count = 0;
//        int challengeCount = _managerConfig.ChallengeCount;
//        var challengePrefabs = _managerConfig.ChallengePrefabs;

//        int maxChallengeEnemies = Math.Min(challengeCount, totalEnemies - calculatedEnemies.Count);

//        for (int i = 0; i < maxChallengeEnemies; i++)
//        {
//            var enemyPrefab = challengePrefabs[_challengeIndex];
//            calculatedEnemies.Add(enemyPrefab.GetEnemy);

//            _challengeIndex = (_challengeIndex + 1) % challengePrefabs.Length;
//            count++;
//        }

//        return count;
//    }

//    private int AddRequiredEnemies(Enemy[] enemies, int totalEnemies, List<Enemy> calculatedEnemies, int currentCount)
//    {
//        foreach (var enemy in enemies)
//        {
//            int requiredCount = Mathf.Min(enemy.RequiredAmount, totalEnemies - currentCount);

//            for (int i = 0; i < requiredCount; i++)
//            {
//                calculatedEnemies.Add(enemy);
//            }

//            currentCount += requiredCount;

//            if (currentCount >= totalEnemies)
//                break;
//        }

//        return currentCount;
//    }

//    private int AddRelativeEnemies(Enemy[] enemies, int totalEnemies, int currentCount, List<Enemy> calculatedEnemies)
//    {
//        int remainingEnemies = totalEnemies - currentCount;

//        foreach (var enemy in enemies)
//        {
//            int relativeCount = Mathf.RoundToInt(remainingEnemies * enemy.RelativeAmount);

//            if (currentCount + relativeCount > totalEnemies)
//                relativeCount = totalEnemies - currentCount;

//            for (int i = 0; i < relativeCount; i++)
//            {
//                calculatedEnemies.Add(enemy);
//            }

//            currentCount += relativeCount;

//            if (currentCount >= totalEnemies)
//                break;
//        }

//        return currentCount;
//    }

//    private void FillWithEmptyEnemies(int totalEnemies, int currentCount, List<Enemy> calculatedEnemies)
//    {
//        while (currentCount < totalEnemies)
//        {
//            calculatedEnemies.Add(Enemy.EmptyEnemy());
//            currentCount++;
//        }
//    }

//    public async UniTask<EnemyVisualPart[]> LoadAllEnemyVisualParts(Enemy[] enemies)
//    {
//        var challengeEnemies = _managerConfig.ChallengePrefabs;
//        var regularEnemies = enemies.ToArray();

//        _enemyChallengeVisualParts = await LoadChallengeVisualParts(challengeEnemies);

//        var regularVisualParts = await LoadEnemyVisualPart(regularEnemies.Select(e => e.EnemyAddress).ToArray());

//        return _enemyChallengeVisualParts.Concat(regularVisualParts).ToArray();
//    }

//    private async UniTask<EnemyVisualPart[]> LoadChallengeVisualParts(EnemyConfig[] challengeEnemies)
//    {
//        //await ReleaseChallengeLoadedResources();

//        var installer = GameplaySceneInstaller.DiContainer;
//        var challengeVisualParts = new List<EnemyVisualPart>();
//        var challengeCount = Mathf.Min(challengeEnemies.Length, _managerConfig.ChallengeCount);

//        for (int i = 0; i < challengeCount; i++)
//        {
//            challengeVisualParts.Add(await LoadVisualPart(GetChallengeAddress(), installer, i));
//        }

//        _enemyChallengeVisualParts = challengeVisualParts.ToArray();

//        return _enemyChallengeVisualParts;
//    }

//    private string GetChallengeAddress()
//    {
//        var challengeEnemies = _managerConfig.ChallengePrefabs;

//        if (_challengeIndex >= challengeEnemies.Length)
//        {
//            _challengeIndex = 0;
//        }

//        var enemyConfig = challengeEnemies[_challengeIndex];
//        _challengeIndex++;

//        return enemyConfig.GetEnemy.EnemyAddress;
//    }

//    private async UniTask<EnemyVisualPart[]> LoadEnemyVisualPart(string[] addresses)
//    {
//        await ReleaseAllLoadedResources();

//        var installer = GameplaySceneInstaller.DiContainer;
//        var loadedObjects = new EnemyVisualPart[addresses.Length];

//        for (int i = 0; i < addresses.Length; i++)
//        {
//            if (addresses[i] == null)
//            {
//                continue;
//            }

//            loadedObjects[i] = await LoadVisualPart(addresses[i], installer, i);
//        }

//        return loadedObjects;
//    }

//    private async UniTask<EnemyVisualPart> LoadVisualPart(string address, DiContainer installer, int index)
//    {
//        var handle = Addressables.LoadAssetAsync<GameObject>(address);
//        await handle;

//        if (handle.Status == AsyncOperationStatus.Succeeded)
//        {
//            var enemyVisualPart = installer.InstantiatePrefabForComponent<EnemyVisualPart>(handle.Result);
//            _allVisualPartsLoadedHandles[index] = handle;
//            _allEnemyVisualParts[index] = enemyVisualPart;

//            return enemyVisualPart;
//        }
//        else
//        {
//            Debug.LogError($"Failed to load asset at {address}");
//            return default;
//        }
//    }

//    //public async UniTask<EnemyVisualPart[]> UpdateChallenge()
//    //{
//    //    return await LoadChallengeVisualParts(_managerConfig.ChallengePrefabs);
//    //}

//    //private async UniTask<EnemyVisualPart> UpdateChallenge(int index)
//    //{
//    //    return await LoadVisualPart(GetChallengeAddress(), GameplaySceneInstaller.DiContainer, index);
//    //}

//    //private async UniTask ReleaseChallengeLoadedResources()
//    //{
//    //    await ReleaseLoadedResources(_challengeVisualPartsLoadedHandles, _enemyChallengeVisualParts);
//    //}

//    public async UniTask ReleaseAllLoadedResources()
//    {
//        await ReleaseLoadedResources(_allVisualPartsLoadedHandles, _allEnemyVisualParts);
//    }

//    private async UniTask ReleaseLoadedResources(AsyncOperationHandle<GameObject>[] handles, EnemyVisualPart[] visualParts)
//    {
//        if (handles == null) return;

//        foreach (var enemy in visualParts)
//        {
//            if (enemy != null && enemy.gameObject != null)
//            {
//                MonoBehaviour.Destroy(enemy.gameObject);
//            }
//        }

//        await UniTask.DelayFrame(1);

//        foreach (var handle in handles)
//        {
//            if (handle.IsValid())
//            {
//                Addressables.Release(handle);
//            }
//        }

//        await UniTask.CompletedTask;
//    }

//}
