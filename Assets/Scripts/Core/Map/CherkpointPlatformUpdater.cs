using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine;

public class CheckpointPlatformUpdater
{
    MapController _mapController;
    MapControllerConfig _config;

    CheckpointPlatform _currentPlatform;

    public void Initialize(MapController mapController, MapControllerConfig config)
    {
        _mapController = mapController;
        _config = config;
    }

    public async UniTask CreatePlatforms(float height)
    {
        CreatePlatform(_config.PrefabCheckpointPlatformAddress, height).Forget();
        await UniTask.CompletedTask;
    }

    public async UniTask<CheckpointPlatform> CreatePlatform(string prefabAddress, float height)
    {
        var checkPoint = await LoadPrefabs(prefabAddress);

        if (checkPoint.TryGetComponent<CheckpointPlatform>(out var checkpointPlatform))
        {
            var platform = GameplaySceneInstaller.DiContainer
                          .InstantiatePrefabForComponent<CheckpointPlatform>(checkpointPlatform);

            var initializer = new CheckpointPlatform.Initializer(height);
            InitializePlatform(platform, initializer);

            return platform;
        }

        return null;
    }

    private void InitializePlatform(CheckpointPlatform checkpointPlatform, CheckpointPlatform.Initializer initializer)
    {
        checkpointPlatform.Initialize(initializer);
    }

    private async UniTask<GameObject> LoadPrefabs(string address)
    {
        AsyncOperationHandle<GameObject> handle = Addressables.LoadAssetAsync<GameObject>(address);

        await handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return handle.Result;
        }
        else
        {
            Debug.LogError("Error loading via Addressables.");
            return default;
        }
    }

}
