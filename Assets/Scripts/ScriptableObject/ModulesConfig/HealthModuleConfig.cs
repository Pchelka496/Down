using UnityEngine;

[CreateAssetMenu(fileName = "HealthModuleConfig", menuName = "Scriptable Objects/HealthModuleConfig")]
public class HealthModuleConfig : BaseModuleConfig
{
    [SerializeField] UpdateCharacteristicsInfo<EnumCharacteristics, int>[] _characteristics;

    public int MaximumHealth => GetCharacteristicForLevel(_characteristics,
                                                             EnumCharacteristics.MaxHealth,
                                                             GetLevel(EnumCharacteristics.MaxHealth)
                                                             );

    public int RepairKitNumberForRepair => GetCharacteristicForLevel(_characteristics,
                                                             EnumCharacteristics.RepairKitNumberForRepair,
                                                             GetLevel(EnumCharacteristics.RepairKitNumberForRepair)
                                                             );

    public override bool ActivityCheck() => true;

    public override System.Type GetModuleType() => typeof(HealthModule);

    public int GetLevel(EnumCharacteristics characteristic)
    {
        var level = base.GetLevel<EnumCharacteristics, int>(_characteristics, characteristic);

        if (level == null)
        {
            return 0;
        }

        return level.Value;
    }

    public void SetLevel(EnumCharacteristics characteristic, int newLevel)
    {
        base.SetLevel<EnumCharacteristics, int>(_characteristics, characteristic, newLevel);
    }

    public int GetMaxLevel(EnumCharacteristics characteristic)
    {
        var maxLevel = base.GetMaxLevel<EnumCharacteristics, int>(_characteristics, characteristic);

        if (maxLevel == null)
        {
            return 0;
        }

        return maxLevel.Value;
    }

    public int GetLevelCost(EnumCharacteristics characteristic, int level)
    {
        var levelCost = base.GetLevelCost<EnumCharacteristics, int>(_characteristics, characteristic, level);

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

}

