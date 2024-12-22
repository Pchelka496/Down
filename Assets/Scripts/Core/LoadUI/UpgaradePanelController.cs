using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

public class LobbyUIPanelFacade : MonoBehaviour
{
    const string UPGRADE_PANEL_ID = "UpgradePanel";
    const string CUSTOMIZATION_PANEL_ID = "CustomizationPanel";
    const string FAST_TRAVEL_MODULE_CONTROLLER_ID = "FastTravelController";
    const string EXCHANGER_PANEL_ID = "ExchangePanel";
    const string SETTING_PANEL_ID = "SettingPanel";
    const string TUTORIAL_PANEL_ID = "TutorialPanel";

    [SerializeField] AssetReference _upgradePanelPrefabReference;
    [SerializeField] AssetReference _customizationPanelPrefabReference;
    [SerializeField] AssetReference _warpEngineController;
    [SerializeField] AssetReference _exchangerPanel;
    [SerializeField] AssetReference _settingPanel;
    [SerializeField] AssetReference _tutorialPanel;

    UIPanelManager _panelManager;
    event System.Action DisposeEvents;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(DiContainer diContainer, GlobalEventsManager globalEventsManager)
    {
        var factory = new UIPanelFactory(diContainer);
        _panelManager = new UIPanelManager(factory, transform);

        globalEventsManager.SubscribeToRoundStarted(RoundStart);

        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundStarted(RoundStart);
    }

    private void RoundStart() => ClearPanels();

    public void OpenUpgradePanel() => _panelManager.OpenPanelAsync(UPGRADE_PANEL_ID, _upgradePanelPrefabReference).Forget();
    public void OpenCustomizationPanel() => _panelManager.OpenPanelAsync(CUSTOMIZATION_PANEL_ID, _customizationPanelPrefabReference).Forget();
    public void OpenFastTravelModuleController() => _panelManager.OpenPanelAsync(FAST_TRAVEL_MODULE_CONTROLLER_ID, _warpEngineController).Forget();
    public void OpenExchangePanel() => _panelManager.OpenPanelAsync(EXCHANGER_PANEL_ID, _exchangerPanel).Forget();
    public void OpenSettingPanel() => _panelManager.OpenPanelAsync(SETTING_PANEL_ID, _settingPanel).Forget();
    public void OpenTutorialPanel() => _panelManager.OpenPanelAsync(TUTORIAL_PANEL_ID, _tutorialPanel).Forget();

    private void ClearPanels()
    {
        _panelManager.CloseAllPanels();
    }

    private void OnDestroy()
    {
        DisposeEvents?.Invoke();
    }
}
