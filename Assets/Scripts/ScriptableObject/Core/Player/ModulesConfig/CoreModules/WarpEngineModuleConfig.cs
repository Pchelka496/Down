using ScriptableObject.ModulesConfig;
using System;
using Types.record;
using UnityEngine;

[CreateAssetMenu(fileName = "WarpEngineConfig", menuName = "Scriptable Objects/WarpEngineConfig")]
public class WarpEngineModuleConfig : BaseModuleConfig
{
    [SerializeField] float _minMoveDuration = 5f;
    [SerializeField] float _maxMoveDuration = 10f;

    [SerializeField] float _minHeight;
    [SerializeField] float _maxHeight;

    [SerializeField] int _maxNumberOfChargingLevels;
    int _currentNumberOfChargingLevels;

    public float MinHeight { get => _minHeight; }
    public float MaxHeight { get => _maxHeight; }
    public int MaxNumberOfChargingLevels { get => _maxNumberOfChargingLevels; }
    public int CurrentNumberOfChargingLevels
    {
        get => _currentNumberOfChargingLevels;
        set
        {
            _saveAction?.Invoke(this);
            _currentNumberOfChargingLevels = Mathf.Clamp(value, 0, _maxNumberOfChargingLevels);
        }
    }

    public float MaxMoveDuration => _maxMoveDuration;
    public float MinMoveDuration => _minMoveDuration;

    public override bool ActivityCheck() => true;
    public override Type GetModuleType() => typeof(WarpEngineModule);

    public override void LoadSaveData(SaveData saveData)
    {
        if (saveData == null)
        {
            Debug.LogError($"saveData is null. {GetType()}");
            return;
        }
        if (saveData.WarpEngineSaveData == null)
        {
            Debug.Log($"saveData.WarpEngineSaveData is null. {GetType()}");
            return;
        }

        CurrentNumberOfChargingLevels = saveData.WarpEngineSaveData.CurrentNumberOfChargingLevels;
    }

    public override void SaveToSaveData(SaveData saveData)
    {
        if (saveData == null)
        {
            Debug.LogError($"saveData is null. {GetType()}");
            return;
        }
        if (saveData.WarpEngineSaveData == null)
        {
            Debug.Log($"saveData.WarpEngineSaveData is null. {GetType()}");
            return;
        }

        saveData.WarpEngineSaveData.CurrentNumberOfChargingLevels = _currentNumberOfChargingLevels;
    }

    [System.Serializable]
    public record WarpEngineSaveData
    {
        [field: SerializeField] public int CurrentNumberOfChargingLevels { get; set; }
    }
}
