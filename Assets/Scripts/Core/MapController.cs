using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;
using static MapControllerConfig;

public class MapController : MonoBehaviour
{
    LevelManager _levelManager;
    MapControllerConfig _config;

    WorldBorderUpdater _worldBorderUpdater;
    CheckpointPlatformUpdater _checkpointPlatform;
    MapUpdater _mapUpdater;

    public float FirstPlatformHeight
    {
        get
        {
            return _config.GetFirstPlatformHeight();
        }
    }

    [Inject]
    private void Construct(LevelManager levelManager)
    {
        _levelManager = levelManager;

        _worldBorderUpdater = GameplaySceneInstaller.DiContainer.Instantiate<WorldBorderUpdater>();
        _checkpointPlatform = GameplaySceneInstaller.DiContainer.Instantiate<CheckpointPlatformUpdater>();
        _mapUpdater = GameplaySceneInstaller.DiContainer.Instantiate<MapUpdater>();
    }

    public void Initialize(MapControllerConfig config)
    {
        _config = config;

        _worldBorderUpdater.Initialize(this, config);
        _checkpointPlatform.Initialize(this, config);
        _mapUpdater.Initialize(this, config);

        var level = _config.GetLevel(_levelManager.PlayerSavedHeight);

        _ = _checkpointPlatform.CreatePlatforms(level);

        _ = _worldBorderUpdater.UpdateWorldBorder(level);
        _ = _mapUpdater.UpdateMap(level);
    }

    public void SwitchToNextLevel()
    {
        var level = _config.GetLevel(_levelManager.PlayerSavedHeight);

        _ = _checkpointPlatform.SwitchToNextLevel(level);
        _ = _worldBorderUpdater.UpdateWorldBorder(level);
        _ = _mapUpdater.UpdateMap(level);
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
