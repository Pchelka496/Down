using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PickerModuleConfig", menuName = "Scriptable Objects/PickerModuleConfig")]
public class PickerModuleConfig : BaseModuleConfig
{
    [SerializeField] UpdateCharacteristicsInfo<EnumCharacteristics, float>[] _characteristics;

    public float PickUpRadius => base.GetCharacteristicForLevel(_characteristics,
                                                              EnumCharacteristics.PickUpRadius,
                                                              GetLevel(EnumCharacteristics.PickUpRadius)
                                                              );

    public float PickUpRewardMultiplier => base.GetCharacteristicForLevel(_characteristics,
                                                             EnumCharacteristics.PickUpRewardMultiplier,
                                                             GetLevel(EnumCharacteristics.PickUpRewardMultiplier)
                                                             );

    public float GetCharacteristicForLevel(int level, EnumCharacteristics characteristics)
    {
        level = Mathf.Clamp(level, 0, GetMaxLevel(characteristics));

        return GetCharacteristicForLevel(_characteristics,
                                         characteristics,
                                         level
                                         );
    }

    public override bool ActivityCheck() => true;

    public override System.Type GetModuleType() => typeof(PickerModule);

    public int GetLevel(EnumCharacteristics characteristic)
    {
        var level = base.GetLevel<EnumCharacteristics, float>(_characteristics, characteristic);

        if (level == null)
        {
            return 0;
        }

        return level.Value;
    }

    public void SetLevel(EnumCharacteristics characteristic, int newLevel)
    {
        base.SetLevel<EnumCharacteristics, float>(_characteristics, characteristic, newLevel);
    }

    public int GetMaxLevel(EnumCharacteristics characteristic)
    {
        var maxLevel = base.GetMaxLevel<EnumCharacteristics, float>(_characteristics, characteristic);

        if (maxLevel == null)
        {
            return 0;
        }

        return maxLevel.Value;
    }

    public int GetLevelCost(EnumCharacteristics characteristic, int level)
    {
        var levelCost = base.GetLevelCost<EnumCharacteristics, float>(_characteristics, characteristic, level);

        if (levelCost == null)
        {
            return 0;
        }

        return levelCost.Value;
    }

    public enum EnumCharacteristics
    {
        PickUpRadius,
        PickUpRewardMultiplier,
    }

}
