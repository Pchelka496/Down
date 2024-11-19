using UnityEngine;

[CreateAssetMenu(fileName = "AirBrakeConfig", menuName = "Scriptable Objects/AirBrakeConfig")]
public class AirBrakeModuleConfig : BaseModuleConfig
{
    [SerializeField] UpdateCharacteristicsInfo<EnumCharacteristics, float>[] _characteristics;

    public float AirMaxBrakeDrag => GetCharacteristicForLevel(_characteristics,
                                                             EnumCharacteristics.AirBrakeDrag,
                                                             GetLevel(EnumCharacteristics.AirBrakeDrag)
                                                             );
    public float AirBrakeReleaseRate => GetCharacteristicForLevel(_characteristics,
                                                             EnumCharacteristics.AirBrakeReleaseRate,
                                                             GetLevel(EnumCharacteristics.AirBrakeReleaseRate)
                                                             );

    public override bool ActivityCheck() => GetLevel(EnumCharacteristics.AirBrakeDrag) > 0;

    public override System.Type GetModuleType() => typeof(AirBrakeModule);

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
        AirBrakeDrag,
        AirBrakeReleaseRate,
    }

}
