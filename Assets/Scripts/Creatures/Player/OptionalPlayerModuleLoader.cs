using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

public class OptionalPlayerModuleLoader
{
    const string CONFIG_ADDRESS = "ScriptableObject/ModulesConfig/PlayerModuleLoaderConfig";

    OptionalPlayerModuleLoaderConfig _config;
    CharacterController _player;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Удалите неиспользуемые закрытые члены", Justification = "<Ожидание>")]
    private void Construct(LevelManager levelManager, CharacterController player)
    {
        levelManager.SubscribeToRoundStart(RoundStart);
        _player = player;

        LoadConfiguration().Forget();
    }

    private void RoundStart(LevelManager levelManager)
    {
        if (_config == null)
        {
            Debug.LogError("Configuration is not loaded!");
            return;
        }

        LoadModules();
        levelManager.SubscribeToRoundEnd(RoundEnd);
    }

    private void RoundEnd(LevelManager levelManager, EnumRoundResults results)
    {
        levelManager.SubscribeToRoundStart(RoundStart);
    }

    private async UniTask LoadConfiguration()
    {
        var loadOperationData = await AddressableLouderHelper.LoadAssetAsync<OptionalPlayerModuleLoaderConfig>(CONFIG_ADDRESS);

        _config = loadOperationData.LoadAsset;

        LoadModules();
    }

    public async UniTask<BaseModule> LoadModuleForTest<T>()
    {
        if (_config == null)
        {
            Debug.LogError("Configuration is not loaded!");
            return null;
        }

        var modulesInfo = _config.ModuleInfoArray;

        foreach (var moduleInfo in modulesInfo)
        {
            if (moduleInfo.ModuleType == typeof(T))
            {
                if (moduleInfo.CreatedModule == null)
                {
                    return await ModuleLoad(moduleInfo);
                }
                else
                {
                    Debug.LogWarning($"Module created but not registered: {typeof(T).Name}");
                }
            }
        }

        Debug.LogWarning($"Module of this type is not found in the array: {typeof(T).Name}");
        return null;
    }

    private void LoadModules()
    {
        if (_config == null)
        {
            Debug.LogError("Configuration is not loaded!");
            return;
        }

        var modulesInfo = _config.ModuleInfoArray;

        foreach (var moduleInfo in modulesInfo)
        {
            if (moduleInfo.ActivityCheck && moduleInfo.CreatedModule == null)
            {
                ModuleLoad(moduleInfo).Forget();
            }
            else if (!moduleInfo.ActivityCheck && moduleInfo.CreatedModule != null)
            {
                ReleaseOldModule(moduleInfo.CreatedModuleHandler, moduleInfo.CreatedModule.gameObject);
            }
        }
    }

    private async UniTask<BaseModule> ModuleLoad(OptionalPlayerModuleLoaderConfig.ModuleInfo moduleInfo)
    {
        var newModule = await GetModuleObject(moduleInfo.ModulePrefabReference);

        moduleInfo.CreatedModule = newModule;
        _player.RegisterModule(newModule);

        return newModule;
    }

    private void ReleaseOldModule(AsyncOperationHandle<GameObject> handle, GameObject module)
    {
        MonoBehaviour.Destroy(module);

        if (handle.IsValid() && 
            handle.Status == AsyncOperationStatus.Succeeded)
        {
            Addressables.Release(handle);
        }
    }

    private async UniTask<BaseModule> GetModuleObject(AssetReference moduleReference)
    {
        var loadOperationData = await AddressableLouderHelper.LoadAssetAsync<GameObject>(moduleReference);

        return GameplaySceneInstaller.DiContainer.InstantiatePrefabForComponent<BaseModule>(loadOperationData.LoadAsset);
    }

}
