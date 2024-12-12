using System.Linq;
using Creatures.Player.PlayerModule.CoreModules.EngineModule;
using Types.record;
using UnityEngine;

namespace ScriptableObject.ModulesConfig.FlightModule
{
    [CreateAssetMenu(fileName = "EngineModuleConfig", menuName = "Scriptable Objects/EngineModuleConfig")]
    public class EngineModuleConfig : BaseModuleConfig
    {
        [Header("The InterpolateForceApplyRateValue level is bound to the EngineForceIncreaseDuration level")]
        [SerializeField]
        UpdateCharacteristicsInfo<EnumCharacteristics, float>[] _characteristics;

        protected UpdateCharacteristicsInfo<EnumCharacteristics, float>[] Characteristics => _characteristics;

        public float EngineMaxForce => GetCharacteristicForLevel(_characteristics,
            EnumCharacteristics.EngineMaxForce,
            GetLevel(EnumCharacteristics.EngineMaxForce)
        );

        public float ApplyForceRate => GetCharacteristicForLevel(_characteristics,
            EnumCharacteristics.ApplyForceRate,
            GetLevel(EnumCharacteristics.ApplyForceRate)
        );

        public float BoostPower => GetCharacteristicForLevel(_characteristics,
            EnumCharacteristics.BoostPower,
            GetLevel(EnumCharacteristics.BoostPower)
        );

        public float BoosterChargeCount => GetCharacteristicForLevel(_characteristics,
            EnumCharacteristics.BoosterChargeCount,
            GetLevel(EnumCharacteristics.BoosterChargeCount)
        );

        public float BoosterChargeCooldown => GetCharacteristicForLevel(_characteristics,
            EnumCharacteristics.BoosterChargeCooldown,
            GetLevel(EnumCharacteristics.BoosterChargeCooldown)
        );

        public float EngineForceIncreaseDuration => GetCharacteristicForLevel(_characteristics,
            EnumCharacteristics.EngineForceIncreaseDuration,
            GetLevel(EnumCharacteristics.EngineForceIncreaseDuration)
        );

        public float InterpolateForceApplyRateValue => GetCharacteristicForLevel(_characteristics,
            EnumCharacteristics.InterpolateForceApplyRateValue,
            GetLevel(EnumCharacteristics.InterpolateForceApplyRateValue)
        );

        public override bool ActivityCheck() => true;

        public override System.Type GetModuleType() => typeof(EngineModule);

        public override void SaveToSaveData(SaveData saveData)
        {
            var saveDataRecord = new EngineModuleSaveData
            {
                SavedData = _characteristics.Select(c => new DefaultModuleSaveData
                {
                    CurrentLevel = c.CurrentLevel,
                    LevelCost = c.LevelCost?.Clone() as int[]
                }).ToArray()
            };
            saveData.EngineModuleSaveData = saveDataRecord;
        }

        public override void LoadSaveData(SaveData saveData)
        {
            var engineModuleSaveData = saveData.EngineModuleSaveData;
            if (engineModuleSaveData == null)
            {
                Debug.LogWarning("EngineModuleSaveData is null.");
                return;
            }

            var savedData = engineModuleSaveData.SavedData;
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
            switch (characteristic)
            {
                case EnumCharacteristics.EngineForceIncreaseDuration:
                {
                    base.SetLevel(_characteristics, EnumCharacteristics.InterpolateForceApplyRateValue, newLevel);
                    break;
                }
            }

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
            EngineMaxForce,
            ApplyForceRate,
            BoostPower,
            EngineForceIncreaseDuration,
            InterpolateForceApplyRateValue,
            BoosterChargeCount,
            BoosterChargeCooldown
        }

        [System.Serializable]
        public record EngineModuleSaveData
        {
            [field: SerializeField] public DefaultModuleSaveData[] SavedData { get; set; }
        }
    }
}