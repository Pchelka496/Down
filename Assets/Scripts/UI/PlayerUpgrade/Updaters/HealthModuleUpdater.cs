using UnityEngine;

public class HealthModuleUpdater : MonoBehaviour
{
    [SerializeField] UpgradeInfo _maxHealth;
    [SerializeField] UpgradeInfo _numberOfPartsRequiredForRepair;

    HealthModuleConfig _moduleConfig;
    PlayerUpgradePanel _playerUpgradePanel;

    public void Initialize(HealthModuleConfig moduleConfig, PlayerUpgradePanel playerUpgradePanel)
    {
        _moduleConfig = moduleConfig;
        _playerUpgradePanel = playerUpgradePanel;

        InitializeUpgradeInfo(_maxHealth, HealthModuleConfig.EnumCharacteristics.MaximumHealth, UpgradeMaxHealthLevel, DowngradeMaxHealthLevel);
        InitializeUpgradeInfo(_numberOfPartsRequiredForRepair, HealthModuleConfig.EnumCharacteristics.NumberOfPartsRequiredForRepair, UpgradeNumberOfPartsRequiredForRepairLevel, DowngradeNumberOfPartsRequiredForRepairLevel);
    }

    private void InitializeUpgradeInfo(UpgradeInfo upgradeInfo, HealthModuleConfig.EnumCharacteristics characteristics, System.Action upgradeAction, System.Action downgradeAction)
    {
        upgradeInfo.Initialize(_moduleConfig.GetLevel(characteristics),
                                   _moduleConfig.GetMaxLevel(characteristics),
                                   GetCost(characteristics,
                                   _moduleConfig.GetLevel(characteristics) + 1),
                                   upgradeAction,
                                   downgradeAction
                                   );
    }

    private string GetCost(HealthModuleConfig.EnumCharacteristics characteristics, int level)
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
    public void UpgradeMaxHealthLevel()
    {
        UpgradeLevel(HealthModuleConfig.EnumCharacteristics.MaximumHealth, _maxHealth);
    }

    public void UpgradeNumberOfPartsRequiredForRepairLevel()
    {
        UpgradeLevel(HealthModuleConfig.EnumCharacteristics.NumberOfPartsRequiredForRepair, _numberOfPartsRequiredForRepair);
    }

    public void UpgradeLevel(HealthModuleConfig.EnumCharacteristics characteristics, UpgradeInfo upgradeInfo)
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
    public void DowngradeMaxHealthLevel()
    {
        DowngradeLevel(HealthModuleConfig.EnumCharacteristics.MaximumHealth, _maxHealth);
    }

    public void DowngradeNumberOfPartsRequiredForRepairLevel()
    {
        DowngradeLevel(HealthModuleConfig.EnumCharacteristics.NumberOfPartsRequiredForRepair, _numberOfPartsRequiredForRepair);
    }

    public void DowngradeLevel(HealthModuleConfig.EnumCharacteristics characteristics, UpgradeInfo upgradeInfo)
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
