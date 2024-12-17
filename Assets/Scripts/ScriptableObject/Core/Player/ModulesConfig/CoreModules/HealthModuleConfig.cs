using System.Linq;
using Creatures.Player;
using Types.record;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScriptableObject.ModulesConfig
{
    [CreateAssetMenu(fileName = "HealthModuleConfig", menuName = "Scriptable Objects/HealthModuleConfig")]
    public class HealthModuleConfig : BaseModuleConfig
    {
        [SerializeField] UpdateCharacteristicsInfo<EnumCharacteristics, int>[] _characteristics;

        protected UpdateCharacteristicsInfo<EnumCharacteristics, int>[] Characteristics => _characteristics;

        public int MaximumHealth => GetCharacteristicForLevel(_characteristics,
                                                              EnumCharacteristics.MaxHealth,
                                                              GetLevel(EnumCharacteristics.MaxHealth));

        public int RepairKitNumberForRepair => GetCharacteristicForLevel(_characteristics,
                                                                         EnumCharacteristics.RepairKitNumberForRepair,
                                                                         GetLevel(EnumCharacteristics.RepairKitNumberForRepair));

        public override bool ActivityCheck() => true;

        public override System.Type GetModuleType() => typeof(HealthModule);

        public override void SaveToSaveData(SaveData saveData)
        {
            var saveDataRecord = new HealthModuleSaveData
            {
                SavedData = _characteristics.Select(c => new DefaultModuleSaveData
                {
                    CurrentLevel = c.CurrentLevel,
                }).ToArray()
            };

            saveData.HealthModuleSaveData = saveDataRecord;
        }

        public override void LoadSaveData(SaveData saveData)
        {
            var loadedData = saveData.HealthModuleSaveData;
            if (loadedData == null)
            {
                Debug.LogWarning("EngineModuleSaveData is null.");
                return;
            }

            var savedData = loadedData.SavedData;
            if (savedData == null)
            {
                Debug.LogWarning("SavedData is null.");
                return;
            }

            if (savedData.Length != _characteristics.Length)
            {
                Debug.LogWarning(
                    $"Mismatched data lengths: savedData length = {savedData.Length}, _characteristics length = {_characteristics.Length}");
            }

            for (int i = 0; i < _characteristics.Length; i++)
            {
                if (i >= savedData.Length)
                {
                    Debug.LogWarning($"No saved data available for index {i}. Skipping...");
                    continue;
                }

                var savedCharacteristic = savedData[i];

                if (savedCharacteristic == null)
                {
                    Debug.LogWarning($"Saved characteristic at index {i} is null. Skipping...");
                    continue;
                }

                _characteristics[i] = new UpdateCharacteristicsInfo<EnumCharacteristics, int>
                {
                    UpdateType = _characteristics[i].UpdateType,
                    CurrentLevel = savedCharacteristic.CurrentLevel,
                    LevelCost = _characteristics[i].LevelCost,
                    CharacteristicsPerLevel = _characteristics[i].CharacteristicsPerLevel
                };
            }
        }

        public int GetLevel(EnumCharacteristics characteristic)
        {
            var level = base.GetLevel(_characteristics, characteristic);

            if (level == null)
            {
                return 0;
            }

            return level.Value;
        }

        public void SetLevel(EnumCharacteristics characteristic, int newLevel)
        {
            base.SetLevel(_characteristics, characteristic, newLevel);
        }

        public int GetMaxLevel(EnumCharacteristics characteristic)
        {
            var maxLevel = base.GetMaxLevel(_characteristics, characteristic);

            if (maxLevel == null)
            {
                return 0;
            }

            return maxLevel.Value;
        }

        public int GetLevelCost(EnumCharacteristics characteristic, int level)
        {
            var levelCost = base.GetLevelCost(_characteristics, characteristic, level);

            if (levelCost == null)
            {
                return 0;
            }

            return levelCost.Value;
        }

        public enum EnumCharacteristics
        {
            MaxHealth,
            RepairKitNumberForRepair,
        }

        [System.Serializable]
        public record HealthModuleSaveData
        {
            [field: SerializeField] public DefaultModuleSaveData[] SavedData { get; set; }
        }
    }
}