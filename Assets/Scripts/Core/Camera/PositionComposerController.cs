using UnityEngine;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using System.Threading;
using System;
using Additional;

[System.Serializable]
public class PositionComposerController : System.IDisposable
{
    [SerializeField] ScreenComposerSettings _lobbyModeComposition;
    [SerializeField] ScreenComposerSettings _defaultFlyModeComposition;
    [SerializeField] CinemachinePositionComposer _composer;
    [SerializeField] float _maxYScreenPosition = -1f;
    [SerializeField] float _minYScreenPosition = 0f;

    [SerializeField] float _minVelocityThreshold = 0f;
    [SerializeField] float _maxVelocityThreshold = 20f;

    [SerializeField] float _updateFrequency = 1f;
    [SerializeField] AnimationCurve _transitionCurve;

    float _currentYScreenPosition;

    Rigidbody2D _playerRb;
    CancellationTokenSource _cts;

    public void Initialize(Rigidbody2D playerRb)
    {
        _playerRb = playerRb;
    }

    public void SetLobbyMode()
    {
        ClearToken(ref _cts);
        _composer.Composition = _lobbyModeComposition;
    }

    public void SetFlyMode()
    {
        ClearToken(ref _cts);
        _cts = new();

        try
        {
            StartAdjustingPositionComposterSettings(_cts.Token).Forget();
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"Exception caught in StartAdjustingPositionComposterSettings: {ex}");
        }
    }

    private async UniTaskVoid StartAdjustingPositionComposterSettings(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            await AdjustPositionComposterSetting(token);
            await UniTask.WaitForSeconds(_updateFrequency, cancellationToken: token);
        }
    }

    private async UniTask AdjustPositionComposterSetting(CancellationToken token)
    {
        var velocityY = Mathf.Abs(_playerRb.velocity.y);

        var normalizedVelocity = Mathf.InverseLerp(_minVelocityThreshold, _maxVelocityThreshold, velocityY);
        normalizedVelocity = Mathf.Clamp01(normalizedVelocity);

        var targetYScreenPosition = Mathf.Lerp(_minYScreenPosition, _maxYScreenPosition, normalizedVelocity);

        var timeElapsed = 0f;
        var duration = 0.5f;
        var initialYScreenPosition = _currentYScreenPosition;

        var defaultComposition = _defaultFlyModeComposition;

        while (timeElapsed < duration)
        {
            var curveValue = _transitionCurve.Evaluate(timeElapsed / duration);

            _currentYScreenPosition = Mathf.Lerp(initialYScreenPosition, targetYScreenPosition, curveValue);

            defaultComposition.ScreenPosition = new(0f, _currentYScreenPosition);
            _composer.Composition = defaultComposition;

            await UniTask.Yield(cancellationToken: token);

            timeElapsed += Time.deltaTime;
        }

        _currentYScreenPosition = targetYScreenPosition;
        defaultComposition.ScreenPosition = new(0f, _currentYScreenPosition);
        _composer.Composition = defaultComposition;
    }

    private void ClearToken(ref CancellationTokenSource cts) => ClearTokenSupport.ClearToken(ref cts);

    public void Dispose()
    {
        ClearToken(ref _cts);
    }

}
