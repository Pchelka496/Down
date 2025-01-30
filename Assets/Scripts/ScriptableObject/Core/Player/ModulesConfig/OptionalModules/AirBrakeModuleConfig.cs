using System.Linq;
using Types.record;
using UnityEngine;

namespace ScriptableObject.ModulesConfig.SupportModules
{
    [CreateAssetMenu(fileName = "AirBrakeConfig", menuName = "Scriptable Objects/AirBrakeConfig")]
    public class AirBrakeModuleConfig : BaseModuleConfig
    {
        [SerializeField] UpdateCharacteristicsInfo<EnumCharacteristics, float>[] _characteristics;
        protected UpdateCharacteristicsInfo<EnumCharacteristics, float>[] Characteristics => _characteristics;

        public float AirMaxBrakeDrag => GetCharacteristicForLevel(_characteristics,
                                                                  EnumCharacteristics.AirBrakeDrag,
                                                                  GetLevel(EnumCharacteristics.AirBrakeDrag));

        public float AirBrakeReleaseRate => GetCharacteristicForLevel(_characteristics,
                                                                      EnumCharacteristics.AirBrakeReleaseRate,
                                                                      GetLevel(EnumCharacteristics.AirBrakeReleaseRate));

        public override bool ActivityCheck() => GetLevel(EnumCharacteristics.AirBrakeDrag) > 0;
        

        public override System.Type GetModuleType() => typeof(AirBrakeModule);

        public override void SaveToSaveData(SaveData saveData)
        {
            var saveDataRecord = new AirBrakeModuleSaveData()
            {
                SavedData = _characteristics.Select(c => new DefaultModuleSaveData
                {
                    CurrentLevel = c.CurrentLevel,
                }).ToArray()
            };

            saveData.AirBrakeModuleSaveData = saveDataRecord;
        }

        public override void LoadSaveData(SaveData saveData)
        {
            var loadedData = saveData.AirBrakeModuleSaveData;
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

                _characteristics[i] = new UpdateCharacteristicsInfo<EnumCharacteristics, float>
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
            AirBrakeDrag,
            AirBrakeReleaseRate,
        }

        [System.Serializable]
        public record AirBrakeModuleSaveData
        {
            [field: SerializeField] public DefaultModuleSaveData[] SavedData { get; set; }
        }
    }
}