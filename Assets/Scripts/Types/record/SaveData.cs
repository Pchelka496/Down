using System.Collections.Generic;
using ScriptableObject.ModulesConfig;
using ScriptableObject.ModulesConfig.FlightModule;
using ScriptableObject.ModulesConfig.SupportModules;
using UnityEngine;

namespace Types.record
{
    [System.Serializable]
    public class SaveData
    {
        [SerializeField] int _money;
        [SerializeField] int _diamond;
        [SerializeField] int _energy;

        [SerializeField] List<CustomKeyValuePair<string, bool>> _skinOpenStatus = new();

        public int Money
        {
            get => _money;
            set => _money = value;
        }

        public int Diamond
        {
            get => _diamond;
            set => _diamond = value;
        }

        public int Energy
        {
            get => _energy;
            set => _energy = value;
        }

        public Dictionary<string, bool> SkinOpenStatus
        {
            get
            {
                var dict = new Dictionary<string, bool>();
                foreach (var pair in _skinOpenStatus)
                {
                    if (string.IsNullOrEmpty(pair.Key))
                    {
                        Debug.LogWarning("Encountered a null or empty key in _skinOpenStatus!");
                        continue;
                    }

                    dict.Add(pair.Key, pair.Value);
                }

                return dict;
            }
            set
            {
                _skinOpenStatus.Clear();
                foreach (var kvp in value)
                {
                    _skinOpenStatus.Add(new CustomKeyValuePair<string, bool>(kvp.Key, kvp.Value));
                }
            }
        }

        [field: SerializeField] public EngineModuleConfig.EngineModuleSaveData EngineModuleSaveData { get; set; }
        [field: SerializeField] public AirBrakeModuleConfig.AirBrakeModuleSaveData AirBrakeModuleSaveData { get; set; }
        [field: SerializeField] public PickerModuleConfig.PickerModuleSaveData PickerModuleSaveData { get; set; }
        [field: SerializeField] public HealthModuleConfig.HealthModuleSaveData HealthModuleSaveData { get; set; }
        [field: SerializeField] public EmergencyBrakeModuleConfig.EmergencyBrakeModuleSaveData EmergencyBrakeModuleSaveData { get; set; }
        [field: SerializeField] public RotationModuleConfig.RotationModuleSaveData RotationModuleSaveData { get; set; }
        [field: SerializeField] public WarpEngineModuleConfig.WarpEngineSaveData WarpEngineSaveData { get; set; }

        [field: SerializeField] public SettingConfig.SettingData SettingConfigData { get; set; }
    }
}