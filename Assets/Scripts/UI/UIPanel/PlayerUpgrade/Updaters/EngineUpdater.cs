using Creatures.Player.FlightModule.EngineModule;
using Creatures.Player.PlayerModule.CoreModules.EngineModule;
using ScriptableObject.ModulesConfig.FlightModule;
using UI.UIPanel.PlayerUpgrade;
using UnityEngine;
using Zenject;

public class EngineUpdater : MonoBehaviour
{
    [Header("UpgradeInfo")]
    [SerializeField] UpgradeInfo _engineMaxForce;
    [SerializeField] UpgradeInfo _applyForceRate;
    [SerializeField] UpgradeInfo _engineInterpolateForceValue;
    [SerializeField] UpgradeInfo _boostPower;
    [SerializeField] UpgradeInfo _boosterChargeCount;
    [SerializeField] UpgradeInfo _boosterChargeCooldown;

    [Header("Description text")]
    [SerializeField] TextContainer _engineMaxForceDescription;
    [SerializeField] TextContainer _applyForceRateDescription;
    [SerializeField] TextContainer _boostPowerDescription;
    [SerializeField] TextContainer _engineInterpolateForceValueDescription;
    [SerializeField] TextContainer _boosterChargeCountDescription;
    [SerializeField] TextContainer _boosterChargeCooldownDescription;

    EngineModuleConfig _moduleConfig;
    PlayerUpgradePanel _playerUpgradePanel;
    ILanguageContainer _languageContainer;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(ILanguageContainer languageContainer)
    {
        _languageContainer = languageContainer;
    }

    public void Initialize(EngineModuleConfig moduleConfig, PlayerUpgradePanel playerUpgradePanel)
    {
        _moduleConfig = moduleConfig;
        _playerUpgradePanel = playerUpgradePanel;

        InitializeUpgradeInfo(_engineMaxForce,
                              EngineModuleConfig.EnumCharacteristics.EngineMaxForce,
                              UpgradeEngineMaxForce,
                              DowngradeEngineMaxForce,
                              DetailedInformationEngineMaxForce
                              );

        InitializeUpgradeInfo(_applyForceRate,
                              EngineModuleConfig.EnumCharacteristics.ApplyForceRate,
                              UpgradeEngineApplyForceRate,
                              DowngradeEngineApplyForceRate,
                              DetailedInformationEngineApplyForceRate
                              );

        InitializeUpgradeInfo(_boostPower,
                              EngineModuleConfig.EnumCharacteristics.BoostPower,
                              UpgradeEngineBoostPower,
                              DowngradeEngineBoostPower,
                              DetailedInformationEngineBoostPower
                              );

        InitializeUpgradeInfo(_engineInterpolateForceValue,
                              EngineModuleConfig.EnumCharacteristics.EngineForceIncreaseDuration,
                              UpgradeEngineInterpolateForceValue,
                              DowngradeEngineInterpolateForceValue,
                              DetailedInformationEngineInterpolateForceValue
                              );

        InitializeUpgradeInfo(_boosterChargeCount,
                             EngineModuleConfig.EnumCharacteristics.BoosterChargeCount,
                             UpgradeBoosterChargeCount,
                             DowngradeBoosterChargeCount,
                             DetailedInformationBoosterChargeCount
                             );

        InitializeUpgradeInfo(_boosterChargeCooldown,
                             EngineModuleConfig.EnumCharacteristics.BoosterChargeCooldown,
                             UpgradeBoosterChargeCooldown,
                             DowngradeBoosterChargeCooldown,
                             DetailedInformationBoosterChargeCooldown
                             );
    }

    private void InitializeUpgradeInfo(UpgradeInfo upgradeInfo,
                                       EngineModuleConfig.EnumCharacteristics characteristics,
                                       System.Action upgradeAction,
                                       System.Action downgradeAction,
                                       System.Action<UpgradeInfo> detailedInformationAction
                                       )
    {
        upgradeInfo.Initialize(_moduleConfig.GetLevel(characteristics),
                               _moduleConfig.GetMaxLevel(characteristics),
                               GetCost(characteristics,
                               _moduleConfig.GetLevel(characteristics) + 1),
                               upgradeAction,
                               downgradeAction,
                               detailedInformationAction
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
    private void UpgradeEngineMaxForce() => UpgradeLevel(EngineModuleConfig.EnumCharacteristics.EngineMaxForce, _engineMaxForce);
    private void UpgradeEngineApplyForceRate() => UpgradeLevel(EngineModuleConfig.EnumCharacteristics.ApplyForceRate, _applyForceRate);
    private void UpgradeEngineBoostPower() => UpgradeLevel(EngineModuleConfig.EnumCharacteristics.BoostPower, _boostPower);
    private void UpgradeEngineInterpolateForceValue() => UpgradeLevel(EngineModuleConfig.EnumCharacteristics.EngineForceIncreaseDuration, _engineInterpolateForceValue);
    private void UpgradeBoosterChargeCount() => UpgradeLevel(EngineModuleConfig.EnumCharacteristics.BoosterChargeCount, _boosterChargeCount);
    private void UpgradeBoosterChargeCooldown() => UpgradeLevel(EngineModuleConfig.EnumCharacteristics.BoosterChargeCooldown, _boosterChargeCooldown);

    private void UpgradeLevel(EngineModuleConfig.EnumCharacteristics characteristics, UpgradeInfo upgradeInfo)
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
        _playerUpgradePanel.Player.EngineModule.UpdateCharacteristics(_moduleConfig);
    }

    //_________________________________ Downgrade Button _________________________________
    private void DowngradeEngineMaxForce() => DowngradeLevel(EngineModuleConfig.EnumCharacteristics.EngineMaxForce, _engineMaxForce);
    private void DowngradeEngineApplyForceRate() => DowngradeLevel(EngineModuleConfig.EnumCharacteristics.ApplyForceRate, _applyForceRate);
    private void DowngradeEngineBoostPower() => DowngradeLevel(EngineModuleConfig.EnumCharacteristics.BoostPower, _boostPower);
    private void DowngradeEngineInterpolateForceValue() => DowngradeLevel(EngineModuleConfig.EnumCharacteristics.EngineForceIncreaseDuration, _engineInterpolateForceValue);
    private void DowngradeBoosterChargeCount() => DowngradeLevel(EngineModuleConfig.EnumCharacteristics.BoosterChargeCount, _boosterChargeCount);
    private void DowngradeBoosterChargeCooldown() => DowngradeLevel(EngineModuleConfig.EnumCharacteristics.BoosterChargeCooldown, _boosterChargeCooldown);

    private void DowngradeLevel(EngineModuleConfig.EnumCharacteristics characteristics, UpgradeInfo upgradeInfo)
    {
        var currentLevel = _moduleConfig.GetLevel(characteristics);
        var nextLevel = currentLevel - 1;

        if (!_playerUpgradePanel.DowngradeLevelCheck(currentLevel))
        {
            return;
        }

        upgradeInfo.UpdateCurrentLevel(nextLevel, GetCost(characteristics, nextLevel));
        _moduleConfig.SetLevel(characteristics, nextLevel);
        _playerUpgradePanel.Player.EngineModule.UpdateCharacteristics(_moduleConfig);
    }

    //_________________________________ Detailed Information Button _________________________________

    private void DetailedInformationEngineMaxForce(UpgradeInfo upgradeInfo)
    {
        _playerUpgradePanel.VisualController.ViewDetailedInformation(upgradeInfo,
                                                                     _engineMaxForceDescription.GetText(_languageContainer.Language));
        EngineTest();
    }

    private void DetailedInformationEngineApplyForceRate(UpgradeInfo upgradeInfo)
    {
        _playerUpgradePanel.VisualController.ViewDetailedInformation(upgradeInfo,
                                                                     _applyForceRateDescription.GetText(_languageContainer.Language));
        EngineTest();
    }

    private void DetailedInformationEngineBoostPower(UpgradeInfo upgradeInfo)
    {
        _playerUpgradePanel.VisualController.ViewDetailedInformation(upgradeInfo,
                                                                     _boostPowerDescription.GetText(_languageContainer.Language));
        EngineTest();
    }

    private void DetailedInformationEngineInterpolateForceValue(UpgradeInfo upgradeInfo)
    {
        _playerUpgradePanel.VisualController.ViewDetailedInformation(upgradeInfo,
                                                                     _engineInterpolateForceValueDescription.GetText(_languageContainer.Language));
        EngineTest();
    }

    private void DetailedInformationBoosterChargeCount(UpgradeInfo upgradeInfo)
    {
        _playerUpgradePanel.VisualController.ViewDetailedInformation(upgradeInfo,
                                                                     _boosterChargeCountDescription.GetText(_languageContainer.Language));
        EngineTest();
    }

    private void DetailedInformationBoosterChargeCooldown(UpgradeInfo upgradeInfo)
    {
        _playerUpgradePanel.VisualController.ViewDetailedInformation(upgradeInfo,
                                                                     _boosterChargeCooldownDescription.GetText(_languageContainer.Language));
        EngineTest();
    }

    private void EngineTest()
    {
        _playerUpgradePanel.VisualController.TestModule<EngineModule>();
        _playerUpgradePanel.VisualController.TestModule<RotationModule>();
    }

}
