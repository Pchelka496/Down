using UnityEngine;

[CreateAssetMenu(fileName = "EngineModuleConfig", menuName = "Scriptable Objects/EngineModuleConfig")]
public class EngineModuleConfig : BaseModuleConfig
{
    [Header("The InterpolateForceApplyRateValue level is bound to the EngineForceIncreaseDuration level")]
    [SerializeField] UpdateCharacteristicsInfo<EnumCharacteristics, float>[] _characteristics;

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
        switch (characteristic)
        {
            case EnumCharacteristics.EngineForceIncreaseDuration:
                {
                    base.SetLevel<EnumCharacteristics, float>(_characteristics, EnumCharacteristics.InterpolateForceApplyRateValue, newLevel);
                    break;
                }
        }

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
        EngineMaxForce,
        ApplyForceRate,
        BoostPower,
        EngineForceIncreaseDuration,
        InterpolateForceApplyRateValue,
        BoosterChargeCount,
        BoosterChargeCooldown
    }

}
