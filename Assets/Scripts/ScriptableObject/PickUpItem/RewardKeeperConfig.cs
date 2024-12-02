using System;
using UnityEngine;

[CreateAssetMenu(fileName = "RewardKeeperConfig", menuName = "Scriptable Objects/RewardKeeperConfig")]
public class RewardKeeperConfig : ScriptableObject
{
    [SerializeField] int _points;
    event Action<int> OnPointChanged;

    public void LoadSaveData(SaveData saveData)
    {
        _points = saveData.Points;
    }

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
        if (increaseValue != 0)
        {
            OnPointChanged?.Invoke(_points);
        }
    }

    public void DecreasePoints(int decreaseValue)
    {
        _points -= decreaseValue;

        if (_points < 0)
        {
            _points = 0;
        }
        if (decreaseValue != 0)
        {
            OnPointChanged?.Invoke(GetPoints());
        }
    }

    public void SubscribeToOnPointChangedEvent(Action<int> action) => OnPointChanged += action;
    public void UnsubscribeToOnPointChangedEvent(Action<int> action) => OnPointChanged -= action;

}
