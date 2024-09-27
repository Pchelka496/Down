using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

public class MapController : MonoBehaviour
{
    LevelManager _levelManager;
    MapControllerConfig _config;

    string _currentPlatformAddress;
    CheckpointPlatform _currentPlatform;
    string _targetPlatformAddress;
    CheckpointPlatform _targetPlatform;

    [Inject]
    private void Construct(LevelManager levelManager)
    {
        _levelManager = levelManager;
    }

    public void Initialize(MapControllerConfig config)
    {
        _config = config;
    }

    private async void UpdateCurrentPlatform()
    {
        if (_currentPlatformAddress == _config.PrefabCurrentCheckpointPlatformAddress) return;

        _currentPlatformAddress = _config.PrefabCurrentCheckpointPlatformAddress;
        var checkPoint = await LoadPrefabs(_currentPlatformAddress);

        if (checkPoint.TryGetComponent<CheckpointPlatform>(out var checkpointPlatform))
        {
            _currentPlatform = Instantiate(checkpointPlatform);
            InitializePlatform(_currentPlatform, CalculateCurrentPlatformHeight());

        }

    }

    private float CalculateCurrentPlatformHeight()
    {

        return 1f;
    }

    private async void UpdateTargetPlatform()
    {
        if (_targetPlatformAddress == _config.PrefabTargetCheckpointPlatformAddress) return;

        _targetPlatformAddress = _config.PrefabTargetCheckpointPlatformAddress;
        var checkPoint = await LoadPrefabs(_targetPlatformAddress);

        if (checkPoint.TryGetComponent<CheckpointPlatform>(out var checkpointPlatform))
        {
            _targetPlatform = Instantiate(checkpointPlatform);
            InitializePlatform(_targetPlatform, CalculateTargetPlatformHeight());
        }

    }

    private float CalculateTargetPlatformHeight()
    {

        return 1f;
    }

    private void InitializePlatform(CheckpointPlatform checkpointPlatform, float height)
    {
        checkpointPlatform.Initialize(height);
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
