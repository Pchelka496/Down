using UnityEngine;

[CreateAssetMenu(fileName = "AirBrakeConfig", menuName = "Scriptable Objects/AirBrakeConfig")]
public class AirBrakeModuleConfig : BaseModuleConfig
{
    [SerializeField] float[] _brakePower = new float[0];

    public override void SetLevel(int level)
    {
        if (!SetLevelCheck(level))
        {
            _currentLevel = _brakePower.Length - 1;
            Debug.LogError($"{this.GetType()} Current level {_currentLevel} is out of bounds for the max health array! Array length: {_brakePower.Length}");
        }

        _currentLevel = level;
    }

    public override int GetMaxLevel() => _brakePower.Length;
    public float GetBrakePower() => _brakePower[_currentLevel];

    public override bool SetLevelCheck(int level)
    {
        return !(level > _brakePower.Length);
    }

}
