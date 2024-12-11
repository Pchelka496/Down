using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class UIPanelManager
{
    readonly UIPanelFactory _factory;
    readonly Transform _parentTransform;
    readonly Dictionary<string, IUIPanel> _createdPanels = new();

    public UIPanelManager(UIPanelFactory factory, Transform parentTransform)
    {
        _factory = factory;
        _parentTransform = parentTransform;
    }

    public async UniTask CreatePanelAsync(string panelId, AssetReference assetReference)
    {
        if (_createdPanels.ContainsKey(panelId))
        {
            return;
        }

        var panel = await _factory.CreatePanelAsync(assetReference, _parentTransform);
        _createdPanels[panelId] = panel;
    }

    public async UniTask OpenPanelAsync(string panelId, AssetReference assetReference)
    {
        if (_createdPanels.ContainsKey(panelId))
        {
            _createdPanels[panelId].Open();
            return;
        }

        var panel = await _factory.CreatePanelAsync(assetReference, _parentTransform);

        panel.Open();
        _createdPanels[panelId] = panel;
    }

    public void CloseAllPanels()
    {
        foreach (var panel in _createdPanels.Values)
        {
            panel.Close();
        }
        _createdPanels.Clear();
    }
}
