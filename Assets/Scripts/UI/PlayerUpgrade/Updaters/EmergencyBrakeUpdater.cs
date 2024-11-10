using UnityEngine;

public class EmergencyBrakeUpdater : MonoBehaviour
{
    [SerializeField] UpgradeInfo _chargeQuantity;
    [SerializeField] UpgradeInfo _chargeCooldown;
    [SerializeField] UpgradeInfo _stopRate;
    [SerializeField] UpgradeInfo _sensingDistance;

    EmergencyBrakeModuleConfig _moduleConfig;
    PlayerUpgradePanel _playerUpgradePanel;

    public void Initialize(EmergencyBrakeModuleConfig moduleConfig, PlayerUpgradePanel playerUpgradePanel)
    {
        _moduleConfig = moduleConfig;
        _playerUpgradePanel = playerUpgradePanel;

        InitializeUpgradeInfo(_chargeQuantity, EmergencyBrakeModuleConfig.EnumCharacteristics.ChargeQuantity, UpgradeChargeQuantity, DowngradeChargeQuantity);
        InitializeUpgradeInfo(_chargeCooldown, EmergencyBrakeModuleConfig.EnumCharacteristics.ChargeCooldown, UpgradeChargeCooldown, DowngradeChargeCooldown);
        InitializeUpgradeInfo(_stopRate, EmergencyBrakeModuleConfig.EnumCharacteristics.StopRate, UpgradeStopRate, DowngradeStopRate);
        InitializeUpgradeInfo(_sensingDistance, EmergencyBrakeModuleConfig.EnumCharacteristics.SensingDistance, UpgradeSensingDistance, DowngradeSensingDistance);
    }

    private void InitializeUpgradeInfo(UpgradeInfo upgradeInfo, EmergencyBrakeModuleConfig.EnumCharacteristics characteristics, System.Action upgradeAction, System.Action downgradeAction)
    {
        upgradeInfo.Initialize(_moduleConfig.GetLevel(characteristics),
                                   _moduleConfig.GetMaxLevel(characteristics),
                                   GetCost(characteristics,
                                   _moduleConfig.GetLevel(characteristics) + 1),
                                   upgradeAction,
                                   downgradeAction
                                   );
    }

    private string GetCost(EmergencyBrakeModuleConfig.EnumCharacteristics characteristics, int level)
    {
        if (level >= _moduleConfig.GetMaxLevel(characteristics))
        {
            return "Max level";
        }

        var cost = _moduleConfig.GetLevelCost(characteristics, level);

        if (cost <= 0)
        {
            return "Free";
        }

        return cost.ToString();
    }

    //_________________________________ Upgrade Button _________________________________
    public void UpgradeChargeQuantity()
    {
        UpgradeLevel(EmergencyBrakeModuleConfig.EnumCharacteristics.ChargeQuantity, _chargeQuantity);
    }

    public void UpgradeChargeCooldown()
    {
        UpgradeLevel(EmergencyBrakeModuleConfig.EnumCharacteristics.ChargeCooldown, _chargeCooldown);
    }

    public void UpgradeStopRate()
    {
        UpgradeLevel(EmergencyBrakeModuleConfig.EnumCharacteristics.StopRate, _stopRate);
    }

    public void UpgradeSensingDistance()
    {
        UpgradeLevel(EmergencyBrakeModuleConfig.EnumCharacteristics.SensingDistance, _sensingDistance);
    }

    public void UpgradeLevel(EmergencyBrakeModuleConfig.EnumCharacteristics characteristics, UpgradeInfo upgradeInfo)
    {
        var currentLevel = _moduleConfig.GetLevel(characteristics);
        var nextLevel = currentLevel + 1;

        if (!_playerUpgradePanel.UpgradeLevelCheck(_moduleConfig.GetLevelCost(characteristics, nextLevel),
                                                  currentLevel,
                                                  _moduleConfig.GetMaxLevel(characteristics)
                                                  ))
        {
            return;
        }

        upgradeInfo.UpdateCurrentLevel(nextLevel, GetCost(characteristics, nextLevel));
        _moduleConfig.SetLevel(characteristics, nextLevel);
    }

    //_________________________________ Downgrade Button _________________________________
    public void DowngradeChargeQuantity()
    {
        DowngradeLevel(EmergencyBrakeModuleConfig.EnumCharacteristics.ChargeQuantity, _chargeQuantity);
    }

    public void DowngradeChargeCooldown()
    {
        DowngradeLevel(EmergencyBrakeModuleConfig.EnumCharacteristics.ChargeCooldown, _chargeCooldown);
    }

    public void DowngradeStopRate()
    {
        DowngradeLevel(EmergencyBrakeModuleConfig.EnumCharacteristics.StopRate, _stopRate);
    }

    public void DowngradeSensingDistance()
    {
        DowngradeLevel(EmergencyBrakeModuleConfig.EnumCharacteristics.SensingDistance, _sensingDistance);
    }

    public void DowngradeLevel(EmergencyBrakeModuleConfig.EnumCharacteristics characteristics, UpgradeInfo upgradeInfo)
    {
        var currentLevel = _moduleConfig.GetLevel(characteristics);
        var nextLevel = currentLevel - 1;

        if (!_playerUpgradePanel.DowngradeLevelCheck(currentLevel))
        {
            return;
        }

        upgradeInfo.UpdateCurrentLevel(nextLevel, GetCost(characteristics, nextLevel));
        _moduleConfig.SetLevel(characteristics, nextLevel);
    }

}
