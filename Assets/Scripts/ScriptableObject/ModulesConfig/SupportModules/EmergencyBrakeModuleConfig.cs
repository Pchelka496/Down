using UnityEngine;

[CreateAssetMenu(fileName = "EmergencyBrakeModuleConfig", menuName = "Scriptable Objects/EmergencyBrakeModuleConfig")]
public class EmergencyBrakeModuleConfig : BaseModuleConfig
{
    [SerializeField] float[] _usageQuantity = new float[0];

    public override void SetLevel(int level)
    {
        if (!SetLevelCheck(level))
        {
            _currentLevel = _usageQuantity.Length - 1;
            Debug.LogError($"{this.GetType()} Current level {_currentLevel} is out of bounds for the array! Array length: {_usageQuantity.Length}");
        }

        _currentLevel = level;
    }

    public override int GetMaxLevel() => _usageQuantity.Length;

    public override bool SetLevelCheck(int level)
    {
        return !(level > _usageQuantity.Length);
    }

}
