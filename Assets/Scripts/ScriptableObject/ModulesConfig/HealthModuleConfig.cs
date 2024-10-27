using UnityEngine;

[CreateAssetMenu(fileName = "HealthModuleConfig", menuName = "Scriptable Objects/HealthModuleConfig")]
public class HealthModuleConfig : BaseModuleConfig
{
    [SerializeField] int[] _maxHealthOnLevel = new int[1];

    public override void SetLevel(int level)
    {
        if (!SetLevelCheck(level))
        {
            _currentLevel = _maxHealthOnLevel.Length - 1;
            Debug.LogError($"{this.GetType()} Current level {_currentLevel} is out of bounds for the array! Array length: {_maxHealthOnLevel.Length}");
        }

        _currentLevel = level;
    }

    public override int GetMaxLevel() => _maxHealthOnLevel.Length - 1;

    public override bool SetLevelCheck(int level)
    {
        return !(level > _maxHealthOnLevel.Length - 1);
    }

    public int GetMaxHealth() => _maxHealthOnLevel[_currentLevel];

}

