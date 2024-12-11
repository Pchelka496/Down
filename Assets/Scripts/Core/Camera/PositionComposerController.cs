using UnityEngine;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using System.Threading;
using Additional;

[System.Serializable]
public class PositionComposerController : System.IDisposable
{
    [SerializeField] ScreenComposerSettings _lobbyModeComposition;
    [SerializeField] ScreenComposerSettings _defaultGameplayModeComposition;
    [SerializeField] ScreenComposerSettings _warpModeComposition;
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
    ComposerState _currentState;

    public void Initialize(Rigidbody2D playerRb)
    {
        _playerRb = playerRb;
        _currentState = ComposerState.Lobby;

        SetLobbyMode();
    }

    public void SetState(ComposerState state)
    {
        if (_currentState == state) return;

        _currentState = state;
        ClearToken(ref _cts);

        switch (state)
        {
            case ComposerState.Lobby:
                {
                    SetLobbyMode();
                    break;
                }
            case ComposerState.Gameplay:
                {
                    SetGameplayMode();
                    break;
                }
            case ComposerState.Warp:
                {
                    SetWarpMode();
                    break;
                }
            default:
                {
                    Debug.LogWarning($"Unhandled state: {state}");
                    break;
                }
        }
    }

    private void SetLobbyMode()
    {
        _composer.Composition = _lobbyModeComposition;
    }

    private void SetGameplayMode()
    {
        _cts = new CancellationTokenSource();
        StartAdjustingPositionComposterSettings(_cts.Token).Forget();
    }

    private void SetWarpMode()
    {
        _composer.Composition = _warpModeComposition;
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

        var defaultComposition = _defaultGameplayModeComposition;

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

    public void Dispose() => ClearToken(ref _cts);

    public enum ComposerState
    {
        Lobby,
        Gameplay,
        Warp
    }
}

