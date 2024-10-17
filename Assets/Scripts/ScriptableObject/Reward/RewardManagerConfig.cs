using UnityEngine;

[CreateAssetMenu(fileName = "RewardManagerConfig", menuName = "Scriptable Objects/RewardManagerConfig")]
public class RewardManagerConfig : ScriptableObject
{
    [SerializeField] RewardControllerConfig _rewardControllerConfig;
    [SerializeField] int _points;

    public RewardControllerConfig RewardControllerConfig { get => _rewardControllerConfig; set => _rewardControllerConfig = value; }

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
