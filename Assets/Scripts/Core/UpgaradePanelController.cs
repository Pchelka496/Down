using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;

public class UpgradePanelController : MonoBehaviour
{
    [SerializeField] string _upgradePanelPrefabAddress;
    PlayerUpgradePanel _upgradePanel;
    AsyncOperationHandle<GameObject> _upgradePanelHandle;

    public PlayerUpgradePanel UpgradePanel => _upgradePanel;

    [Inject]
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
        _upgradePanel = GameplaySceneInstaller.DiContainer.InstantiatePrefabForComponent<PlayerUpgradePanel>(await LoadPrefabs(_upgradePanelPrefabAddress));
        _upgradePanel.transform.SetParent(transform, false);
    }

    private void ClearPanel()
    {
        Destroy(_upgradePanel.gameObject);
        Addressables.Release(_upgradePanelHandle);
    }

    private async UniTask<GameObject> LoadPrefabs(string address)
    {
        _upgradePanelHandle = Addressables.LoadAssetAsync<GameObject>(address);

        await _upgradePanelHandle;

        if (_upgradePanelHandle.Status == AsyncOperationStatus.Succeeded)
        {
            return _upgradePanelHandle.Result;
        }
        else
        {
            Debug.LogError("Error loading via Addressables.");
            return default;
        }
    }

}
