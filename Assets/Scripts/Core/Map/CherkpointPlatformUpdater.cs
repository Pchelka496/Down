using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine;

public class CheckpointPlatformUpdater
{
    MapController _mapController;
    MapControllerConfig _config;

    string _currentPlatformAddress;
    CheckpointPlatform _currentPlatform;
    string _targetPlatformAddress;
    CheckpointPlatform _targetPlatform;

    public void Initialize(MapController mapController, MapControllerConfig config)
    {
        _mapController = mapController;
        _config = config;
    }

    public async UniTask SwitchToNextLevel(MapControllerConfig.Level currentLevel)
    {
        _ = ResetTargetPlatform(currentLevel);

        await UniTask.CompletedTask;
    }

    public async UniTask CreatePlatforms(MapControllerConfig.Level currentLevel)
    {
        _ = CreateCurrentPlatform(currentLevel);
        _ = CreateTargetPlatform(currentLevel);

        await UniTask.CompletedTask;
    }

    private async UniTask CreateCurrentPlatform(MapControllerConfig.Level currentLevel)
    {
        if (_currentPlatformAddress == _config.PrefabCurrentCheckpointPlatformAddress) return;

        _currentPlatformAddress = _config.PrefabCurrentCheckpointPlatformAddress;
        var checkPoint = await LoadPrefabs(_currentPlatformAddress);

        if (checkPoint.TryGetComponent<CheckpointPlatform>(out var checkpointPlatform))
        {
            _currentPlatform = GameplaySceneInstaller.DiContainer.InstantiatePrefabForComponent<CheckpointPlatform>(checkpointPlatform);
            var initializer = new CheckpointPlatform.Initializer(currentLevel.CurrentPlatformHeight, currentLevel.CurrentPlatformWidth, currentLevel.CurrentPlatformDoorsPositions, false);

            InitializePlatform(_currentPlatform, initializer);
        }

    }

    private async UniTask CreateTargetPlatform(MapControllerConfig.Level currentLevel)
    {
        if (_targetPlatformAddress == _config.PrefabTargetCheckpointPlatformAddress) return;

        _targetPlatformAddress = _config.PrefabTargetCheckpointPlatformAddress;
        var initializer = new CheckpointPlatform.Initializer(currentLevel.TargetPlatformHeight, currentLevel.TargetPlatformWidth, currentLevel.TargetPlatformDoorsPositions, true);

        _targetPlatform = await CreatePlatform(_targetPlatformAddress, initializer);
    }

    private async UniTask ResetTargetPlatform(MapControllerConfig.Level currentLevel)
    {
        if (_currentPlatformAddress != _config.PrefabTargetCheckpointPlatformAddress)
        {
            _ = CreateTargetPlatform(currentLevel);
            return;
        }
        (_currentPlatform, _targetPlatform) = (_targetPlatform, _currentPlatform);

        var initializer = new CheckpointPlatform.Initializer(currentLevel.TargetPlatformHeight, currentLevel.TargetPlatformWidth, currentLevel.TargetPlatformDoorsPositions, true);
        InitializePlatform(_targetPlatform, initializer);

        await UniTask.CompletedTask;
    }

    public async UniTask<CheckpointPlatform> CreatePlatform(string prefabAddress, CheckpointPlatform.Initializer initializer)
    {
        var checkPoint = await LoadPrefabs(prefabAddress);

        if (checkPoint.TryGetComponent<CheckpointPlatform>(out var checkpointPlatform))
        {
            var platform = GameplaySceneInstaller.DiContainer
                .InstantiatePrefabForComponent<CheckpointPlatform>(checkpointPlatform);
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
