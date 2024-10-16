using Cysharp.Threading.Tasks;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine;

public class CheckpointPlatformController
{
   public const float PLATFORM_MOVE_DURATION = 15f;

    MapController _mapController;
    MapControllerConfig _config;
    CheckpointPlatform _currentPlatform;

    float _overshootHeightMultiplier = 1.005f;

    AnimationCurve movementCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public void Initialize(MapController mapController, MapControllerConfig config)
    {
        _mapController = mapController;
        _config = config;
        movementCurve = config.PlatformCurveMovementCurve;
    }

    public async UniTask MovePlatformToHeight(float targetHeight)
    {
        if (_currentPlatform == null)
        {
            Debug.LogWarning("No platform available to move.");
            return;
        }

        Vector3 startPos = _currentPlatform.transform.position;
        Vector3 overshootPos = new Vector3(startPos.x, targetHeight * _overshootHeightMultiplier, startPos.z);
        Vector3 finalPos = new Vector3(startPos.x, targetHeight, startPos.z);

        await MovePlatform(startPos, overshootPos, PLATFORM_MOVE_DURATION / 2);
        await MovePlatform(overshootPos, finalPos, PLATFORM_MOVE_DURATION / 2);
    }

    private async UniTask MovePlatform(Vector3 startPos, Vector3 endPos, float duration)
    {
        float timeElapsed = 0f;
        var transform = _currentPlatform.transform;

        while (timeElapsed < duration)
        {
            if (_currentPlatform == null) return;

            timeElapsed += Time.deltaTime;
            float t = Mathf.Clamp01(timeElapsed / duration);
            float curvedT = movementCurve.Evaluate(t);

            Vector3 newPos = Vector3.Lerp(startPos, endPos, curvedT);
            transform.position = newPos;

            await UniTask.Yield();
        }

        transform.position = endPos;
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
        checkpointPlatform.Initialize(initializer, this);
        _currentPlatform = checkpointPlatform;
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
