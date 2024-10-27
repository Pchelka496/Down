using Unity.Cinemachine;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using DG.Tweening;
using System;

[System.Serializable]
public class LensController : System.IDisposable
{
    const float _lensFarClipPlane = 1000f;
    const float _lensNearClipPlane = 0.3f;
    const bool _lensIsOrthographic = true;

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

    public void SetLobbyMod()
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
        try
        {
            ClearToken(ref _cts);

            _cts = new CancellationTokenSource();

            await SmoothSizeIncrease();

            await AdjustLensSettingsBasedOnVelocity(_cts.Token);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Exception caught in StartAdjustingLensSettingsAsync: {ex}");
        }
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

    private async UniTask AdjustLensSettingsBasedOnVelocity(CancellationToken token)
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

    private void ClearToken(ref CancellationTokenSource cts)
    {
        if (cts == null) return;

        if (!cts.IsCancellationRequested)
        {
            cts.Cancel();
        }

        cts.Dispose();
        cts = null;
    }

    public void Dispose()
    {
        ClearToken(ref _cts);
    }

}
