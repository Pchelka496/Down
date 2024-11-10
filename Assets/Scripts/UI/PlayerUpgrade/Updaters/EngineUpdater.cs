using UnityEngine;

public class EngineUpdater : MonoBehaviour
{
    [SerializeField] UpgradeInfo _engineMaxForce;
    [SerializeField] UpgradeInfo _applyForceRate;
    [SerializeField] UpgradeInfo _engineInterpolateForceValue;
    [SerializeField] UpgradeInfo _engineRotationSpeed;
    [SerializeField] UpgradeInfo _boostPower;

    EngineModuleConfig _moduleConfig;
    PlayerUpgradePanel _playerUpgradePanel;

    public void Initialize(EngineModuleConfig moduleConfig, PlayerUpgradePanel playerUpgradePanel)
    {
        _moduleConfig = moduleConfig;
        _playerUpgradePanel = playerUpgradePanel;

        InitializeUpgradeInfo(_engineMaxForce, EngineModuleConfig.EnumCharacteristics.EngineMaxForce, UpgradeEngineMaxForce, DowngradeEngineMaxForce);
        InitializeUpgradeInfo(_applyForceRate, EngineModuleConfig.EnumCharacteristics.ApplyForceRate, UpgradeEngineApplyForceRate, DowngradeEngineApplyForceRate);
        InitializeUpgradeInfo(_boostPower, EngineModuleConfig.EnumCharacteristics.BoostPower, UpgradeEngineBoostPower, DowngradeEngineBoostPower);
        InitializeUpgradeInfo(_engineInterpolateForceValue, EngineModuleConfig.EnumCharacteristics.EngineForceIncreaseDuration, UpgradeEngineInterpolateForceValue, DowngradeEngineInterpolateForceValue);
        InitializeUpgradeInfo(_engineRotationSpeed, EngineModuleConfig.EnumCharacteristics.EngineRotationSpeed, UpgradeEngineRotationSpeed, DowngradeEngineRotationSpeed);
    }

    private void InitializeUpgradeInfo(UpgradeInfo upgradeInfo, EngineModuleConfig.EnumCharacteristics characteristics, System.Action upgradeAction, System.Action downgradeAction)
    {
        upgradeInfo.Initialize(_moduleConfig.GetLevel(characteristics),
                                   _moduleConfig.GetMaxLevel(characteristics),
                                   GetCost(characteristics,
                                   _moduleConfig.GetLevel(characteristics) + 1),
                                   upgradeAction,
                                   downgradeAction
                                   );
    }

    private string GetCost(EngineModuleConfig.EnumCharacteristics characteristics, int level)
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
    public void UpgradeEngineMaxForce()
    {
        UpgradeLevel(EngineModuleConfig.EnumCharacteristics.EngineMaxForce, _engineMaxForce);
    }

    public void UpgradeEngineApplyForceRate()
    {
        UpgradeLevel(EngineModuleConfig.EnumCharacteristics.ApplyForceRate, _applyForceRate);
    }

    public void UpgradeEngineBoostPower()
    {
        UpgradeLevel(EngineModuleConfig.EnumCharacteristics.BoostPower, _boostPower);
    }

    public void UpgradeEngineInterpolateForceValue()
    {
        UpgradeLevel(EngineModuleConfig.EnumCharacteristics.EngineForceIncreaseDuration, _engineInterpolateForceValue);
    }

    public void UpgradeEngineRotationSpeed()
    {
        UpgradeLevel(EngineModuleConfig.EnumCharacteristics.EngineRotationSpeed, _engineRotationSpeed);
    }

    public void UpgradeLevel(EngineModuleConfig.EnumCharacteristics characteristics, UpgradeInfo upgradeInfo)
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
    public void DowngradeEngineMaxForce()
    {
        DowngradeLevel(EngineModuleConfig.EnumCharacteristics.EngineMaxForce, _engineMaxForce);
    }

    public void DowngradeEngineApplyForceRate()
    {
        DowngradeLevel(EngineModuleConfig.EnumCharacteristics.ApplyForceRate, _applyForceRate);
    }

    public void DowngradeEngineBoostPower()
    {
        DowngradeLevel(EngineModuleConfig.EnumCharacteristics.BoostPower, _boostPower);
    }

    public void DowngradeEngineInterpolateForceValue()
    {
        DowngradeLevel(EngineModuleConfig.EnumCharacteristics.EngineForceIncreaseDuration, _engineInterpolateForceValue);
    }

    public void DowngradeEngineRotationSpeed()
    {
        DowngradeLevel(EngineModuleConfig.EnumCharacteristics.EngineRotationSpeed, _engineRotationSpeed);
    }

    public void DowngradeLevel(EngineModuleConfig.EnumCharacteristics characteristics, UpgradeInfo upgradeInfo)
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
