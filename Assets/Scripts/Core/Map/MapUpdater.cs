using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

public class MapUpdater
{
    MapController _controller;
    MapControllerConfig _config;
    LevelManager _levelManager;

    string _mapAddress;
    GameObject _map;

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

    public async UniTask UpdateMap(MapControllerConfig.Level currentLevel)
    {
        if (_map != null)
        {
            MonoBehaviour.Destroy(_map);
        }

        _mapAddress = currentLevel.MapPrefabAddress;
        var mapPrefab = await LoadPrefabs(currentLevel.MapPrefabAddress);
        _map = GameplaySceneInstaller.DiContainer.InstantiatePrefab(mapPrefab);

        _map.transform.position = currentLevel.MapGlobalPosition;
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
