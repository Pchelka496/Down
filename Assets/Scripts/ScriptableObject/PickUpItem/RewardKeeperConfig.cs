using UnityEngine;

[CreateAssetMenu(fileName = "RewardKeeperConfig", menuName = "Scriptable Objects/RewardKeeperConfig")]
public class RewardKeeperConfig : ScriptableObject
{
    [SerializeField] int _points;

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
