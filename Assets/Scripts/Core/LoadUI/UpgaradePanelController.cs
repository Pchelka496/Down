using Core;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

public class LobbyUIPanelFacade : MonoBehaviour
{
    const string UPGRADE_PANEL_ID = "UpgradePanel";
    const string CUSTOMIZATION_PANEL_ID = "CustomizationPanel";
    const string WARP_ENGINE_CONTROLLER_ID = "WarpEngineController";
    const string EXCHANGER_PANEL_ID = "ExchangePanel";

    [SerializeField] AssetReference _upgradePanelPrefabReference;
    [SerializeField] AssetReference _customizationPanelPrefabReference;
    [SerializeField] AssetReference _warpEngineController;
    [SerializeField] AssetReference _exchangerPanel;

    UIPanelManager _panelManager;
    event System.Action DisposeEvents;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:������� �������������� �������� �����", Justification = "<��������>")]
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
    public void OpenWarpEngineController() => _panelManager.OpenPanelAsync(WARP_ENGINE_CONTROLLER_ID, _warpEngineController).Forget();
    public void OpenExchangePanel() => _panelManager.OpenPanelAsync(EXCHANGER_PANEL_ID, _exchangerPanel).Forget();

    private void ClearPanels()
    {
        _panelManager.CloseAllPanels();
    }

    private void OnDestroy()
    {
        DisposeEvents?.Invoke();
    }
}
