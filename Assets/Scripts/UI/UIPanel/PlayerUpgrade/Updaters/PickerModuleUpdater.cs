using Core.Installers;
using ScriptableObject.ModulesConfig.SupportModules;
using UI.UIPanel.PlayerUpgrade;
using UnityEngine;
using Zenject;
using static RadiusDisplay;

public class PickerModuleUpdater : MonoBehaviour
{
    [Header("UpgradeInfo")]
    [SerializeField] UpgradeInfo _pickUpRadius;
    [SerializeField] UpgradeInfo _pickUpMultiplier;

    [Header("Description text")]
    [SerializeField] TextContainer _pickUpRadiusDescription;
    [SerializeField] TextContainer _pickUpMultiplierDescription;

    [SerializeField] RadiusDisplayData _firstRadiusData;
    [SerializeField] RadiusDisplayData _secondRadiusData;

    PickerModuleConfig _moduleConfig;
    PlayerUpgradePanel _playerUpgradePanel;
    ILanguageContainer _languageContainer;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(ILanguageContainer languageContainer)
    {
        _languageContainer = languageContainer;
    }

    public void Initialize(PickerModuleConfig moduleConfig, PlayerUpgradePanel playerUpgradePanel)
    {
        _moduleConfig = moduleConfig;
        _playerUpgradePanel = playerUpgradePanel;

        InitializeUpgradeInfo(_pickUpRadius,
                              PickerModuleConfig.EnumCharacteristics.PickUpRadius,
                              UpgradePickUpRadius,
                              DowngradePickUpRadius,
                              DetailedInformationPickUpRadiusDescriptionDescription
                              );

        InitializeUpgradeInfo(_pickUpMultiplier,
                              PickerModuleConfig.EnumCharacteristics.PickUpRewardMultiplier,
                              UpgradePickUpMultiplier,
                              DowngradePickUpMultiplier,
                              DetailedInformationPickUpMultiplierDescriptionDescription
                              );
    }

    private void InitializeUpgradeInfo(UpgradeInfo upgradeInfo,
                                       PickerModuleConfig.EnumCharacteristics characteristics,
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

        RadiusDisplayDataSetting();
        _playerUpgradePanel.VisualController.UpdateDetailedInformation(_firstRadiusData, _secondRadiusData);
    }

    public void UpgradePickUpMultiplier()
    {
        UpgradeLevel(PickerModuleConfig.EnumCharacteristics.PickUpRewardMultiplier, _pickUpMultiplier);
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
        _playerUpgradePanel.Player.PickerModule.UpdateCharacteristics(_moduleConfig, GameplaySceneInstaller.DiContainer.Resolve<RewardCounter>());
    }

    //_________________________________ Downgrade Button _________________________________
    public void DowngradePickUpRadius()
    {
        DowngradeLevel(PickerModuleConfig.EnumCharacteristics.PickUpRadius, _pickUpRadius);

        RadiusDisplayDataSetting();
        _playerUpgradePanel.VisualController.UpdateDetailedInformation(_firstRadiusData, _secondRadiusData);
    }

    public void DowngradePickUpMultiplier()
    {
        DowngradeLevel(PickerModuleConfig.EnumCharacteristics.PickUpRewardMultiplier, _pickUpMultiplier);
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
        _playerUpgradePanel.Player.PickerModule.UpdateCharacteristics(_moduleConfig, GameplaySceneInstaller.DiContainer.Resolve<RewardCounter>());
    }

    //_________________________________ Detailed Information Button _________________________________
    private void DetailedInformationPickUpRadiusDescriptionDescription(UpgradeInfo upgradeInfo)
    {
        RadiusDisplayDataSetting();

        _playerUpgradePanel.VisualController.ViewDetailedInformation(upgradeInfo,
                                                                     _firstRadiusData,
                                                                     _secondRadiusData,
                                                                     _pickUpRadiusDescription.GetText(_languageContainer.Language)
                                                                     );
    }

    private void RadiusDisplayDataSetting()
    {
        var characteristics = PickerModuleConfig.EnumCharacteristics.PickUpRadius;

        var currentLevel = _moduleConfig.GetLevel(characteristics);

        _firstRadiusData.Radius = _moduleConfig.GetCharacteristicForLevel(currentLevel, characteristics);
        _secondRadiusData.Radius = _moduleConfig.GetCharacteristicForLevel(currentLevel, characteristics);
    }

    private void DetailedInformationPickUpMultiplierDescriptionDescription(UpgradeInfo upgradeInfo)
    {
        _playerUpgradePanel.VisualController.ViewDetailedInformation(upgradeInfo,
                                                                     _pickUpMultiplierDescription.GetText(_languageContainer.Language));
    }
}
