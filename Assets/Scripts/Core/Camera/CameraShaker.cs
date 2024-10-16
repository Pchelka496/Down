using UnityEngine;
using Cysharp.Threading.Tasks;
using Unity.Cinemachine;
using System.Threading;

[System.Serializable]
public class CameraShaker
{
    [SerializeField] CinemachineBasicMultiChannelPerlin _cameraNoise;
    [SerializeField] float _defaultShakeDuration = 0.5f;
    [SerializeField] float _shakeAmplitude = 2f;
    [SerializeField] float _shakeFrequency = 2f;

    CancellationTokenSource _cts;

    private void Start()
    {
        if (_cameraNoise == null)
        {
            Debug.LogError("CinemachineBasicMultiChannelPerlin == null");
        }
    }

    public async UniTask StartShake(float? duration = null)
    {
        if (_cameraNoise == null) return;

        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        _cameraNoise.AmplitudeGain = _shakeAmplitude;
        _cameraNoise.FrequencyGain = _shakeFrequency;

        await UniTask.WaitForSeconds(duration ?? _defaultShakeDuration, cancellationToken: _cts.Token);

        StopShake();
    }

    public UniTask StartShake()
    {
        if (_cameraNoise == null)
        {
            return UniTask.FromException(new System.Exception("Camera noise component not found."));
        }

        _cts?.Cancel();
        _cts = new CancellationTokenSource();

        _cameraNoise.AmplitudeGain = _shakeAmplitude;
        _cameraNoise.FrequencyGain = _shakeFrequency;

        return UniTask.CompletedTask;
    }

    public void StopShake()
    {
        if (_cameraNoise == null) return;

        _cameraNoise.AmplitudeGain = 0f;
        _cameraNoise.FrequencyGain = 0f;

        _cts?.Cancel();
    }

}
