using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "RepairKitControllerConfig", menuName = "Scriptable Objects/RepairKitControllerConfig")]
public class RepairKitControllerConfig : ScriptableObject
{
    [SerializeField] AssetReference _repairKitPrefabAddress;
    [SerializeField] int _maxRepairKitCount;

    public AssetReference RewardPrefabAddress { get => _repairKitPrefabAddress; }
    public int MaxRepairKitCount { get => _maxRepairKitCount; }

}
