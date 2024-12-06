using UnityEngine;

namespace ScriptableObject.PickUpItem
{
    [CreateAssetMenu(fileName = "PickUpItemManagerConfig", menuName = "Scriptable Objects/PickUpItemManagerConfig")]
    public class PickUpItemManagerConfig : UnityEngine.ScriptableObject
    {
        [SerializeField] RewardControllerConfig _rewardControllerConfig;
        [SerializeField] RepairKitControllerConfig _repairKitControllerConfig;

        public RewardControllerConfig RewardControllerConfig => _rewardControllerConfig;
        public RepairKitControllerConfig RepairKitControllerConfig => _repairKitControllerConfig;
    }
}
