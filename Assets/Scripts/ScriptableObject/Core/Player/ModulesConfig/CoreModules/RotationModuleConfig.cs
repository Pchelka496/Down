using System.Linq;
using Types.record;
using UnityEngine;

namespace ScriptableObject.ModulesConfig.SupportModules
{
    [CreateAssetMenu(fileName = "RotationModuleConfig", menuName = "Scriptable Objects/RotationModuleConfig")]
    public class RotationModuleConfig : BaseModuleConfig
    {
        [SerializeField] UpdateCharacteristicsInfo<EnumCharacteristics, float>[] _characteristics;

        protected UpdateCharacteristicsInfo<EnumCharacteristics, float>[] Characteristics => _characteristics;

        public float RotationSpeed => GetCharacteristicForLevel(_characteristics,
            EnumCharacteristics.RotationSpeed,
            GetLevel(EnumCharacteristics.RotationSpeed)
        );

        public override bool ActivityCheck() => true;

        public override System.Type GetModuleType() => typeof(RotationModule);

        public override void SaveToSaveData(SaveData saveData)
        {
            var saveDataRecord = new RotationModuleSaveData()
            {
                SavedData = _characteristics.Select(c => new DefaultModuleSaveData
                {
                    CurrentLevel = c.CurrentLevel,
                    LevelCost = c.LevelCost?.Clone() as int[]
                }).ToArray()
            };

            saveData.RotationModuleSaveData = saveDataRecord;
        }

        public override void LoadSaveData(SaveData saveData)
        {
            var loadedData = saveData.RotationModuleSaveData;
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
                    LevelCost = savedCharacteristic.LevelCost
                        ?.Clone() as int[],
                    CharacteristicsPerLevel =
                        _characteristics[i].CharacteristicsPerLevel
                            ?.Clone() as float[]
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
            RotationSpeed,
        }

        [System.Serializable]
        public record RotationModuleSaveData
        {
            [field: SerializeField] public DefaultModuleSaveData[] SavedData { get; set; }
        }
    }
}