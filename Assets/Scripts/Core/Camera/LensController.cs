using Unity.Cinemachine;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using DG.Tweening;
using Additional;

[System.Serializable]
public class LensController : System.IDisposable
{
    const float SMOOTH_INCREASE_SIZE_DURATION = 3f;

    [SerializeField] float _lobbyModeOrthographicSize = 10f;
    [SerializeField] float _gameplayMinOrthographicSize = 5f;
    [SerializeField] float _gameplayMaxOrthographicSize = 10f;

    [SerializeField] float _minVelocityThreshold = 10f;
    [SerializeField] float _maxVelocityThreshold = 40f;
    [SerializeField] float _changeThreshold = 5f;

    [SerializeField] AnimationCurve _lensInterpolationCurve;
    [SerializeField] float _updateFrequency = 1f;

    [SerializeField] float _warpModeOrthographicSize = 10f;

    CinemachineCamera _camera;
    Rigidbody2D _player;
    CancellationTokenSource _cts;
    LensState _currentState;

    public float LobbyModeOrthographicSize => _lobbyModeOrthographicSize;

    public void Initialize(CinemachineCamera camera, Rigidbody2D player)
    {
        _camera = camera;
        _player = player;
        _currentState = LensState.Lobby;

        SetLobbyMode();
    }

    public void SetState(LensState state)
    {
        if (_currentState == state) return;

        _currentState = state;
        ClearToken(ref _cts);

        switch (state)
        {
            case LensState.Lobby:
                {
                    SetLobbyMode();
                    break;
                }
            case LensState.Gameplay:
                {
                    SetGameplayMode();
                    break;
                }
            case LensState.Warp:
                {
                    SetWarpMode();
                    break;
                }
            default:
                {
                    Debug.LogWarning($"Unhandled LensState: {state}");
                    break;
                }
        }
    }

    private void SetLobbyMode()
    {

        SmoothIncreaseSize(_lobbyModeOrthographicSize).Forget();
    }

    private void SetGameplayMode()
    {
        StartAdjustingLensSettingsAsync().Forget();
    }

    private void SetWarpMode()
    {
        _camera.Lens.OrthographicSize = _warpModeOrthographicSize;
    }

    private async UniTaskVoid StartAdjustingLensSettingsAsync()
    {
        ClearToken(ref _cts);
        _cts = new();

        await SmoothIncreaseSize(_gameplayMinOrthographicSize);

        AdjustLensSettingsBasedOnVelocity(_cts.Token).Forget();
    }

    private async UniTask SmoothIncreaseSize(float newOrthographicSize)
    {
        await DOTween.To(
            () => _camera.Lens.OrthographicSize,
            x =>
            {
                var lensSettings = _camera.Lens;
                _camera.Lens.OrthographicSize = x;
            },
            newOrthographicSize,
            SMOOTH_INCREASE_SIZE_DURATION).AsyncWaitForCompletion();
    }

    private async UniTaskVoid AdjustLensSettingsBasedOnVelocity(CancellationToken token)
    {
        var previousClampedVelocity = Mathf.FloorToInt(_minVelocityThreshold / _changeThreshold) * _changeThreshold;

        while (!token.IsCancellationRequested)
        {
            var velocityY = Mathf.Abs(_player.velocity.y);

            var clampedVelocity = Mathf.Clamp(velocityY, _minVelocityThreshold, _maxVelocityThreshold);
            var roundedVelocity = Mathf.FloorToInt(clampedVelocity / _changeThreshold) * _changeThreshold;

            if (roundedVelocity != previousClampedVelocity)
            {
                var t = Mathf.InverseLerp(_minVelocityThreshold, _maxVelocityThreshold, roundedVelocity);
                var interpolatedValue = _lensInterpolationCurve.Evaluate(t);

                await SmoothlyAdjustLensSettings(interpolatedValue, token);

                previousClampedVelocity = roundedVelocity;
            }

            await UniTask.WaitForSeconds(_updateFrequency, cancellationToken: token);
        }
    }

    private async UniTask SmoothlyAdjustLensSettings(float interpolationFactor, CancellationToken token)
    {
        var targetOrthographicSize = Mathf.Lerp(_gameplayMinOrthographicSize, _gameplayMaxOrthographicSize, interpolationFactor);

        var startValue = _camera.Lens.OrthographicSize;
        var duration = 1f;
        var timeElapsed = 0f;

        while (timeElapsed < duration && !token.IsCancellationRequested)
        {
            var t = timeElapsed / duration;
            var newValue = Mathf.Lerp(startValue, targetOrthographicSize, t);

            _camera.Lens.OrthographicSize = newValue;

            await UniTask.Yield(token);

            timeElapsed += Time.deltaTime;
        }

        _camera.Lens.OrthographicSize = targetOrthographicSize;
    }

    private void ClearToken(ref CancellationTokenSource cts) => ClearTokenSupport.ClearToken(ref cts);

    public void Dispose() => ClearToken(ref _cts);

    public enum LensState
    {
        Lobby,
        Gameplay,
        Warp
    }
}
