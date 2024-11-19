using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseModuleConfig : ScriptableObject
{
    [SerializeField] bool _resetThePriceAfterPurchase;

    public abstract bool ActivityCheck();
    public abstract System.Type GetModuleType();

    protected Characteristics GetCharacteristicForLevel<EnumCharacteristics, Characteristics>(
        UpdateCharacteristicsInfo<EnumCharacteristics, Characteristics>[] allCharacteristicsInfo,
        EnumCharacteristics enumValue,
        int level
        ) where EnumCharacteristics : struct, Enum
    {
        if (!LevelCheck(allCharacteristicsInfo, enumValue, level))
        {
            var maxLevel = GetMaxLevel(allCharacteristicsInfo, enumValue);

            if (maxLevel == null)
            {
                maxLevel = 0;
            }

            level = Mathf.Clamp(level, 0, maxLevel.Value);
        }

        foreach (var info in allCharacteristicsInfo)
        {
            if (EqualityComparer<EnumCharacteristics>.Default.Equals(info.UpdateType, enumValue))
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

    protected int? GetLevel<EnumCharacteristics, Characteristics>(
        UpdateCharacteristicsInfo<EnumCharacteristics, Characteristics>[] allCharacteristicsInfo,
        EnumCharacteristics enumValue
        ) where EnumCharacteristics : struct, Enum
    {
        foreach (var info in allCharacteristicsInfo)
        {
            if (EqualityComparer<EnumCharacteristics>.Default.Equals(info.UpdateType, enumValue))
            {
                return info.CurrentLevel;
            }
        }

        Debug.LogWarning($"Characteristic {enumValue} not found.");
        return null;
    }

    protected void SetLevel<EnumCharacteristics, Characteristics>(
        UpdateCharacteristicsInfo<EnumCharacteristics, Characteristics>[] allCharacteristicsInfo,
        EnumCharacteristics enumValue,
        int newLevel
        ) where EnumCharacteristics : struct, Enum
    {
        foreach (var info in allCharacteristicsInfo)
        {
            if (EqualityComparer<EnumCharacteristics>.Default.Equals(info.UpdateType, enumValue))
            {
                if (!LevelCheck(allCharacteristicsInfo, enumValue, newLevel))
                {
                    Debug.LogWarning($"{this.GetType()} level {newLevel} is out of bounds! Array length: {GetMaxLevel(allCharacteristicsInfo, enumValue) + 1}");

                    var maxLevel = GetMaxLevel(allCharacteristicsInfo, enumValue);

                    if (maxLevel == null)
                    {
                        maxLevel = 0;
                    }

                    newLevel = Mathf.Clamp(newLevel, 0, maxLevel.Value);
                }

                info.CurrentLevel = newLevel;

                if (_resetThePriceAfterPurchase)
                {
                    SetLevelCostToZero(allCharacteristicsInfo, enumValue, newLevel);
                }
                return;
            }
        }
        Debug.LogWarning($"Characteristic {enumValue} not found.");
    }

    protected int? GetMaxLevel<EnumCharacteristics, Characteristics>(
        UpdateCharacteristicsInfo<EnumCharacteristics, Characteristics>[] allCharacteristicsInfo,
        EnumCharacteristics enumValue
        ) where EnumCharacteristics : struct, Enum
    {
        foreach (var info in allCharacteristicsInfo)
        {
            if (EqualityComparer<EnumCharacteristics>.Default.Equals(info.UpdateType, enumValue))
            {
                return info.CharacteristicsPerLevel.Length - 1;
            }
        }
        Debug.LogWarning($"Characteristic {enumValue} not found.");
        return null;
    }

    protected int? GetLevelCost<EnumCharacteristics, Characteristics>(
        UpdateCharacteristicsInfo<EnumCharacteristics, Characteristics>[] allCharacteristicsInfo,
        EnumCharacteristics enumValue,
        int level
        ) where EnumCharacteristics : struct, Enum
    {
        if (!LevelCheck(allCharacteristicsInfo, enumValue, level))
        {
            var maxLevel = GetMaxLevel(allCharacteristicsInfo, enumValue);

            if (maxLevel == null)
            {
                maxLevel = 0;
            }

            level = Mathf.Clamp(level, 0, maxLevel.Value);
        }

        foreach (var info in allCharacteristicsInfo)
        {
            if (EqualityComparer<EnumCharacteristics>.Default.Equals(info.UpdateType, enumValue))
            {
                return info.LevelCost[level];
            }
        }

        Debug.LogWarning($"Characteristic {enumValue} not found.");
        return null;
    }

    protected void SetLevelCostToZero<EnumCharacteristics, Characteristics>(
        UpdateCharacteristicsInfo<EnumCharacteristics, Characteristics>[] allCharacteristicsInfo,
        EnumCharacteristics enumValue,
        int level
        ) where EnumCharacteristics : struct, Enum
    {
        foreach (var info in allCharacteristicsInfo)
        {
            if (EqualityComparer<EnumCharacteristics>.Default.Equals(info.UpdateType, enumValue))
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

    protected bool LevelCheck<EnumCharacteristics, Characteristics>(
        UpdateCharacteristicsInfo<EnumCharacteristics, Characteristics>[] allCharacteristicsInfo,
        EnumCharacteristics enumValue,
        int level
        ) where EnumCharacteristics : struct, Enum
    {
        if (level < 0)
            return false;

        foreach (var info in allCharacteristicsInfo)
        {
            if (EqualityComparer<EnumCharacteristics>.Default.Equals(info.UpdateType, enumValue))
            {
                return level <= info.CharacteristicsPerLevel.Length - 1;
            }
        }

        Debug.LogWarning($"Characteristic {enumValue} not found.");
        return false;
    }

    [Serializable]
    protected class UpdateCharacteristicsInfo<EnumCharacteristics, Characteristics> where EnumCharacteristics : struct, Enum
    {
        [SerializeField] EnumCharacteristics updateType;
        [SerializeField] int currentLevel;
        [SerializeField] Characteristics[] characteristicsPerLevel;
        [SerializeField] int[] levelCost;

        public EnumCharacteristics UpdateType { get => updateType; set => updateType = value; }
        public int CurrentLevel { get => currentLevel; set => currentLevel = value; }
        public Characteristics[] CharacteristicsPerLevel { get => characteristicsPerLevel; set => characteristicsPerLevel = value; }
        public int[] LevelCost { get => levelCost; set => levelCost = value; }

    }

}

