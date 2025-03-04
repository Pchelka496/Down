using ScriptableObject.ModulesConfig.SupportModules;
using UI.UIPanel.PlayerUpgrade;
using UnityEngine;
using Zenject;

public class AirBrakeUpdater : MonoBehaviour
{
    [Header("UpgradeInfo")]
    [SerializeField] UpgradeInfo _airBrakeDrag;
    [SerializeField] UpgradeInfo _airBrakeReleaseRate;

    [Header("Description text")]
    [SerializeField] TextContainer _airBrakeDragDescription;
    [SerializeField] TextContainer _airBrakeReleaseRateDescription;

    AirBrakeModuleConfig _moduleConfig;
    PlayerUpgradePanel _playerUpgradePanel;

    ILanguageContainer _languageContainer;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(ILanguageContainer languageContainer)
    {
        _languageContainer = languageContainer;
    }

    public void Initialize(AirBrakeModuleConfig moduleConfig, PlayerUpgradePanel playerUpgradePanel)
    {
        _moduleConfig = moduleConfig;
        _playerUpgradePanel = playerUpgradePanel;

        InitializeUpgradeInfo(_airBrakeDrag,
                              AirBrakeModuleConfig.EnumCharacteristics.AirBrakeDrag,
                              UpgradeAirBrakeDrag,
                              DowngradeAirBrakeDrag,
                              DetailedInformationAirBrakeDrag
                              );

        InitializeUpgradeInfo(_airBrakeReleaseRate,
                              AirBrakeModuleConfig.EnumCharacteristics.AirBrakeReleaseRate,
                              UpgradeAirBrakeReleaseRate,
                              DowngradeAirBrakeReleaseRate,
                              DetailedInformationAirBrakeReleaseRateDescription
                              );
    }

    private void InitializeUpgradeInfo(UpgradeInfo upgradeInfo,
                                       AirBrakeModuleConfig.EnumCharacteristics characteristics,
                                       System.Action upgradeAction,
                                       System.Action downgradeAction,
                                       System.Action<UpgradeInfo> detailedInformationAction)
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

        if (_playerUpgradePanel.Player.GetModule<AirBrakeModule>(out var module))
        {
            module.UpdateCharacteristics(_moduleConfig);
        }
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

        if (_playerUpgradePanel.Player.GetModule<AirBrakeModule>(out var module))
        {
            module.UpdateCharacteristics(_moduleConfig);
        }
    }

    //_________________________________ Detailed Information Button _________________________________
    private void DetailedInformationAirBrakeDrag(UpgradeInfo upgradeInfo)
    {
        _playerUpgradePanel.VisualController.ViewDetailedInformation(upgradeInfo,
                                                                     _airBrakeDragDescription.GetText(_languageContainer.Language));

        _playerUpgradePanel.VisualController.TestModule<AirBrakeModule>();
    }

    private void DetailedInformationAirBrakeReleaseRateDescription(UpgradeInfo upgradeInfo)
    {
        _playerUpgradePanel.VisualController.ViewDetailedInformation(upgradeInfo,
                                                                     _airBrakeReleaseRateDescription.GetText(_languageContainer.Language));

        _playerUpgradePanel.VisualController.TestModule<AirBrakeModule>();
    }

}
