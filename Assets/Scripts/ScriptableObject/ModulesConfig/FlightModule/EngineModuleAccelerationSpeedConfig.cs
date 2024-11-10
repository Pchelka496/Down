using UnityEngine;

[CreateAssetMenu(fileName = "EngineModuleAccelerationSpeedConfig", menuName = "Scriptable Objects/EngineModuleAccelerationSpeedConfig")]
public class EngineModuleAccelerationSpeedConfig : BaseModuleConfig
{
    [SerializeField] float[] _engineAccelerationSpeed = new float[0];

    public override bool ActivityCheck()=> true;

    //public override void SetLevel(int level)
    //{
    //    if (!SetLevelCheck(level))
    //    {
    //        _currentLevel = _engineAccelerationSpeed.Length - 1;
    //        Debug.LogError($"{this.GetType()} Current level {_currentLevel} is out of bounds for the array! Array length: {_engineAccelerationSpeed.Length}");
    //    }

    //    _currentLevel = level;
    //}

    //public override int GetMaxLevel() => _engineAccelerationSpeed.Length - 1;

    //public float GetAccelerationSpeed() => _engineAccelerationSpeed[_currentLevel];

    //public override bool SetLevelCheck(int level)
    //{
    //    return !(level > _engineAccelerationSpeed.Length - 1);
    //}

}
