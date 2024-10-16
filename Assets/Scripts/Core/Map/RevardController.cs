using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class RewardController
{
    MapController _mapController;
    RewardControllerConfig _config;
    AsyncOperationHandle<GameObject>[] _loadedHandles;

    public void Initialize(MapController mapController, RewardControllerConfig config)
    {
        _mapController = mapController;
        _config = config;
    }

    public void RoundStart()
    {
        
    }

    public void RoundEnd()
    { 
    
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

}
