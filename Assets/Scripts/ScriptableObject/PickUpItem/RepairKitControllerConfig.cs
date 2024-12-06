using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ScriptableObject.PickUpItem
{
    [CreateAssetMenu(fileName = "RepairKitControllerConfig", menuName = "Scriptable Objects/RepairKitControllerConfig")]
    public class RepairKitControllerConfig : UnityEngine.ScriptableObject
    {
        [SerializeField] AssetReference _repairKitPrefabAddress;
        [SerializeField] int _maxRepairKitCount;

        public AssetReference RewardPrefabAddress => _repairKitPrefabAddress;
        public int MaxRepairKitCount => _maxRepairKitCount;
    }
}
