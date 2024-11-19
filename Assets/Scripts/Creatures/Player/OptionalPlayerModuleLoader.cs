using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

public class OptionalPlayerModuleLoader : MonoBehaviour
{
    [SerializeField] OptionalPlayerModuleLoaderConfig _config;
    CharacterController _player;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Удалите неиспользуемые закрытые члены", Justification = "<Ожидание>")]
    private void Construct(LevelManager levelManager, CharacterController player)
    {
        levelManager.SubscribeToRoundStart(RoundStart);
        _player = player;

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

    public async UniTask<BaseModule> LoadModuleForTest<T>()
    {
        var modulesInfo = _config.ModuleInfoArray;

        foreach (var moduleInfo in modulesInfo)
        {
            if (moduleInfo.ModuleType == typeof(T))
            {
                if (moduleInfo.CreatedModule == null)
                {
                    return await ModuleLoud(moduleInfo);
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
        var modulesInfo = _config.ModuleInfoArray;

        foreach (var moduleInfo in modulesInfo)
        {
            if (moduleInfo.ActivityCheck && moduleInfo.CreatedModule == null)
            {
                ModuleLoud(moduleInfo).Forget();
            }
            else if (!moduleInfo.ActivityCheck && moduleInfo.CreatedModule != null)
            {
                ReleaseOldModule(moduleInfo.CreatedModuleHandler, moduleInfo.CreatedModule.gameObject);
            }
        }
    }

    private async UniTask<BaseModule> ModuleLoud(OptionalPlayerModuleLoaderConfig.ModuleInfo moduleInfo)
    {
        var newModule = await GetModuleObject(moduleInfo.ModulePrefabReference);

        moduleInfo.CreatedModule = newModule;
        _player.RegisterModule(newModule);

        return newModule;
    }

    private void ReleaseOldModule(AsyncOperationHandle<GameObject> handle, GameObject module)
    {
        Destroy(module);

        if (handle.IsValid())
        {
            Addressables.Release(handle);
        }
    }

    private async UniTask<BaseModule> GetModuleObject(AssetReference moduleReference)
    {
        var handle = Addressables.LoadAssetAsync<GameObject>(moduleReference);

        await handle;

        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            return GameplaySceneInstaller.DiContainer.InstantiatePrefabForComponent<BaseModule>(handle.Result);
        }
        else
        {
            Debug.LogError("Error loading via Addressable.");
            return default;
        }
    }

}
