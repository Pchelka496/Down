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
        var prefab = await assetReference.LoadAssetAsync<GameObject>();
        var panel = _diContainer.InstantiatePrefabForComponent<IUIPanel>(prefab, parent);
        return panel;
    }

}
