using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

public class WorldBorderUpdater
{
    MapController _controller;
    MapControllerConfig _config;
    LevelManager _levelManager;

    [Inject]
    private void Construct(LevelManager levelManager)
    {
        _levelManager = levelManager;
    }

    public void Initialize(MapController mapController, MapControllerConfig config)
    {
        _controller = mapController;
        _config = config;
    }

    public async UniTask UpdateWorldBorder(MapControllerConfig.Level currentLevel)
    {
        var worldBorderPrefab = await LoadPrefabs(_config.PrefabWorldBorderAddress);
        var worldBorderSize = _config.WorldBorderSize;

        var levelWidth = currentLevel.LevelWidth;
        var levelHeight = currentLevel.LevelHeight;

        var currentPlatformHeight = currentLevel.CurrentPlatformHeight;
        var targetPlatformHeight = currentLevel.TargetPlatformHeight;

        var totalHeight = Mathf.Abs(currentPlatformHeight - targetPlatformHeight);

        var leftWallStartPosition = new Vector3(-levelWidth / 2, targetPlatformHeight, 0);
        var rightWallStartPosition = new Vector3(levelWidth / 2, targetPlatformHeight, 0);

        var brickHeight = worldBorderSize.y;

        int brickCount = Mathf.CeilToInt(totalHeight / brickHeight);

        for (int i = 0; i < brickCount; i++)
        {
            Vector3 brickPosition = leftWallStartPosition + new Vector3(0, i * brickHeight, 0);
            var leftWallBrick = MonoBehaviour.Instantiate(worldBorderPrefab, brickPosition, Quaternion.identity);
        }

        for (int i = 0; i < brickCount; i++)
        {
            Vector3 brickPosition = rightWallStartPosition + new Vector3(0, i * brickHeight, 0);
            var rightWallBrick = MonoBehaviour.Instantiate(worldBorderPrefab, brickPosition, Quaternion.identity);
        }
    }

    public async UniTask<GameObject> LoadPrefabs(string address)
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
