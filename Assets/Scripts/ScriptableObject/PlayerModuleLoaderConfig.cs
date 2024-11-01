using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

[CreateAssetMenu(fileName = "PlayerModuleLoaderConfig", menuName = "Scriptable Objects/PlayerModuleLoaderConfig")]
public class PlayerModuleLoaderConfig : ScriptableObject
{
    [SerializeField] ModuleInfo[] _moduleInfoArray;

    public ModuleInfo[] ModuleInfoArray { get => _moduleInfoArray; set => _moduleInfoArray = value; }

    [System.Serializable]
    public record ModuleInfo
    {
        [SerializeField] AssetReference _modulePrefabAddress;
        [SerializeField] BaseModuleConfig _activityCheck;
        BaseModule _createdModule;
        AsyncOperationHandle<GameObject> _createdModuleHandler;

        public AssetReference ModulePrefabReference { get => _modulePrefabAddress; set => _modulePrefabAddress = value; }
        public BaseModuleConfig ActivityCheck { get => _activityCheck; set => _activityCheck = value; }

        public BaseModule CreatedModule { get => _createdModule; set => _createdModule = value; }
        public AsyncOperationHandle<GameObject> CreatedModuleHandler { get => _createdModuleHandler; set => _createdModuleHandler = value; }

    }

}
