using System;
using System.Collections.Generic;
using Types.record;
using UnityEngine;
using UnityEngine.Serialization;

namespace ScriptableObject.ModulesConfig
{
    public abstract class BaseModuleConfig : UnityEngine.ScriptableObject, IHaveDataForSave
    {
        [SerializeField] bool _resetThePriceAfterPurchase;
        protected Action<BaseModuleConfig> _saveAction;

        public abstract bool ActivityCheck();
        public abstract Type GetModuleType();

        public abstract void SaveToSaveData(SaveData saveData);
        public abstract void LoadSaveData(SaveData saveData);

        Action IHaveDataForSave.SubscribeWithUnsubscribe(Action<IHaveDataForSave> saveAction)
        {
            _saveAction = saveAction;
            return () => { _saveAction = null; };
        }

        protected TCharacteristics GetCharacteristicForLevel<TEnumCharacteristics, TCharacteristics>(
            UpdateCharacteristicsInfo<TEnumCharacteristics, TCharacteristics>[] allCharacteristicsInfo,
            TEnumCharacteristics enumValue,
            int level
            ) where TEnumCharacteristics : struct, Enum
        {
            if (!LevelCheck(allCharacteristicsInfo, enumValue, level))
            {
                var maxLevel = GetMaxLevel(allCharacteristicsInfo, enumValue) ?? 0;

                level = Mathf.Clamp(level, 0, maxLevel);
            }

            foreach (var info in allCharacteristicsInfo)
            {
                if (EqualityComparer<TEnumCharacteristics>.Default.Equals(info.UpdateType, enumValue))
                {
                    if (level >= 0 && level < info.CharacteristicsPerLevel.Length)
                    {
                        return info.CharacteristicsPerLevel[level];
                    }
                    else
                    {
                        Debug.LogWarning($"Level {level} is out of bounds for characteristic {enumValue}.");
                        return default;
                    }
                }
            }

            Debug.LogWarning($"Characteristic {enumValue} not found.");
            return default;
        }

        protected int? GetLevel<TEnumCharacteristics, TCharacteristics>(
            UpdateCharacteristicsInfo<TEnumCharacteristics,
            TCharacteristics>[] allCharacteristicsInfo,
            TEnumCharacteristics enumValue
            ) where TEnumCharacteristics : struct, Enum
        {
            foreach (var info in allCharacteristicsInfo)
            {
                if (EqualityComparer<TEnumCharacteristics>.Default.Equals(info.UpdateType, enumValue))
                {
                    return info.CurrentLevel;
                }
            }

            Debug.LogWarning($"Characteristic {enumValue} not found.");
            return null;
        }

        protected void SetLevel<TEnumCharacteristics, TCharacteristics>(
            UpdateCharacteristicsInfo<TEnumCharacteristics, TCharacteristics>[] allCharacteristicsInfo,
            TEnumCharacteristics enumValue,
            int newLevel
        ) where TEnumCharacteristics : struct, Enum
        {
            foreach (var info in allCharacteristicsInfo)
            {
                if (EqualityComparer<TEnumCharacteristics>.Default.Equals(info.UpdateType, enumValue))
                {
                    if (!LevelCheck(allCharacteristicsInfo, enumValue, newLevel))
                    {
                        Debug.LogWarning(
                            $"{GetType()} level {newLevel} is out of bounds! Array length: {GetMaxLevel(allCharacteristicsInfo, enumValue) + 1}");

                        var maxLevel = GetMaxLevel(allCharacteristicsInfo, enumValue) ?? 0;

                        newLevel = Mathf.Clamp(newLevel, 0, maxLevel);
                    }

                    info.CurrentLevel = newLevel;

                    if (_resetThePriceAfterPurchase)
                    {
                        SetLevelCostToZero(allCharacteristicsInfo, enumValue, newLevel);
                    }

                    _saveAction?.Invoke(this);
                    return;
                }
            }

            Debug.LogWarning($"Characteristic {enumValue} not found.");
        }

        protected int? GetMaxLevel<TEnumCharacteristics, TCharacteristics>(
            UpdateCharacteristicsInfo<TEnumCharacteristics, TCharacteristics>[] allCharacteristicsInfo,
            TEnumCharacteristics enumValue
            ) where TEnumCharacteristics : struct, Enum
        {
            foreach (var info in allCharacteristicsInfo)
            {
                if (EqualityComparer<TEnumCharacteristics>.Default.Equals(info.UpdateType, enumValue))
                {
                    return info.CharacteristicsPerLevel.Length - 1;
                }
            }

            Debug.LogWarning($"Characteristic {enumValue} not found.");
            return null;
        }

        protected int? GetLevelCost<TEnumCharacteristics, TCharacteristics>(
            UpdateCharacteristicsInfo<TEnumCharacteristics, TCharacteristics>[] allCharacteristicsInfo,
            TEnumCharacteristics enumValue,
            int level
            ) where TEnumCharacteristics : struct, Enum
        {
            if (!LevelCheck(allCharacteristicsInfo, enumValue, level))
            {
                var maxLevel = GetMaxLevel(allCharacteristicsInfo, enumValue) ?? 0;

                level = Mathf.Clamp(level, 0, maxLevel);
            }

            foreach (var info in allCharacteristicsInfo)
            {
                if (EqualityComparer<TEnumCharacteristics>.Default.Equals(info.UpdateType, enumValue))
                {
                    return info.LevelCost[level];
                }
            }

            Debug.LogWarning($"Characteristic {enumValue} not found.");
            return null;
        }

        protected void SetLevelCostToZero<TEnumCharacteristics, TCharacteristics>(
            UpdateCharacteristicsInfo<TEnumCharacteristics, TCharacteristics>[] allCharacteristicsInfo,
            TEnumCharacteristics enumValue,
            int level
            ) where TEnumCharacteristics : struct, Enum
        {
            foreach (var info in allCharacteristicsInfo)
            {
                if (EqualityComparer<TEnumCharacteristics>.Default.Equals(info.UpdateType, enumValue))
                {
                    if (LevelCheck(allCharacteristicsInfo, enumValue, level))
                    {
                        info.LevelCost[level] = 0;
                    }
                    else
                    {
                        Debug.LogWarning($"Level {level} is out of bounds for characteristic {enumValue}.");
                    }

                    return;
                }
            }

            Debug.LogWarning($"Characteristic {enumValue} not found.");
        }

        protected bool LevelCheck<TEnumCharacteristics, TCharacteristics>(
            UpdateCharacteristicsInfo<TEnumCharacteristics, TCharacteristics>[] allCharacteristicsInfo,
            TEnumCharacteristics enumValue,
            int level
            ) where TEnumCharacteristics : struct, Enum
        {
            if (level < 0)
                return false;

            foreach (var info in allCharacteristicsInfo)
            {
                if (EqualityComparer<TEnumCharacteristics>.Default.Equals(info.UpdateType, enumValue))
                {
                    return level <= info.CharacteristicsPerLevel.Length - 1;
                }
            }

            Debug.LogWarning($"Characteristic {enumValue} not found.");
            return false;
        }

        [Serializable]
        public class UpdateCharacteristicsInfo<TEnumCharacteristics, TCharacteristics>
            where TEnumCharacteristics : struct, Enum
        {
            [FormerlySerializedAs("updateType")]
            [SerializeField]
            TEnumCharacteristics _updateType;

            [FormerlySerializedAs("currentLevel")]
            [SerializeField]
            int _currentLevel;

            [FormerlySerializedAs("characteristicsPerLevel")]
            [SerializeField]
            TCharacteristics[] _characteristicsPerLevel;

            [FormerlySerializedAs("levelCost")]
            [SerializeField]
            int[] _levelCost;

            public TEnumCharacteristics UpdateType
            {
                get => _updateType;
                set => _updateType = value;
            }

            public int CurrentLevel
            {
                get => _currentLevel;
                set => _currentLevel = value;
            }

            public TCharacteristics[] CharacteristicsPerLevel
            {
                get => _characteristicsPerLevel;
                set => _characteristicsPerLevel = value;
            }

            public int[] LevelCost
            {
                get => _levelCost;
                set => _levelCost = value;
            }
        }

        [Serializable]
        public record DefaultModuleSaveData
        {
            [field: SerializeField] public int CurrentLevel { get; set; }
            [field: SerializeField] public int[] LevelCost { get; set; }
        }
    }
}