using ScriptableObject.ModulesConfig.SupportModules;
using UI.UIPanel.PlayerUpgrade;
using UnityEngine;
using Zenject;

public class RotationModuleUpdater : MonoBehaviour
{
    [Header("UpgradeInfo")]
    [SerializeField] UpgradeInfo _rotationSpeed;

    [Header("Description text")]
    [SerializeField] TextContainer _rotationSpeedDescription;

    RotationModuleConfig _moduleConfig;
    PlayerUpgradePanel _playerUpgradePanel;
    ILanguageContainer _languageContainer;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(ILanguageContainer languageContainer)
    {
        _languageContainer = languageContainer;
    }

    public void Initialize(RotationModuleConfig moduleConfig, PlayerUpgradePanel playerUpgradePanel)
    {
        _moduleConfig = moduleConfig;
        _playerUpgradePanel = playerUpgradePanel;

        InitializeUpgradeInfo(_rotationSpeed,
                              RotationModuleConfig.EnumCharacteristics.RotationSpeed,
                              UpgradeStabilizationSpeed,
                              DowngradeStabilizationSpeed,
                              DetailedInformationEngineMaxForce
                              );
    }

    private void InitializeUpgradeInfo(UpgradeInfo upgradeInfo,
                                      RotationModuleConfig.EnumCharacteristics characteristics,
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

    private string GetCost(RotationModuleConfig.EnumCharacteristics characteristics, int level)
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
        UpgradeLevel(RotationModuleConfig.EnumCharacteristics.RotationSpeed, _rotationSpeed);
    }

    public void UpgradeLevel(RotationModuleConfig.EnumCharacteristics characteristics, UpgradeInfo upgradeInfo)
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
        _playerUpgradePanel.Player.RotationModule.UpdateCharacteristics(_moduleConfig);
    }

    //_________________________________ Downgrade Button _________________________________
    public void DowngradeStabilizationSpeed()
    {
        DowngradeLevel(RotationModuleConfig.EnumCharacteristics.RotationSpeed, _rotationSpeed);
    }

    public void DowngradeLevel(RotationModuleConfig.EnumCharacteristics characteristics, UpgradeInfo upgradeInfo)
    {
        var currentLevel = _moduleConfig.GetLevel(characteristics);
        var nextLevel = currentLevel - 1;

        if (!_playerUpgradePanel.DowngradeLevelCheck(currentLevel))
        {
            return;
        }

        upgradeInfo.UpdateCurrentLevel(nextLevel, GetCost(characteristics, nextLevel));
        _moduleConfig.SetLevel(characteristics, nextLevel);
        _playerUpgradePanel.Player.RotationModule.UpdateCharacteristics(_moduleConfig);
    }

    //_________________________________ Detailed Information Button _________________________________
    private void DetailedInformationEngineMaxForce(UpgradeInfo upgradeInfo)
    {
        _playerUpgradePanel.VisualController.ViewDetailedInformation(upgradeInfo,
                                                                     _rotationSpeedDescription.GetText(_languageContainer.Language));

        _playerUpgradePanel.VisualController.TestModule<RotationModule>();
    }

}
