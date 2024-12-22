using System;
using System.Runtime.CompilerServices;
using System.Threading;
using Additional;
using Creatures.Player;
using Cysharp.Threading.Tasks;
using ScriptableObject;
using UnityEngine;
using Zenject;

public class BackgroundController : IDisposable
{
    const float MIN_HEIGHT = 0f;
    const float GAMEPLAY_MODE_CHECK_INTERVAL = 0.5f;
    const float WARP_MODE_CHECK_INTERVAL = 0.02f;
    const float LOBBY_MODE_CHECK_INTERVAL = 1f;

    Gradient _backgroundGradient;
    Camera _camera;
    MapController _mapController;
    BackgroundControllerConfig _config;
    PlayerController _player;
    float _maxLevelHeight;
    TimeSpan _backgroundUpdaterCheckInterval;

    CancellationTokenSource _cts;
    event Action DisposeEvents;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(PlayerController player, MapController mapController, Camera camera, GlobalEventsManager globalEventsManager)
    {
        _player = player;
        _camera = camera;
        _mapController = mapController;

        globalEventsManager.SubscribeToRoundStarted(RoundStart);
        globalEventsManager.SubscribeToRoundEnded(RoundEnd);
        globalEventsManager.SubscribeToFastTravelStarted(FastTravelStart);

        DisposeEvents += () => globalEventsManager?.UnsubscribeFromFastTravelStarted(FastTravelStart);
        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundStarted(RoundStart);
        DisposeEvents += () => globalEventsManager?.UnsubscribeFromRoundEnded(RoundEnd);
    }

    public void Initialize(BackgroundControllerConfig config)
    {
        _config = config;
        _backgroundGradient = _config.BackgroundGradient;
        _maxLevelHeight = _mapController.FullMapHeight;

        SetState(EnumState.Lobby);
    }

    private void FastTravelStart() => SetState(EnumState.FastTravel);
    private void RoundStart() => SetState(EnumState.Gameplay);
    private void RoundEnd() => SetState(EnumState.Lobby);

    private void SetState(EnumState newState)
    {
        switch (newState)
        {
            case EnumState.Lobby:
                {
                    _backgroundUpdaterCheckInterval = TimeSpan.FromSeconds(LOBBY_MODE_CHECK_INTERVAL);
                    break;
                }
            case EnumState.Gameplay:
                {
                    _backgroundUpdaterCheckInterval = TimeSpan.FromSeconds(GAMEPLAY_MODE_CHECK_INTERVAL);
                    break;
                }
            case EnumState.FastTravel:
                {
                    _backgroundUpdaterCheckInterval = TimeSpan.FromSeconds(WARP_MODE_CHECK_INTERVAL);
                    break;
                }
            default:
                {
                    Debug.LogError($"Unknown {this.GetType()} state: {newState}");
                    break;
                }
        }
        ClearToken();
        _cts = new();

        StartBackgroundUpdater(_cts.Token).Forget();
    }

    private async UniTaskVoid StartBackgroundUpdater(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                UpdateBackground();
                await UniTask.Delay(_backgroundUpdaterCheckInterval, cancellationToken: cancellationToken);
            }
        }
        catch (System.OperationCanceledException)
        {
        }
    }

    private void UpdateBackground()
    {
        float playerHeight = _player.transform.position.y;

        float normalizedHeight = Mathf.InverseLerp(_maxLevelHeight, MIN_HEIGHT, playerHeight);

        ChangeBackground(normalizedHeight);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ChangeBackground(float normalizedHeight)
    {
        var backgroundColor = _backgroundGradient.Evaluate(normalizedHeight);

        _camera.backgroundColor = backgroundColor;
    }

    private void ClearToken() => ClearTokenSupport.ClearToken(ref _cts);

    public void Dispose()
    {
        ClearToken();
        DisposeEvents?.Invoke();
    }

    private enum EnumState
    {
        Lobby,
        Gameplay,
        FastTravel,
    }
}
