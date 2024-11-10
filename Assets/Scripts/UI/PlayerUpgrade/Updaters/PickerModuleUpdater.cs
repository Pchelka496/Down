using UnityEngine;

public class PickerModuleUpdater : MonoBehaviour
{
    [SerializeField] UpgradeInfo _pickUpRadius;
    [SerializeField] UpgradeInfo _pickUpMultiplier;

    PickerModuleConfig _moduleConfig;
    PlayerUpgradePanel _playerUpgradePanel;

    public void Initialize(PickerModuleConfig moduleConfig, PlayerUpgradePanel playerUpgradePanel)
    {
        _moduleConfig = moduleConfig;
        _playerUpgradePanel = playerUpgradePanel;

        InitializeUpgradeInfo(_pickUpRadius, PickerModuleConfig.EnumCharacteristics.PickUpRadius, UpgradePickUpRadius, DowngradePickUpRadius);
        InitializeUpgradeInfo(_pickUpMultiplier, PickerModuleConfig.EnumCharacteristics.PickUpMultiplier, UpgradePickUpMultiplier, DowngradePickUpMultiplier);
    }

    private void InitializeUpgradeInfo(UpgradeInfo upgradeInfo, PickerModuleConfig.EnumCharacteristics characteristics, System.Action upgradeAction, System.Action downgradeAction)
    {
        upgradeInfo.Initialize(_moduleConfig.GetLevel(characteristics),
                                   _moduleConfig.GetMaxLevel(characteristics),
                                   GetCost(characteristics,
                                   _moduleConfig.GetLevel(characteristics) + 1),
                                   upgradeAction,
                                   downgradeAction
                                   );
    }

    private string GetCost(PickerModuleConfig.EnumCharacteristics characteristics, int level)
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
    public void UpgradePickUpRadius()
    {
        UpgradeLevel(PickerModuleConfig.EnumCharacteristics.PickUpRadius, _pickUpRadius);
    }

    public void UpgradePickUpMultiplier()
    {
        UpgradeLevel(PickerModuleConfig.EnumCharacteristics.PickUpMultiplier, _pickUpMultiplier);
    }

    public void UpgradeLevel(PickerModuleConfig.EnumCharacteristics characteristics, UpgradeInfo upgradeInfo)
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
    public void DowngradePickUpRadius()
    {
        DowngradeLevel(PickerModuleConfig.EnumCharacteristics.PickUpRadius, _pickUpRadius);
    }

    public void DowngradePickUpMultiplier()
    {
        DowngradeLevel(PickerModuleConfig.EnumCharacteristics.PickUpMultiplier, _pickUpMultiplier);
    }

    public void DowngradeLevel(PickerModuleConfig.EnumCharacteristics characteristics, UpgradeInfo upgradeInfo)
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
