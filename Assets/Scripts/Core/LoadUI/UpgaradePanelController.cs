using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

public class LobbyUIPanelFacade : MonoBehaviour
{
    const string UPGRADE_PANEL_ID = "UpgradePanel";
    const string CUSTOMIZATION_PANEL_ID = "CustomizationPanel";

    [SerializeField] AssetReference _upgradePanelPrefabReference;
    [SerializeField] AssetReference _customizationPanelPrefabReference;

    UIPanelManager _panelManager;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Удалите неиспользуемые закрытые члены", Justification = "<Ожидание>")]
    private void Construct(DiContainer diContainer, LevelManager levelManager)
    {
        var factory = new UIPanelFactory(diContainer);
        _panelManager = new UIPanelManager(factory, transform);

        levelManager.SubscribeToRoundStart(RoundStart);
    }

    private void RoundStart(LevelManager levelManager)
    {
        ClearPanels();
        levelManager.SubscribeToRoundEnd(RoundEnd);
    }

    private void RoundEnd(LevelManager levelManager, EnumRoundResults results)
    {
        levelManager.SubscribeToRoundStart(RoundStart);
    }

    public void OpenUpgradePanel() => _panelManager.OpenPanelAsync(UPGRADE_PANEL_ID, _upgradePanelPrefabReference).Forget();
    public void OpenCustomizationPanel() => _panelManager.OpenPanelAsync(CUSTOMIZATION_PANEL_ID, _customizationPanelPrefabReference).Forget();

    private void ClearPanels()
    {
        _panelManager.CloseAllPanels();
    }

}
