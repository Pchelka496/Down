using Unity.Cinemachine;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using DG.Tweening;

[System.Serializable]
public class LensController : System.IDisposable
{
    [SerializeField] float _lobbyModeOrthographicSize = 10f;
    [SerializeField] float _maxOrthographicSize = 10f;
    [SerializeField] float _minOrthographicSize = 5f;

    [SerializeField] float _minVelocityThreshold = 10f;
    [SerializeField] float _maxVelocityThreshold = 40f;

    [SerializeField] float _changeThreshold = 5f;

    [SerializeField] AnimationCurve _lensInterpolationCurve;
    [SerializeField] float _updateFrequency = 1f;

    CinemachineCamera _camera;
    Rigidbody2D _player;
    CancellationTokenSource _cts;

    public void Initialize(CinemachineCamera camera, Rigidbody2D player)
    {
        _camera = camera;
        _player = player;
    }

    public void SetLobbyMode()
    {
        ClearToken(ref _cts);
        _camera.Lens.OrthographicSize = _lobbyModeOrthographicSize;
    }

    public void SetFlyMode()
    {
        StartAdjustingLensSettingsAsync().Forget();
    }

    private async UniTaskVoid StartAdjustingLensSettingsAsync()
    {
        ClearToken(ref _cts);

        _cts = new CancellationTokenSource();

        await SmoothSizeIncrease();

        AdjustLensSettingsBasedOnVelocity(_cts.Token).Forget();
    }

    private async UniTask SmoothSizeIncrease()
    {
        _camera.Lens.OrthographicSize = 0;

        var tween = DOTween.To(() => _camera.Lens.OrthographicSize,
                   x =>
                   {
                       var lensSettings = _camera.Lens;
                       _camera.Lens.OrthographicSize = x;
                   },
                   _minOrthographicSize,
                   3f);

        await tween.AsyncWaitForCompletion();
    }

    private async UniTaskVoid AdjustLensSettingsBasedOnVelocity(CancellationToken token)
    {
        float previousClampedVelocity = Mathf.FloorToInt(_minVelocityThreshold / _changeThreshold) * _changeThreshold;

        while (!token.IsCancellationRequested)
        {
            float velocityY = Mathf.Abs(_player.velocity.y);

            float clampedVelocity = Mathf.Clamp(velocityY, _minVelocityThreshold, _maxVelocityThreshold);

            float roundedVelocity = Mathf.FloorToInt(clampedVelocity / _changeThreshold) * _changeThreshold;

            if (roundedVelocity != previousClampedVelocity)
            {
                float t = Mathf.InverseLerp(_minVelocityThreshold, _maxVelocityThreshold, roundedVelocity);

                float interpolatedValue = _lensInterpolationCurve.Evaluate(t);

                await SmoothlyAdjustLensSettings(interpolatedValue, token);

                previousClampedVelocity = roundedVelocity;
            }

            await UniTask.WaitForSeconds(_updateFrequency, cancellationToken: token);
        }
    }

    private async UniTask SmoothlyAdjustLensSettings(float interpolationFactor, CancellationToken token)
    {
        float targetOrthographicSize = Mathf.Lerp(_minOrthographicSize, _maxOrthographicSize, interpolationFactor);

        float startValue = _camera.Lens.OrthographicSize;

        float duration = 1f;
        float timeElapsed = 0f;

        while (timeElapsed < duration && !token.IsCancellationRequested)
        {
            float t = timeElapsed / duration;
            float newValue = Mathf.Lerp(startValue, targetOrthographicSize, t);

            _camera.Lens.OrthographicSize = newValue;

            await UniTask.Yield(token);

            timeElapsed += Time.deltaTime;
        }

        _camera.Lens.OrthographicSize = targetOrthographicSize;
    }

    private void ClearToken(ref CancellationTokenSource cts) => ClearTokenSupport.ClearToken(ref cts);

    public void Dispose()
    {
        ClearToken(ref _cts);
    }

}
