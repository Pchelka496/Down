using UnityEngine;

public class AirBrakeUpdater : MonoBehaviour
{
    [SerializeField] UpgradeInfo _airBrakeDrag;
    [SerializeField] UpgradeInfo _airBrakeReleaseRate;

    AirBrakeModuleConfig _moduleConfig;
    PlayerUpgradePanel _playerUpgradePanel;

    public void Initialize(AirBrakeModuleConfig moduleConfig, PlayerUpgradePanel playerUpgradePanel)
    {
        _moduleConfig = moduleConfig;
        _playerUpgradePanel = playerUpgradePanel;

        InitializeUpgradeInfo(_airBrakeDrag, AirBrakeModuleConfig.EnumCharacteristics.AirBrakeDrag, UpgradeAirBrakeDrag, DowngradeAirBrakeDrag);
        InitializeUpgradeInfo(_airBrakeReleaseRate, AirBrakeModuleConfig.EnumCharacteristics.AirBrakeReleaseRate, UpgradeAirBrakeReleaseRate, DowngradeAirBrakeReleaseRate);
    }

    private void InitializeUpgradeInfo(UpgradeInfo upgradeInfo, AirBrakeModuleConfig.EnumCharacteristics characteristics, System.Action upgradeAction, System.Action downgradeAction)
    {
        upgradeInfo.Initialize(_moduleConfig.GetLevel(characteristics),
                                   _moduleConfig.GetMaxLevel(characteristics),
                                   GetCost(characteristics,
                                   _moduleConfig.GetLevel(characteristics) + 1),
                                   upgradeAction,
                                   downgradeAction
                                   );
    }

    private string GetCost(AirBrakeModuleConfig.EnumCharacteristics characteristics, int level)
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
    public void UpgradeAirBrakeDrag()
    {
        UpgradeLevel(AirBrakeModuleConfig.EnumCharacteristics.AirBrakeDrag, _airBrakeDrag);
    }

    public void UpgradeAirBrakeReleaseRate()
    {
        UpgradeLevel(AirBrakeModuleConfig.EnumCharacteristics.AirBrakeReleaseRate, _airBrakeReleaseRate);
    }

    public void UpgradeLevel(AirBrakeModuleConfig.EnumCharacteristics characteristics, UpgradeInfo upgradeInfo)
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
    public void DowngradeAirBrakeDrag()
    {
        DowngradeLevel(AirBrakeModuleConfig.EnumCharacteristics.AirBrakeDrag, _airBrakeDrag);
    }

    public void DowngradeAirBrakeReleaseRate()
    {
        DowngradeLevel(AirBrakeModuleConfig.EnumCharacteristics.AirBrakeReleaseRate, _airBrakeReleaseRate);
    }

    public void DowngradeLevel(AirBrakeModuleConfig.EnumCharacteristics characteristics, UpgradeInfo upgradeInfo)
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
