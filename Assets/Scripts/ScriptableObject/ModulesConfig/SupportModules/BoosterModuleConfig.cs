using UnityEngine;

[CreateAssetMenu(fileName = "BoosterModuleConfig", menuName = "Scriptable Objects/BoosterModuleConfig")]
public class BoosterModuleConfig : BaseModuleConfig
{
    [SerializeField] float[] _boosterPower = new float[0];

    public override void SetLevel(int level)
    {
        if (!SetLevelCheck(level))
        {
            _currentLevel = _boosterPower.Length - 1;
            Debug.LogError($"{this.GetType()} Current level {_currentLevel} is out of bounds for the _boosterPower array! Array length: {_boosterPower.Length}");
        }

        _currentLevel = level;
    }

    public override int GetMaxLevel() => _boosterPower.Length - 1;
    public float GetBoosterPower() => _boosterPower[_currentLevel];

    public override bool SetLevelCheck(int level)
    {
        return !(level > _boosterPower.Length - 1);
    }

}
