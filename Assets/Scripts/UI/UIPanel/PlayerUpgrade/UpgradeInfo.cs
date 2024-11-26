using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class UpgradeInfo : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _costText;
    [SerializeField] LevelProgressDisplay _levelProgressDisplay;

    [SerializeField] Button _upgradeButton;
    [SerializeField] Button _downgradeButton;
    [SerializeField] Button _detailedInformation;

    [SerializeField] RectTransform _rectTransform;
    EnumLanguage _language;

    public RectTransform RectTransform { get => _rectTransform; }

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Удалите неиспользуемые закрытые члены", Justification = "<Ожидание>")]
    private void Construct(EnumLanguage language)
    {
        _language = language;
    }

    public async void Initialize(int currentLevel,
                                 int maxLevel,
                                 string currentLevelConst,
                                 System.Action upgradeAction,
                                 System.Action downgradeAction,
                                 System.Action<UpgradeInfo> detailedInformationAction)
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
        if (_detailedInformation)
        {
            _detailedInformation.onClick.AddListener(() => detailedInformationAction?.Invoke(this));
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

    private void Reset()
    {
        _rectTransform = gameObject.GetComponent<RectTransform>();
    }

}
