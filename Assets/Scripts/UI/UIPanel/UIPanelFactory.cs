using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Zenject;

public class UIPanelFactory
{
    private readonly DiContainer _diContainer;

    public UIPanelFactory(DiContainer diContainer)
    {
        _diContainer = diContainer;
    }

    public async UniTask<IUIPanel> CreatePanelAsync(AssetReference assetReference, Transform parent)
    {
        var loadAssetData = await AddressableLouderHelper.LoadAssetAsync<GameObject>(assetReference);
        var panel = _diContainer.InstantiatePrefabForComponent<IUIPanel>(loadAssetData.LoadAsset, parent);

        return panel;
    }

}
