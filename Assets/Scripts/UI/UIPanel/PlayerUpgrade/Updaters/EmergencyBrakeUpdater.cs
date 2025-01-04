using ScriptableObject.ModulesConfig.SupportModules;
using UI.UIPanel.PlayerUpgrade;
using UnityEngine;
using Zenject;

public class EmergencyBrakeUpdater : MonoBehaviour
{
    [Header("UpgradeInfo")]
    [SerializeField] UpgradeInfo _chargeQuantity;
    [SerializeField] UpgradeInfo _chargeCooldown;
    [SerializeField] UpgradeInfo _sensingDistance;

    [Header("Description text")]
    [SerializeField] TextContainer _chargeQuantityDescription;
    [SerializeField] TextContainer _chargeCooldownDescription;
    [SerializeField] TextContainer _sensingDistanceDescription;

    [Header("Visualize sensing distance")]
    [SerializeField] Color _firstCircleColor;
    [SerializeField] float _firstThicknessCircle;

    [SerializeField] Color _secondCircleColor;
    [SerializeField] float _secondThicknessCircle;

    EmergencyBrakeModuleConfig _moduleConfig;
    PlayerUpgradePanel _playerUpgradePanel;
    ILanguageContainer _languageContainer;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(ILanguageContainer languageContainer)
    {
        _languageContainer = languageContainer;
    }

    public void Initialize(EmergencyBrakeModuleConfig moduleConfig, PlayerUpgradePanel playerUpgradePanel)
    {
        _moduleConfig = moduleConfig;
        _playerUpgradePanel = playerUpgradePanel;

        InitializeUpgradeInfo(_chargeQuantity,
                              EmergencyBrakeModuleConfig.EnumCharacteristics.MaxCharges,
                              UpgradeChargeQuantity,
                              DowngradeChargeQuantity,
                              DetailedInformationChargeQuantityDescription
                              );

        InitializeUpgradeInfo(_chargeCooldown,
                              EmergencyBrakeModuleConfig.EnumCharacteristics.ChargeCooldown,
                              UpgradeChargeCooldown,
                              DowngradeChargeCooldown,
                              DetailedInformationChargeCooldownDescription
                              );

        InitializeUpgradeInfo(_sensingDistance,
                              EmergencyBrakeModuleConfig.EnumCharacteristics.SensingDistance,
                              UpgradeSensingDistance,
                              DowngradeSensingDistance,
                              DetailedInformationSensingDistanceDescription
                              );
    }

    private void InitializeUpgradeInfo(UpgradeInfo upgradeInfo,
                                       EmergencyBrakeModuleConfig.EnumCharacteristics characteristics,
                                       System.Action upgradeAction,
                                       System.Action downgradeAction,
                                       System.Action<UpgradeInfo> detailedInformationAction
                                       )
    {
        upgradeInfo.Initialize(_moduleConfig.GetLevel(characteristics),
                               _moduleConfig.GetMaxLevel(characteristics),
                               GetCost(characteristics,
                               _moduleConfig.GetLevel(characteristics)),
                               upgradeAction,
                               downgradeAction,
                               detailedInformationAction
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
        UpgradeLevel(EmergencyBrakeModuleConfig.EnumCharacteristics.MaxCharges, _chargeQuantity);
    }

    public void UpgradeChargeCooldown()
    {
        UpgradeLevel(EmergencyBrakeModuleConfig.EnumCharacteristics.ChargeCooldown, _chargeCooldown);
    }

    public void UpgradeSensingDistance()
    {
        UpgradeLevel(EmergencyBrakeModuleConfig.EnumCharacteristics.SensingDistance, _sensingDistance);
        _playerUpgradePanel.VisualController.UpdateDetailedInformation(GetFirstCircleData(), GetSecondCircleData());
    }

    private RadiusDisplay.RadiusDisplayData GetSecondCircleData()
    {
        var playerPosition = _playerUpgradePanel.PlayerViewUpgradePosition;

        var distance = _moduleConfig.SensingDistance;

        return new RadiusDisplay.RadiusDisplayData
        {
            GlobalPosition = new(playerPosition.x, playerPosition.y + distance),
            Radius = EmergencyBrakeModule.COLLIDER_RADIUS,
            Color = _secondCircleColor,
            Thickness = _secondThicknessCircle
        };
    }

    private RadiusDisplay.RadiusDisplayData GetFirstCircleData()
    {
        var playerPosition = _playerUpgradePanel.PlayerViewUpgradePosition;

        var distance = _moduleConfig.SensingDistance;

        return new RadiusDisplay.RadiusDisplayData
        {
            GlobalPosition = new(playerPosition.x, playerPosition.y + distance),
            Radius = EmergencyBrakeModule.COLLIDER_RADIUS,
            Color = _firstCircleColor,
            Thickness = _firstThicknessCircle
        };
    }

    public void UpgradeLevel(EmergencyBrakeModuleConfig.EnumCharacteristics characteristics, UpgradeInfo upgradeInfo)
    {
        var currentLevel = _moduleConfig.GetLevel(characteristics);

        if (!_playerUpgradePanel.UpgradeLevelCheck(_moduleConfig.GetLevelCost(characteristics, currentLevel),
                                                  currentLevel,
                                                  _moduleConfig.GetMaxLevel(characteristics)
                                                  ))
        {
            return;
        }
        var nextLevel = currentLevel + 1;

        upgradeInfo.UpdateCurrentLevel(nextLevel, GetCost(characteristics, nextLevel));
        _moduleConfig.SetLevel(characteristics, nextLevel);

        if (_playerUpgradePanel.Player.GetModule<EmergencyBrakeModule>(out var module))
        {
            module.UpdateCharacteristics(_moduleConfig);
        }
    }

    //_________________________________ Downgrade Button _________________________________
    public void DowngradeChargeQuantity()
    {
        DowngradeLevel(EmergencyBrakeModuleConfig.EnumCharacteristics.MaxCharges, _chargeQuantity);
    }

    public void DowngradeChargeCooldown()
    {
        DowngradeLevel(EmergencyBrakeModuleConfig.EnumCharacteristics.ChargeCooldown, _chargeCooldown);
    }

    public void DowngradeSensingDistance()
    {
        DowngradeLevel(EmergencyBrakeModuleConfig.EnumCharacteristics.SensingDistance, _sensingDistance);
        _playerUpgradePanel.VisualController.UpdateDetailedInformation(GetFirstCircleData(), GetSecondCircleData());
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

        if (_playerUpgradePanel.Player.GetModule<EmergencyBrakeModule>(out var module))
        {
            module.UpdateCharacteristics(_moduleConfig);
        }
    }

    //_________________________________ Detailed Information Button _________________________________
    private void DetailedInformationChargeCooldownDescription(UpgradeInfo upgradeInfo)
    {
        _playerUpgradePanel.VisualController.ViewDetailedInformation(upgradeInfo,
                                                                     _chargeCooldownDescription.GetText(_languageContainer.Language));
    }

    private void DetailedInformationChargeQuantityDescription(UpgradeInfo upgradeInfo)
    {
        _playerUpgradePanel.VisualController.ViewDetailedInformation(upgradeInfo,
                                                                     _chargeQuantityDescription.GetText(_languageContainer.Language));
    }

    private void DetailedInformationSensingDistanceDescription(UpgradeInfo upgradeInfo)
    {
        SensingDistanceVisualize(upgradeInfo);
    }

    private void SensingDistanceVisualize(UpgradeInfo upgradeInfo)
    {
        _playerUpgradePanel.VisualController.ViewDetailedInformation(upgradeInfo,
                                                                     GetFirstCircleData(),
                                                                     GetSecondCircleData(),
                                                                     _sensingDistanceDescription.GetText(_languageContainer.Language));
    }
}
