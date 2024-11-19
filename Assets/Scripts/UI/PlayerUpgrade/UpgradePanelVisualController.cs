using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using Zenject;
using static RadiusDisplay;

public class UpgradePanelVisualController : MonoBehaviour
{
    [SerializeField] ScrollRectController _scrollRectController;
    [SerializeField] RectTransform _playerViewUpgradePosition;

    [Header("DisplayInfo")]
    [SerializeField] RadiusDisplay _radiusDisplay;
    [SerializeField] TextDisplay _textDisplay;
    [SerializeField] ModulesTester _modulesTester;

    [Header("Upgrade Info Management")]
    [SerializeField] UpgradeInfo[] _allUpgradeInfo;

    UpgradeInfo _triggeringUpgradeInfo;
    ViewMode _currentViewMode;

    public ViewMode CurrentViewMode => _currentViewMode;

    private void Start()
    {
        _radiusDisplay.Initialize(_playerViewUpgradePosition);
    }

    private void OnEnable()
    {
        SwitchToViewMode(ViewMode.Basic);
    }

    public void ViewDetailedInformation(UpgradeInfo upgradeInfo, string detailedInformation)
    {
        ViewDetailedInformation(upgradeInfo);
        UpdateDetailedInformation(detailedInformation);
    }

    public void ViewDetailedInformation(UpgradeInfo upgradeInfo, RadiusDisplayData currentRadius, RadiusDisplayData targetRadius)
    {
        ViewDetailedInformation(upgradeInfo);
        UpdateDetailedInformation(currentRadius, targetRadius);
    }

    public void ViewDetailedInformation(UpgradeInfo upgradeInfo,
                                        RadiusDisplayData currentRadius,
                                        RadiusDisplayData targetRadius,
                                        string detailedInformation
                                        )
    {
        ViewDetailedInformation(upgradeInfo);
        UpdateDetailedInformation(detailedInformation);
        UpdateDetailedInformation(currentRadius, targetRadius);
    }

    public void UpdateDetailedInformation(string detailedInformation)
    {
        switch (_currentViewMode)
        {
            case ViewMode.Detailed:
                {
                    _textDisplay.DisplayText(detailedInformation).Forget();
                    break;
                }
        }
    }

    public void UpdateDetailedInformation(RadiusDisplayData currentRadius, RadiusDisplayData targetRadius)
    {
        switch (_currentViewMode)
        {
            case ViewMode.Detailed:
                {
                    _radiusDisplay.DisplayRadius(currentRadius, targetRadius);
                    break;
                }
        }
    }

    public void TestModule<T>() where T : BaseModule
    {
        switch (_currentViewMode)
        {
            case ViewMode.Detailed:
                {
                    _modulesTester.ModuleTest<T>().Forget();
                    break;
                }
        }
    }

    private void ViewDetailedInformation(UpgradeInfo triggeringUpgradeInfo)
    {
        switch (_currentViewMode)
        {
            case ViewMode.Detailed:
                {
                    SwitchToViewMode(ViewMode.Basic);
                    break;
                }
            default:
                {
                    _triggeringUpgradeInfo = triggeringUpgradeInfo;
                    SwitchToViewMode(ViewMode.Detailed);
                    break;
                }
        }
    }

    public void EndViewDetailedInformation()
    {
        SwitchToViewMode(ViewMode.Basic);
    }

    private void SwitchToViewMode(ViewMode mode)
    {
        _currentViewMode = mode;

        switch (mode)
        {
            case ViewMode.Basic:
                {
                    foreach (var upgradeInfo in _allUpgradeInfo)
                    {
                        upgradeInfo.gameObject.SetActive(true);
                    }

                    _scrollRectController.DefaultWork();
                    _textDisplay.StopDisplaying();
                    _radiusDisplay.StopDisplaying();
                    _modulesTester.StopTest();

                    break;
                }
            case ViewMode.Detailed:
                {
                    foreach (var upgradeInfo in _allUpgradeInfo)
                    {
                        if (_triggeringUpgradeInfo == upgradeInfo)
                        {
                            continue;
                        }

                        upgradeInfo.gameObject.SetActive(false);
                    }

                    _scrollRectController.ViewDetailedInformation(_triggeringUpgradeInfo.RectTransform);

                    break;
                }
            default:
                {
                    Debug.LogError($"Unknown ViewMode: {mode}");

                    break;
                }
        }
    }

    public enum ViewMode
    {
        Basic,
        Detailed
    }

#if UNITY_EDITOR
    [ContextMenu("Update All Upgrade Info")]
    public void UpdateAllUpgradeInfo()
    {
        _allUpgradeInfo = GetComponentsInChildren<UpgradeInfo>(true);
        EditorUtility.SetDirty(this);
        Debug.Log($"{_allUpgradeInfo.Length} UpgradeInfo objects found and assigned.");
    }
#endif

}

