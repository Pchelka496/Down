using Core.Installers;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class BaseChargeIndicator : MonoBehaviour
{
    [SerializeField] AssetReference _iconReference;
    [SerializeField] RectTransform _container;
    [SerializeField] Vector2 _iconDirection;
    [SerializeField] float _iconSpacing;

    int _maxCharges;
    readonly List<BoosterIcon> _icons = new();
    AddressableLouderHelper.LoadOperationData<GameObject> _loadOperationData;

    System.Action DisposeEvents;

    protected int MaxCharges => _maxCharges;

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    protected virtual void Construct(GlobalEventsManager globalEventsManager)
    {
        globalEventsManager.SubscribeToRoundStarted(RoundStart);
        globalEventsManager.SubscribeToFastTravelStarted(FastTravelStart);
        globalEventsManager.SubscribeToRoundEnded(RoundEnd);

        DisposeEvents += () => globalEventsManager?.UnsubscribeFromFastTravelStarted(FastTravelStart);
        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundStarted(RoundStart);
        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundEnded(RoundEnd);
    }

    private void Start()
    {
        RoundEnd();
    }

    protected virtual void RoundStart() => ActiveSetting(_maxCharges);
    protected virtual void FastTravelStart() => ActiveSetting(_maxCharges);
    protected virtual void RoundEnd() => gameObject.SetActive(false);

    protected virtual bool ActiveSetting(int maxCharges)
    {
        if (maxCharges <= 0)
        {
            ClearIcons();
            gameObject.SetActive(false);

            return false;
        }
        else
        {
            gameObject.SetActive(true);

            return true;
        }
    }

    public async UniTaskVoid UpdateMaxChargeAmount(int maxCharges)
    {
        _maxCharges = maxCharges;

        ActiveSetting(_maxCharges);

        while (_icons.Count < maxCharges)
        {
            var boosterIcon = await CreateIcon();
            if (boosterIcon == null) return;

            _icons.Add(boosterIcon);
        }

        while (_icons.Count > maxCharges)
        {
            var lastIcon = _icons[^1];
            _icons.RemoveAt(_icons.Count - 1);
            Destroy(lastIcon.gameObject);
        }

        UpdateIconPositions();
    }

    public void UpdateCurrentChargeAmount(int currentCharges)
    {
        for (int i = 0; i < _icons.Count; i++)
        {
            if (i < currentCharges)
            {
                _icons[i].Activate();
            }
            else
            {
                _icons[i].Deactivate();
            }
        }
    }

    private void ClearIcons()
    {
        foreach (var icon in _icons)
        {
            Destroy(icon.gameObject);
        }

        _icons.Clear();

        if (_loadOperationData.Handle.IsValid() &&
            _loadOperationData.Handle.Status == AsyncOperationStatus.Succeeded)
        {
            Addressables.Release(_loadOperationData.Handle);
        }
    }

    private async UniTask<BoosterIcon> CreateIcon()
    {
        _loadOperationData = await AddressableLouderHelper.LoadAssetAsync<GameObject>(_iconReference);

        var icon = GameplaySceneInstaller.DiContainer.InstantiatePrefabForComponent<BoosterIcon>(_loadOperationData.LoadAsset, _container);
        return icon;
    }

    private void UpdateIconPositions()
    {
        if (_icons.Count == 0) return;

        var containerSize = _container.rect.size;
        float iconSize = Mathf.Min(containerSize.x, containerSize.y) / _icons.Count - _iconSpacing;
        iconSize = Mathf.Max(iconSize, 0);

        for (int i = 0; i < _icons.Count; i++)
        {
            var icon = _icons[i];
            icon.RectTransform.sizeDelta = new Vector2(iconSize, iconSize);

            var positionOffset = (iconSize + _iconSpacing) * i * _iconDirection.normalized;
            var centerOffset = new Vector2(
                -_iconDirection.x * (containerSize.x - iconSize) / 2,
                -_iconDirection.y * (containerSize.y - iconSize) / 2);

            icon.RectTransform.localPosition = centerOffset + positionOffset;
        }
    }

    private void OnDestroy()
    {
        ClearIcons();
        DisposeEvents?.Invoke();
    }
}
