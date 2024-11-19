using UnityEngine;

[CreateAssetMenu(fileName = "RotationModuleConfig", menuName = "Scriptable Objects/RotationModuleConfig")]
public class RotationModuleConfig : BaseModuleConfig
{
    [SerializeField] UpdateCharacteristicsInfo<EnumCharacteristics, float>[] _characteristics;

    public float RotationSpeed => GetCharacteristicForLevel(_characteristics,
                                                              EnumCharacteristics.RotationSpeed,
                                                              GetLevel(EnumCharacteristics.RotationSpeed)
                                                              );

    //public float TurningResistance => GetCharacteristicForLevel(_characteristics,
    //                                                         EnumCharacteristics.TurningResistance,
    //                                                         GetLevel(EnumCharacteristics.TurningResistance)
    //                                                         );

    public override bool ActivityCheck() => true;

    public override System.Type GetModuleType() => typeof(RotationModule);

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
        RotationSpeed,
        //TurningResistance,
    }

}
