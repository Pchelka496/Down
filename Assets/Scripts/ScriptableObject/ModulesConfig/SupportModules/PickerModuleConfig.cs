using UnityEngine;

[CreateAssetMenu(fileName = "PickerModuleConfig", menuName = "Scriptable Objects/PickerModuleConfig")]
public class PickerModuleConfig : BaseModuleConfig
{
    [Tooltip("collider radius")]
    [SerializeField] float[] _pickUpRadius = new float[0];

    public override bool ActivityCheck() => true;

    public override void SetLevel(int level)
    {
        if (!SetLevelCheck(level))
        {
            _currentLevel = _pickUpRadius.Length - 1;
            Debug.LogError($"{this.GetType()} Current level {_currentLevel} is out of bounds for the array _pickUpRadius! Array length: {_pickUpRadius.Length}");
        }

        _currentLevel = level;
    }

    public override int GetMaxLevel() => _pickUpRadius.Length - 1;

    public float GetPickUpRadius() => _pickUpRadius[_currentLevel];

    public override bool SetLevelCheck(int level)
    {
        return !(level > _pickUpRadius.Length - 1);
    }

}
