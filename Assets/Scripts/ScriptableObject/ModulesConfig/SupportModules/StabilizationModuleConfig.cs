using UnityEngine;

[CreateAssetMenu(fileName = "StabilizationModuleConfig", menuName = "Scriptable Objects/StabilizationModuleConfig")]
public class StabilizationModuleConfig : BaseModuleConfig
{
    [SerializeField] float[] _rotationSpeedOnLevel = new float[0];

    public override void SetLevel(int level)
    {
        if (!SetLevelCheck(level))
        {
            _currentLevel = _rotationSpeedOnLevel.Length - 1;
            Debug.LogError($"{this.GetType()} Current level {_currentLevel} is out of bounds for the array! Array length: {_rotationSpeedOnLevel.Length}");
        }

        _currentLevel = level;
    }

    public override int GetMaxLevel() => _rotationSpeedOnLevel.Length - 1;

    public float GetRotationSpeed() => _rotationSpeedOnLevel[_currentLevel];

    public override bool SetLevelCheck(int level)
    {
        return !(level > _rotationSpeedOnLevel.Length - 1);
    }

}
