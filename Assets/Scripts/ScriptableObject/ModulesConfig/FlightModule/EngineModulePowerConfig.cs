using UnityEngine;

[CreateAssetMenu(fileName = "EngineModuleConfig", menuName = "Scriptable Objects/EngineModuleConfig")]
public class EngineModulePowerConfig : BaseModuleConfig
{
    [SerializeField] float[] _engineMaxPower = new float[0];

    public override bool ActivityCheck() => true;

    public override void SetLevel(int level)
    {
        if (!SetLevelCheck(level))
        {
            _currentLevel = _engineMaxPower.Length - 1;
            Debug.LogError($"{this.GetType()} Current level {_currentLevel} is out of bounds for the array! Array length: {_engineMaxPower.Length}");
        }

        _currentLevel = level;
    }

    public override int GetMaxLevel() => _engineMaxPower.Length - 1;

    public float GetMaxPower() => _engineMaxPower[_currentLevel];

    public override bool SetLevelCheck(int level)
    {
        return !(level > _engineMaxPower.Length - 1);
    }

}
