using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

public class LobbyUIElementLoader : MonoBehaviour
{
    [SerializeField] AssetReference _upgradePanelPrefabReference;

    PlayerUpgradePanel _upgradePanel;
    AddressableLouderHelper.LoadOperationData<GameObject> _upgradePanelData;

    public PlayerUpgradePanel UpgradePanel => _upgradePanel;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Удалите неиспользуемые закрытые члены", Justification = "<Ожидание>")]
    private void Construct(LevelManager levelManager)
    {
        levelManager.SubscribeToRoundStart(RoundStart);
        CreatePanel();
    }

    private void RoundStart(LevelManager levelManager)
    {
        levelManager.SubscribeToRoundEnd(RoundEnd);
        ClearPanel();
    }

    private void RoundEnd(LevelManager levelManager, EnumRoundResults results)
    {
        levelManager.SubscribeToRoundStart(RoundStart);
        CreatePanel();
    }

    private async void CreatePanel()
    {
        _upgradePanel = GameplaySceneInstaller.DiContainer.InstantiatePrefabForComponent<PlayerUpgradePanel>(await LoadPrefabs(_upgradePanelPrefabReference));
        _upgradePanel.transform.SetParent(transform, false);
    }

    private void ClearPanel()
    {
        Destroy(_upgradePanel.gameObject);

        if (_upgradePanelData.Handle.IsValid() &&
            _upgradePanelData.Handle.Status == AsyncOperationStatus.Succeeded)
        {
            Addressables.Release(_upgradePanelData.Handle);
        }
    }

    private async UniTask<GameObject> LoadPrefabs(AssetReference reference)
    {
        _upgradePanelData = await AddressableLouderHelper.LoadAssetAsync<GameObject>(reference);

        return _upgradePanelData.LoadAsset;
    }

}
