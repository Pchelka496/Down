using Creatures.Player;
using ScriptableObject.ModulesConfig;
using UI.UIPanel.PlayerUpgrade;
using UnityEngine;
using Zenject;

public class HealthModuleUpdater : MonoBehaviour
{
    [Header("UpgradeInfo")]
    [SerializeField] UpgradeInfo _maxHealth;
    [SerializeField] UpgradeInfo _repairKitNumberForRepair;

    [Header("Description text")]
    [SerializeField] TextContainer _maxHealthDescription;
    [SerializeField] TextContainer _repairKitNumberForRepairDescription;

    HealthModuleConfig _moduleConfig;
    PlayerUpgradePanel _playerUpgradePanel;
    ILanguageContainer _languageContainer;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(ILanguageContainer languageContainer)
    {
        _languageContainer = languageContainer;
    }

    public void Initialize(HealthModuleConfig moduleConfig, PlayerUpgradePanel playerUpgradePanel)
    {
        _moduleConfig = moduleConfig;
        _playerUpgradePanel = playerUpgradePanel;

        InitializeUpgradeInfo(_maxHealth,
                              HealthModuleConfig.EnumCharacteristics.MaxHealth,
                              UpgradeMaxHealthLevel,
                              DowngradeMaxHealthLevel,
                              DetailedInformationMaxHealthDescription
                              );

        InitializeUpgradeInfo(_repairKitNumberForRepair,
                              HealthModuleConfig.EnumCharacteristics.RepairKitNumberForRepair,
                              UpgradeNumberOfPartsRequiredForRepairLevel,
                              DowngradeNumberOfPartsRequiredForRepairLevel,
                              DetailedInformationRepairKitNumberForRepairDescription
                              );
    }

    private void InitializeUpgradeInfo(UpgradeInfo upgradeInfo,
                                       HealthModuleConfig.EnumCharacteristics characteristics,
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
        UpgradeLevel(HealthModuleConfig.EnumCharacteristics.MaxHealth, _maxHealth);
        _playerUpgradePanel.VisualController.TestModule<HealthModule>();
    }

    public void UpgradeNumberOfPartsRequiredForRepairLevel()
    {
        UpgradeLevel(HealthModuleConfig.EnumCharacteristics.RepairKitNumberForRepair, _repairKitNumberForRepair);
        _playerUpgradePanel.VisualController.TestModule<HealthModule>();
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
        DowngradeLevel(HealthModuleConfig.EnumCharacteristics.MaxHealth, _maxHealth);
        _playerUpgradePanel.VisualController.TestModule<HealthModule>();
    }

    public void DowngradeNumberOfPartsRequiredForRepairLevel()
    {
        DowngradeLevel(HealthModuleConfig.EnumCharacteristics.RepairKitNumberForRepair, _repairKitNumberForRepair);
        _playerUpgradePanel.VisualController.TestModule<HealthModule>();
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
        _playerUpgradePanel.Player.HealthModule.UpdateCharacteristics(_moduleConfig);
    }

    //_________________________________ Detailed Information Button _________________________________
    private void DetailedInformationMaxHealthDescription(UpgradeInfo upgradeInfo)
    {
        _playerUpgradePanel.VisualController.ViewDetailedInformation(upgradeInfo,
                                                                     _maxHealthDescription.GetText(_languageContainer.Language));

        _playerUpgradePanel.VisualController.TestModule<HealthModule>();
    }

    private void DetailedInformationRepairKitNumberForRepairDescription(UpgradeInfo upgradeInfo)
    {
        _playerUpgradePanel.VisualController.ViewDetailedInformation(upgradeInfo,
                                                                     _repairKitNumberForRepairDescription.GetText(_languageContainer.Language));

        _playerUpgradePanel.VisualController.TestModule<HealthModule>();
    }

}
