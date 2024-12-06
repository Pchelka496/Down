using Creatures.Player;
using ScriptableObject.ModulesConfig;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ScriptableObject
{
    [CreateAssetMenu(fileName = "OptionalPlayerModuleLoaderConfig", menuName = "Scriptable Objects/OptionalPlayerModuleLoaderConfig")]
    public class OptionalPlayerModuleLoaderConfig : UnityEngine.ScriptableObject
    {
        [SerializeField] ModuleInfo[] _moduleInfoArray;

        public ModuleInfo[] ModuleInfoArray { get => _moduleInfoArray; set => _moduleInfoArray = value; }

        [System.Serializable]
        public record ModuleInfo
        {
            [SerializeField] AssetReference _modulePrefabAddress;
            [SerializeField] BaseModuleConfig _moduleConfig;
            BaseModule _createdModule;
            AsyncOperationHandle<GameObject> _createdModuleHandler;

            public AssetReference ModulePrefabReference { get => _modulePrefabAddress; set => _modulePrefabAddress = value; }
            public bool ActivityCheck => _moduleConfig.ActivityCheck();
            public System.Type ModuleType => _moduleConfig.GetModuleType();

            public BaseModule CreatedModule { get => _createdModule; set => _createdModule = value; }
            public AsyncOperationHandle<GameObject> CreatedModuleHandler { get => _createdModuleHandler; set => _createdModuleHandler = value; }

        }

    }
}
