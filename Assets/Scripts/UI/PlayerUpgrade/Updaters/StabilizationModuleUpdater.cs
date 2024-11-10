using UnityEngine;

public class StabilizationModuleUpdater : MonoBehaviour
{
    [SerializeField] UpgradeInfo _stabilizationSpeed;
    [SerializeField] UpgradeInfo _turningResistance;

    StabilizationModuleConfig _moduleConfig;
    PlayerUpgradePanel _playerUpgradePanel;

    public void Initialize(StabilizationModuleConfig moduleConfig, PlayerUpgradePanel playerUpgradePanel)
    {
        _moduleConfig = moduleConfig;
        _playerUpgradePanel = playerUpgradePanel;

        InitializeUpgradeInfo(_stabilizationSpeed, StabilizationModuleConfig.EnumCharacteristics.StabilizationSpeed, UpgradeStabilizationSpeed, DowngradeStabilizationSpeed);
        InitializeUpgradeInfo(_turningResistance, StabilizationModuleConfig.EnumCharacteristics.TurningResistance, UpgradeTurningResistance, DowngradeTurningResistance);
    }

    private void InitializeUpgradeInfo(UpgradeInfo upgradeInfo, StabilizationModuleConfig.EnumCharacteristics characteristics, System.Action upgradeAction, System.Action downgradeAction)
    {
        upgradeInfo.Initialize(_moduleConfig.GetLevel(characteristics),
                                   _moduleConfig.GetMaxLevel(characteristics),
                                   GetCost(characteristics,
                                   _moduleConfig.GetLevel(characteristics) + 1),
                                   upgradeAction,
                                   downgradeAction
                                   );
    }

    private string GetCost(StabilizationModuleConfig.EnumCharacteristics characteristics, int level)
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
    public void UpgradeStabilizationSpeed()
    {
        UpgradeLevel(StabilizationModuleConfig.EnumCharacteristics.StabilizationSpeed, _stabilizationSpeed);
    }

    public void UpgradeTurningResistance()
    {
        UpgradeLevel(StabilizationModuleConfig.EnumCharacteristics.TurningResistance, _turningResistance);
    }

    public void UpgradeLevel(StabilizationModuleConfig.EnumCharacteristics characteristics, UpgradeInfo upgradeInfo)
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
    public void DowngradeStabilizationSpeed()
    {
        DowngradeLevel(StabilizationModuleConfig.EnumCharacteristics.StabilizationSpeed, _stabilizationSpeed);
    }

    public void DowngradeTurningResistance()
    {
        DowngradeLevel(StabilizationModuleConfig.EnumCharacteristics.TurningResistance, _turningResistance);
    }

    public void DowngradeLevel(StabilizationModuleConfig.EnumCharacteristics characteristics, UpgradeInfo upgradeInfo)
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
