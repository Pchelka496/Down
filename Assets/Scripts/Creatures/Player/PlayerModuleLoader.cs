using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

public class PlayerModuleLoader : MonoBehaviour
{
    [SerializeField] PlayerModuleLoaderConfig _config;

    string _currentGroundMovementModuleAddress;
    string _currentFlightModuleAddress;

    AsyncOperationHandle<GameObject> _flightModuleHandle;
    AsyncOperationHandle<GameObject> _groundMovementModule;

    [Inject]
    private void Construct(LevelManager levelManager)
    {
        levelManager.SubscribeToRoundStart(RoundStart);
        LoadGroundMovementModule().Forget();
    }

    private void RoundStart(LevelManager levelManager)
    {
        LoadFlightModule().Forget();
        LoadSupportModules().Forget();

        levelManager.SubscribeToRoundEnd(RoundEnd);
    }

    private void RoundEnd(LevelManager levelManager, EnumRoundResults results)
    {
        LoadGroundMovementModule().Forget();

        levelManager.SubscribeToRoundStart(RoundStart);
    }

    private async UniTaskVoid LoadFlightModule()
    {
        if (_currentFlightModuleAddress == _config.FlightModuleAddressPrefab) return;

        if (_currentFlightModuleAddress != null)
        {
            ReleaseOldModule(_flightModuleHandle);
        }

        _currentFlightModuleAddress = _config.FlightModuleAddressPrefab;
        await GetModuleObject(_currentFlightModuleAddress, _flightModuleHandle);
    }

    private async UniTaskVoid LoadGroundMovementModule()
    {
        if (_currentGroundMovementModuleAddress == _config.GroundMovementModuleAddressPrefab) return;

        if (_currentGroundMovementModuleAddress != null)
        {
            ReleaseOldModule(_groundMovementModule);
        }

        _currentGroundMovementModuleAddress = _config.GroundMovementModuleAddressPrefab;
        await GetModuleObject(_currentGroundMovementModuleAddress, _groundMovementModule);
    }

    private async UniTaskVoid LoadSupportModules()
    {
        var supportModulesPrefabAddress = _config.SupportModules;

        foreach (var moduleAddress in supportModulesPrefabAddress)
        {
            GetModuleObject(moduleAddress).Forget();
        }

        await UniTask.CompletedTask;
    }

    private void ReleaseOldModule(AsyncOperationHandle<GameObject> handle)
    {
        Destroy(handle.Result);
        Addressables.Release(handle);
    }

    private async UniTask<GameObject> GetModuleObject(string address, AsyncOperationHandle<GameObject> handle = new())
    {
        handle = Addressables.LoadAssetAsync<GameObject>(address);

        await handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return GameplaySceneInstaller.DiContainer.InstantiatePrefab(handle.Result);
        }
        else
        {
            Debug.LogError("Error loading via Addressables.");
            return default;
        }
    }

}
