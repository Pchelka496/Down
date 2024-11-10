using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeInfo : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _costText;
    [SerializeField] LevelProgressDisplay _levelProgressDisplay;

    [SerializeField] Button _upgradeButton;
    [SerializeField] Button _downgradeButton;

    public async void Initialize(int currentLevel, int maxLevel, string currentLevelConst, System.Action upgradeAction, System.Action downgradeAction)
    {
        await _levelProgressDisplay.Initialize(currentLevel: currentLevel, maxLevel: maxLevel);

        if (_upgradeButton != null)
        {
            _upgradeButton.onClick.AddListener(() => upgradeAction?.Invoke());
        }
        if (_downgradeButton != null)
        {
            _downgradeButton.onClick.AddListener(() => downgradeAction?.Invoke());
        }

        UpdateCurrentLevel(currentLevel, currentLevelConst);
    }

    public void UpdateCurrentLevel(int currentLevel, string costText)
    {
        _levelProgressDisplay.UpdateLevelProgress(currentLevel);
        SetUpgradeCost(costText);
    }

    private void SetUpgradeCost(string costText)
    {
        _costText.text = costText;
    }

}
