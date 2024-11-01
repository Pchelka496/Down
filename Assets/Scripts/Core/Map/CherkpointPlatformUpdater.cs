using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine;

public class CheckpointPlatformController
{
    MapController _mapController;
    MapControllerConfig _config;
    CheckpointPlatform _currentPlatform;

    AsyncOperationHandle<GameObject> _currentPlatformHandle;

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

    public void ClearPlatform()
    {
        MonoBehaviour.Destroy(_currentPlatform.gameObject);
        Addressables.Release(_currentPlatformHandle);
    }

    public async UniTask<CheckpointPlatform> CreatePlatform(AssetReference platformReference, float height)
    {
        var checkPoint = await LoadPrefabs(platformReference);

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
        checkpointPlatform.Initialize(initializer, this);
        _currentPlatform = checkpointPlatform;
    }

    private async UniTask<GameObject> LoadPrefabs(AssetReference platformReference)
    {
        _currentPlatformHandle = Addressables.LoadAssetAsync<GameObject>(platformReference);

        await _currentPlatformHandle;

        if (_currentPlatformHandle.Status == AsyncOperationStatus.Succeeded)
        {
            return _currentPlatformHandle.Result;
        }
        else
        {
            Debug.LogError("Error loading via Addressables.");
            return default;
        }
    }

}
