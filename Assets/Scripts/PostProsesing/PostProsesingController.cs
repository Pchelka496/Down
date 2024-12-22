using Additional;
using Creatures.Player;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Threading;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;
using static PostProcessingControllerConfig;

public class PostProcessingController : System.IDisposable
{
    readonly PostProcessingControllerConfig _config;
    readonly Volume _globalVolume;

    readonly VolumeProfileData _lobbyMode;
    readonly VolumeProfileData _gameplayMode;
    readonly VolumeProfileData _warpMode;
    readonly VolumeProfileData _playerTakeImpact;

    EnumState _currentState;
    EnumState _currentMainState;

    CancellationTokenSource _changeStateCts;
    event System.Action DisposeEvents;

    public PostProcessingController(Initializer initializer)
    {
        _config = initializer.Config;
        _globalVolume = initializer.GlobalVolume;

        _lobbyMode = _config.Lobby;
        _gameplayMode = _config.Gameplay;
        _warpMode = _config.Warp;
        _playerTakeImpact = _config.PlayerTakeImpact;

        RoundEnd();
    }

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(GlobalEventsManager globalEventsManager, PlayerController player)
    {
        globalEventsManager.SubscribeToRoundStarted(RoundStart);
        globalEventsManager.SubscribeToRoundEnded(RoundEnd);
        globalEventsManager.SubscribeToFastTravelStarted(FastTravelStart);
        player.HealthModule.SubscribeToOnPlayerTakeImpact(PlayerTakeImpact);

        DisposeEvents += () => globalEventsManager?.UnsubscribeFromFastTravelStarted(FastTravelStart);
        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundStarted(RoundStart);
        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundEnded(RoundEnd);
        DisposeEvents += () =>
        {
            if (player != null & player.HealthModule != null)
            {
                player.HealthModule.UnsubscribeFromOnPlayerTakeImpact(PlayerTakeImpact);
            }
        };
    }

    private void FastTravelStart() => SetState(EnumState.Warp);
    private void RoundStart() => SetState(EnumState.Gameplay);
    private void RoundEnd() => SetState(EnumState.Lobby);
    private void PlayerTakeImpact() => SetState(EnumState.PlayerTakeImpact);

    private async void SetState(EnumState newState)
    {
        _currentState = newState;

        var increeseDuration = 1f;
        var decreaseDuration = 1f;

        ClearToken(ref _changeStateCts);
        _changeStateCts = new();

        switch (_currentState)
        {
            case EnumState.Lobby:
                {
                    _currentMainState = EnumState.Lobby;
                    _globalVolume.sharedProfile = _lobbyMode.VolumeProfile;

                    increeseDuration = _lobbyMode.TransitionToDuration;
                    decreaseDuration = _lobbyMode.TransitionFromDuration;

                    break;
                }
            case EnumState.Gameplay:
                {
                    _currentMainState = EnumState.Gameplay;
                    _globalVolume.sharedProfile = _gameplayMode.VolumeProfile;

                    increeseDuration = _gameplayMode.TransitionToDuration;
                    decreaseDuration = _gameplayMode.TransitionFromDuration;

                    break;
                }
            case EnumState.Warp:
                {
                    _currentMainState = EnumState.Warp;
                    _globalVolume.sharedProfile = _warpMode.VolumeProfile;

                    increeseDuration = _warpMode.TransitionToDuration;
                    decreaseDuration = _warpMode.TransitionFromDuration;

                    break;
                }
            case EnumState.PlayerTakeImpact:
                {
                    _globalVolume.sharedProfile = _playerTakeImpact.VolumeProfile;

                    increeseDuration = _playerTakeImpact.TransitionToDuration;
                    decreaseDuration = _playerTakeImpact.TransitionFromDuration;

                    SetDelayedState(_config.TakeImpactVolumeDuration, _currentMainState, _changeStateCts.Token).Forget();

                    break;
                }
            default:
                {
                    Debug.LogError($"Unknown {GetType()} state: {newState}");
                    break;
                }
        }

        await SmoothChangeWeight(duration: decreaseDuration,
                                 token: _changeStateCts.Token,
                                 startValue: 1f,
                                 endValue: 0f);

        SmoothChangeWeight(duration: increeseDuration,
                           token: _changeStateCts.Token,
                           startValue: 0f,
                           endValue: 1f).Forget();
    }

    private async UniTask SmoothChangeWeight(float duration, CancellationToken token, float startValue, float endValue)
    {
        _globalVolume.weight = startValue;

        var tweener = DOTween.To(
              () => _globalVolume.weight,
              x =>
              {
                  _globalVolume.weight = x;
              },
              endValue,
              duration);
        try
        {
            await UniTask.WaitForSeconds(duration, cancellationToken: token);
        }
        catch (System.OperationCanceledException)
        {
            tweener.Kill();
        }
        finally
        {
            tweener.Kill();
            _globalVolume.weight = endValue;
        }
    }

    private async UniTask SetDelayedState(float delay, EnumState state, CancellationToken token)
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(delay), cancellationToken: token);

        SetState(state);
    }

    private void ClearToken(ref CancellationTokenSource cts) => ClearTokenSupport.ClearToken(ref cts);

    public void Dispose()
    {
        ClearToken(ref _changeStateCts);
        DisposeEvents?.Invoke();
    }

    private enum EnumState
    {
        Warp,
        Gameplay,
        Lobby,
        PlayerTakeImpact
    }

    [System.Serializable]
    public struct Initializer
    {
        [field: SerializeField] public PostProcessingControllerConfig Config { get; set; }
        [field: SerializeField] public Volume GlobalVolume { get; set; }
    }
}
