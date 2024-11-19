using UnityEngine;

[CreateAssetMenu(fileName = "PickUpItemManagerConfig", menuName = "Scriptable Objects/PickUpItemManagerConfig")]
public class PickUpItemManagerConfig : ScriptableObject
{
    [SerializeField] RewardControllerConfig _rewardControllerConfig;
    [SerializeField] RepairKitControllerConfig _repairKitControllerConfig;
    [SerializeField] int _points;

    public RewardControllerConfig RewardControllerConfig { get => _rewardControllerConfig; }
    public RepairKitControllerConfig RepairKitControllerConfig { get => _repairKitControllerConfig; }

    public int GetPoints() => _points;

    public void IncreasePoints(int increaseValue)
    {
        if (_points > int.MaxValue - increaseValue)
        {
            _points = int.MaxValue;
        }
        else
        {
            _points += increaseValue;
        }
    }

    public void DecreasePoints(int decreaseValue)
    {
        _points -= decreaseValue;

        if (_points < 0)
        {
            _points = 0;
        }
    }

}
