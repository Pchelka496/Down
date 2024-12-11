using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine;
using Core.Installers;
using ScriptableObject.Map;

public class CheckpointPlatformController
{
    MapControllerConfig _config;
    CheckpointPlatform _currentPlatform;

    AsyncOperationHandle<GameObject> _currentPlatformHandle;

    public void Initialize(MapControllerConfig config)
    {
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

        if (_currentPlatformHandle.IsValid() &&
            _currentPlatformHandle.Status == AsyncOperationStatus.Succeeded)
        {
            Addressables.Release(_currentPlatformHandle);
            _currentPlatformHandle = default;
        }
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
        checkpointPlatform.Initialize(initializer);
        _currentPlatform = checkpointPlatform;
    }

    private async UniTask<GameObject> LoadPrefabs(AssetReference platformReference)
    {
        var loadOperationData = await AddressableLouderHelper.LoadAssetAsync<GameObject>(platformReference);

        _currentPlatformHandle = loadOperationData.Handle;

        return loadOperationData.LoadAsset;
    }
}
