using Additional;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public class ExchangePanelOpenButton : MonoBehaviour
{
    const float ORIGINAL_SCALE = 1f;

    [SerializeField] Button[] _buttons;

    [Header("Animation Settings")]
    [SerializeField] float _targetScale = 1.2f;
    [SerializeField] float _animationDuration = 0.3f;
    [SerializeField] Ease _animationEase = Ease.OutBack;

    [SerializeField] bool _separateAnimation;
    [NaughtyAttributes.ShowIf("_separateAnimation")][SerializeField] Transform _money;
    [NaughtyAttributes.ShowIf("_separateAnimation")][SerializeField] Transform _diamond;
    [NaughtyAttributes.ShowIf("_separateAnimation")][SerializeField] Transform _energy;

    LobbyUIPanelFacade _lobbyUIPanel;
    PlayerResourcedKeeper _rewardKeeper;
    CancellationTokenSource _cts;

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Удалите неиспользуемые закрытые члены", Justification = "<Ожидание>")]
    private void Construct(LobbyUIPanelFacade lobbyUIPanel, PlayerResourcedKeeper rewardKeeper)
    {
        _lobbyUIPanel = lobbyUIPanel;
        _rewardKeeper = rewardKeeper;

        _rewardKeeper.SubscribeToOnResourceDecreaseFailed(OnResourceDecreaseFailed);
    }

    private void Start()
    {
        foreach (var button in _buttons)
        {
            button.onClick.AddListener(_lobbyUIPanel.OpenExchangePanel);
        }
    }

    private void OnDisable()
    {
        ClearToken();
    }

    private void OnResourceDecreaseFailed(PlayerResourcedKeeper.ResourceDecreaseFailedEventArgs eventArgs)
    {
        ClearToken();
        _cts = new();

        Transform animatedTransform = null;

        if (!_separateAnimation)
        {
            animatedTransform = transform;
        }
        else
        {
            switch (eventArgs.ResourceType)
            {
                case PlayerResourcedKeeper.ResourceType.Money:
                    {
                        animatedTransform = _money;
                        break;
                    }
                case PlayerResourcedKeeper.ResourceType.Diamonds:
                    {
                        animatedTransform = _diamond;
                        break;
                    }
                case PlayerResourcedKeeper.ResourceType.Energy:
                    {
                        animatedTransform = _energy;
                        break;
                    }
            }
        }

        if (animatedTransform != null)
        {
            ResourceDecreaseFailed(_cts.Token, animatedTransform).Forget();
        }
    }

    private async UniTaskVoid ResourceDecreaseFailed(CancellationToken token, Transform animatedTransform)
    {
        try
        {
            await AnimateButtonScale(animatedTransform, _targetScale, _animationDuration, _animationEase, token);
            await AnimateButtonScale(animatedTransform, ORIGINAL_SCALE, _animationDuration, _animationEase, token);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (animatedTransform != null)
            {
                animatedTransform.localScale = new(ORIGINAL_SCALE, ORIGINAL_SCALE, ORIGINAL_SCALE);
            }
        }
    }

    private async UniTask AnimateButtonScale(Transform animatedTransform, float targetScale, float duration, Ease ease, CancellationToken token)
    {
        var tcs = new UniTaskCompletionSource();
        var tweener = animatedTransform.DOScale(targetScale, duration).SetEase(ease);

        tweener.OnComplete(() => tcs.TrySetResult());
        tweener.OnKill(() => tcs.TrySetCanceled());

        await tcs.Task.AttachExternalCancellation(token);
    }

    private void ClearToken() => ClearTokenSupport.ClearToken(ref _cts);

    private void OnDestroy()
    {
        _rewardKeeper.UnsubscribeFromOnResourceDecreaseFailed(OnResourceDecreaseFailed);
        ClearToken();
    }
}

