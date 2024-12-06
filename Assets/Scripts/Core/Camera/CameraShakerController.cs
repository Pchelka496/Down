using UnityEngine;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using System.Threading;
using System;
using Additional;

[System.Serializable]
public class CameraShakerController : System.IDisposable
{
    [SerializeField] CinemachineBasicMultiChannelPerlin _cameraNoise;
    [SerializeField] float _defaultShakeDuration = 0.5f;
    [SerializeField] float _shakeAmplitude = 2f;
    [SerializeField] float _shakeFrequency = 2f;

    CancellationTokenSource _cts;

    public async UniTaskVoid StartShake(float? duration = null)
    {
        if (_cameraNoise == null) return;

        ClearToken(ref _cts);
        _cts = new();

        _cameraNoise.AmplitudeGain = _shakeAmplitude;
        _cameraNoise.FrequencyGain = _shakeFrequency;

        try
        {
            await UniTask.WaitForSeconds(duration ?? _defaultShakeDuration, cancellationToken: _cts.Token);
        }
        catch (System.OperationCanceledException)
        {
        }

        StopShake();
    }

    public UniTask StartShake()
    {
        if (_cameraNoise == null)
        {
            return UniTask.FromException(new System.Exception("Camera noise component not found."));
        }

        ClearToken(ref _cts);
        _cts = new();

        _cameraNoise.AmplitudeGain = _shakeAmplitude;
        _cameraNoise.FrequencyGain = _shakeFrequency;

        return UniTask.CompletedTask;
    }

    public void StopShake()
    {
        if (_cameraNoise == null) return;

        _cameraNoise.AmplitudeGain = 0f;
        _cameraNoise.FrequencyGain = 0f;

        ClearToken(ref _cts);
    }
    private void ClearToken(ref CancellationTokenSource cts) => ClearTokenSupport.ClearToken(ref cts);

    public void Dispose()
    {
        ClearToken(ref _cts);
    }

}
