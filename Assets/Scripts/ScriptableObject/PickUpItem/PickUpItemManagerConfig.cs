using UnityEngine;

[CreateAssetMenu(fileName = "PickUpItemManagerConfig", menuName = "Scriptable Objects/PickUpItemManagerConfig")]
public class PickUpItemManagerConfig : ScriptableObject
{
    [SerializeField] RewardControllerConfig _rewardControllerConfig;
    [SerializeField] RepairKitControllerConfig _repairKitControllerConfig;

    public RewardControllerConfig RewardControllerConfig { get => _rewardControllerConfig; }
    public RepairKitControllerConfig RepairKitControllerConfig { get => _repairKitControllerConfig; }

}
