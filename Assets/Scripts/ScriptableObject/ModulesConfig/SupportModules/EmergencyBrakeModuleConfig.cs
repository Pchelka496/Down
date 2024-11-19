using UnityEngine;

[CreateAssetMenu(fileName = "EmergencyBrakeModuleConfig", menuName = "Scriptable Objects/EmergencyBrakeModuleConfig")]
public class EmergencyBrakeModuleConfig : BaseModuleConfig
{
    [Header("SensingDistance is y collider offset(from -0.1 to the lower value)")]
    [Header("MaxCharges is int")]
    [SerializeField] UpdateCharacteristicsInfo<EnumCharacteristics, float>[] _characteristics;

    public int MaxCharges => (int)GetCharacteristicForLevel(_characteristics,
                                                             EnumCharacteristics.MaxCharges,
                                                             GetLevel(EnumCharacteristics.MaxCharges)
                                                             );

    public float ChargeCooldown => GetCharacteristicForLevel(_characteristics,
                                                             EnumCharacteristics.ChargeCooldown,
                                                             GetLevel(EnumCharacteristics.ChargeCooldown)
                                                             );

    public float StopRate => GetCharacteristicForLevel(_characteristics,
                                                             EnumCharacteristics.StopRate,
                                                             GetLevel(EnumCharacteristics.StopRate)
                                                             );

    public float SensingDistance => GetCharacteristicForLevel(_characteristics,
                                                             EnumCharacteristics.SensingDistance,
                                                             GetLevel(EnumCharacteristics.SensingDistance)
                                                             );

    public override bool ActivityCheck() => true;

    public override System.Type GetModuleType() => typeof(EmergencyBrakeModule);

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
        MaxCharges,
        ChargeCooldown,
        StopRate,
        SensingDistance,
    }

}
