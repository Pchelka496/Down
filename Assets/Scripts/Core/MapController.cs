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

    CheckpointPlatformController _checkpointPlatform;
    MapUpdater _mapUpdater;

    public float FirstHeight => _config.FirstPlatformHeight();
    public float FullMapHeight { get => _config.MaximumHeight; }

    [Inject]
    private void Construct(LevelManager levelManager)
    {
        _levelManager = levelManager;

        _checkpointPlatform = GameplaySceneInstaller.DiContainer.Instantiate<CheckpointPlatformController>();
        _mapUpdater = GameplaySceneInstaller.DiContainer.Instantiate<MapUpdater>();
    }

    public void Initialize(MapControllerConfig config)
    {
        _config = config;

        _checkpointPlatform.Initialize(this, config);
        _mapUpdater.Initialize(this, config);

        var level = _config.GetSavingHeight(_levelManager.PlayerSavedHeight);

        _ = _checkpointPlatform.CreatePlatforms(level);
    }

    public PlatformInformation GetClosestPlatformAboveHeight(float height) => _config.GetClosestPlatformAboveHeight(height);
    public PlatformInformation GetClosestPlatformBelowHeight(float height) => _config.GetClosestPlatformBelowHeight(height);

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
