using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpgradeInfo : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _costText;
    [SerializeField] LevelProgressDisplay _levelProgressDisplay;
    BaseModuleConfig _moduleConfig;

    public BaseModuleConfig ModuleConfig { get => _moduleConfig; }

    public void Initialize(BaseModuleConfig moduleConfig)
    {
        var maxLevel = moduleConfig.GetMaxLevel();
        var currentLevel = moduleConfig.GetCurrentLevel();

        _levelProgressDisplay.Initialize(maxLevel, currentLevel);
        _moduleConfig = moduleConfig;
        SetUpgradeCost(currentLevel);
    }

    public void UpdateCurrentLevel(int currentLevel)
    {
        _levelProgressDisplay.UpdateLevelProgress(currentLevel);
        SetUpgradeCost(currentLevel);
    }

    private void SetUpgradeCost(int currentLevel)
    {
        if (currentLevel >= _moduleConfig.MaxLevel())
        {
            _costText.text = "Максимальный уровень";
            return;
        }

        _costText.text = _moduleConfig.GetCurrentLevelCost().ToString();

    }

}
