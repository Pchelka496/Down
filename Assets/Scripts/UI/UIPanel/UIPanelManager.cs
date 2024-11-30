using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class UIPanelManager
{
    readonly UIPanelFactory _factory;
    readonly Transform _parentTransform;
    readonly Dictionary<string, IUIPanel> _activePanels = new();

    public UIPanelManager(UIPanelFactory factory, Transform parentTransform)
    {
        _factory = factory;
        _parentTransform = parentTransform;
    }

    public async UniTask CreatePanelAsync(string panelId, AssetReference assetReference)
    {
        if (_activePanels.ContainsKey(panelId))
        {
            return;
        }

        var panel = await _factory.CreatePanelAsync(assetReference, _parentTransform);
        _activePanels[panelId] = panel;
    }

    public async UniTask OpenPanelAsync(string panelId, AssetReference assetReference)
    {
        if (_activePanels.ContainsKey(panelId))
        {
            _activePanels[panelId].Open();
            return;
        }

        var panel = await _factory.CreatePanelAsync(assetReference, _parentTransform);

        panel.Open();
        _activePanels[panelId] = panel;
    }

    public void ClosePanel(string panelId)
    {
        if (_activePanels.TryGetValue(panelId, out var panel))
        {
            panel.Close();
            _activePanels.Remove(panelId);
        }
    }

    public void CloseAllPanels()
    {
        foreach (var panel in _activePanels.Values)
        {
            panel.Close();
        }
        _activePanels.Clear();
    }

}
