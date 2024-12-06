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
    const float CHECK_INTERVAL = 0.5f;

    Gradient _backgroundGradient;
    Camera _camera;
    MapController _mapController;
    BackgroundControllerConfig _config;
    PlayerController _player;
    float _maxLevelHeight;

    CancellationTokenSource _cts;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:������� �������������� �������� �����", Justification = "<��������>")]
    private void Construct(PlayerController player, MapController mapController, Camera camera)
    {
        _player = player;
        _camera = camera;
        _mapController = mapController;
    }

    public void Initialize(BackgroundControllerConfig config)
    {
        _config = config;
        _backgroundGradient = _config.BackgroundGradient;
        _maxLevelHeight = _mapController.FullMapHeight;

        _cts = new CancellationTokenSource();
        StartBackgroundUpdaterAsync(_cts.Token).Forget();
    }

    private async UniTaskVoid StartBackgroundUpdaterAsync(CancellationToken cancellationToken)
    {
        var checkTimeSpan = TimeSpan.FromSeconds(CHECK_INTERVAL);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                UpdateBackground();
                await UniTask.Delay(checkTimeSpan, cancellationToken: cancellationToken);
            }
        }
        catch (OperationCanceledException)
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
        Color backgroundColor = _backgroundGradient.Evaluate(normalizedHeight);

        _camera.backgroundColor = backgroundColor;
    }

    public void Dispose()
    {
        ClearTokenSupport.ClearToken(ref _cts);
    }

}


