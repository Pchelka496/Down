using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using Core.Installers;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class BoosterIndicator : MonoBehaviour
{
    [SerializeField] AssetReference _boosterIconReference;
    [SerializeField] RectTransform _boosterStartPosition;
    [SerializeField] Vector2 _iconDirection;
    [SerializeField] float _iconDistance;

    int _maxCharges;
    readonly List<BoosterIcon> _boosterIcons = new();
    AddressableLouderHelper.LoadOperationData<GameObject> _loadOperationData;

    System.Action DisposeEvents;

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(GlobalEventsManager globalEventsManager)
    {
        globalEventsManager.SubscribeToRoundStarted(RoundStart);
        globalEventsManager.SubscribeToWarpStarted(WarpStart);
        globalEventsManager.SubscribeToRoundEnded(RoundEnd);

        DisposeEvents += () => globalEventsManager?.UnsubscribeFromWarpStarted(WarpStart);
        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundStarted(RoundStart);
        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundEnded(RoundEnd);
    }

    private void Start()
    {
        RoundEnd();
    }

    private void RoundStart() => ActiveSetting(_maxCharges);
    private void WarpStart() => ActiveSetting(_maxCharges);
    private void RoundEnd() => gameObject.SetActive(false);

    private void ActiveSetting(int maxCharges)
    {
        if (maxCharges <= 0)
        {
            ClearIcons();
            gameObject.SetActive(false);

            return;
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    public async UniTaskVoid UpdateMaxChargeAmount(int maxCharges)
    {
        _maxCharges = maxCharges;

        ActiveSetting(_maxCharges);

        while (_boosterIcons.Count < maxCharges)
        {
            var boosterIcon = await CreateBoosterIcon();
            if (boosterIcon == null) return;

            _boosterIcons.Add(boosterIcon);
            PositionIcon(boosterIcon, _boosterIcons.Count - 1);
        }

        while (_boosterIcons.Count > maxCharges)
        {
            var lastIcon = _boosterIcons[^1];
            _boosterIcons.RemoveAt(_boosterIcons.Count - 1);
            Destroy(lastIcon.gameObject);
        }
    }

    public void UpdateCurrentChargeAmount(int currentCharges)
    {
        for (int i = 0; i < _boosterIcons.Count; i++)
        {
            if (i < currentCharges)
            {
                _boosterIcons[i].Activate();
            }
            else
            {
                _boosterIcons[i].Deactivate();
            }
        }
    }

    private void ClearIcons()
    {
        foreach (var icon in _boosterIcons)
        {
            Destroy(icon.gameObject);
        }

        _boosterIcons.Clear();

        if (_loadOperationData.Handle.IsValid() &&
            _loadOperationData.Handle.Status == AsyncOperationStatus.Succeeded)
        {
            Addressables.Release(_loadOperationData.Handle);
        }
    }

    private async UniTask<BoosterIcon> CreateBoosterIcon()
    {
        _loadOperationData = await AddressableLouderHelper.LoadAssetAsync<GameObject>(_boosterIconReference);

        var boosterIcon = GameplaySceneInstaller.DiContainer.InstantiatePrefabForComponent<BoosterIcon>(_loadOperationData.LoadAsset, _boosterStartPosition);

        return boosterIcon;
    }

    private void PositionIcon(BoosterIcon icon, int index)
    {
        var localPosition = _iconDistance * index * _iconDirection.normalized;
        icon.RectTransform.localPosition = localPosition;
    }

    private void OnDestroy()
    {
        ClearIcons();
        DisposeEvents?.Invoke();
    }
}
