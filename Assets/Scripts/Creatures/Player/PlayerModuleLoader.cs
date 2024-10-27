using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

public class PlayerModuleLoader : MonoBehaviour
{
    [SerializeField] PlayerModuleLoaderConfig _config;

    [Inject]
    private void Construct(LevelManager levelManager)
    {
        levelManager.SubscribeToRoundStart(RoundStart);

        LoadModules();
    }

    private void RoundStart(LevelManager levelManager)
    {
        LoadModules();

        levelManager.SubscribeToRoundEnd(RoundEnd);
    }

    private void RoundEnd(LevelManager levelManager, EnumRoundResults results)
    {
        levelManager.SubscribeToRoundStart(RoundStart);
    }

    private void LoadModules()
    {
        var modulesInfo = _config.ModuleInfoArray;

        foreach (var moduleInfo in modulesInfo)
        {
            if (moduleInfo.ActivityCheck && moduleInfo.CreatedModule == null)
            {
                ModuleLoud(moduleInfo);
            }
            else if (!moduleInfo.ActivityCheck && moduleInfo.CreatedModule != null)
            {
                ReleaseOldModule(moduleInfo.CreatedModuleHandler);
            }
        }
    }

    private async void ModuleLoud(PlayerModuleLoaderConfig.ModuleInfo moduleInfo)
    {
        moduleInfo.CreatedModule = await GetModuleObject(moduleInfo.ModulePrefabAddress);
    }

    private void ReleaseOldModule(AsyncOperationHandle<GameObject> handle)
    {
        Destroy(handle.Result);
        Addressables.Release(handle);
    }

    private async UniTask<BaseModule> GetModuleObject(string address, AsyncOperationHandle<GameObject> handle = new())
    {
        handle = Addressables.LoadAssetAsync<GameObject>(address);

        await handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return GameplaySceneInstaller.DiContainer.InstantiatePrefabForComponent<BaseModule>(handle.Result);
        }
        else
        {
            Debug.LogError("Error loading via Addressables.");
            return default;
        }
    }

}
