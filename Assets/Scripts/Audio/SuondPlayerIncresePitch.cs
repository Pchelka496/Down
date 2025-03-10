using Additional;
using Cysharp.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading;
using UnityEngine;

[System.Serializable]
public class SoundPlayerIncreasePitch : System.IDisposable
{
    [SerializeField][Range(0f, 3f)] float _minPitch = 0.8f;
    [SerializeField][Range(0f, 3f)] float _maxPitch = 1.2f;
    [SerializeField][Range(0f, 1f)] float _volume = 0.7f;
    [SerializeField] AudioClip _audioClip;

    [SerializeField][Range(0.1f, 10f)] float _resetTime = 3f;
    [SerializeField] int _playingForMaxPitch = 5;

    AudioSourcePool _audioSourcePool;
    int _coinsCollectedInARow = 0;
    float _currentPitch = 0;

    CancellationTokenSource _resetPitchCts;

    event System.Action OnPitchReset;

    public void Initialize(AudioSourcePool audioSourcePool)
    {
        _audioSourcePool = audioSourcePool;
    }

    public void PlaySound()
    {
        _coinsCollectedInARow++;
        UpdatePitch();

        _audioSourcePool.PlaySound(_audioClip, _volume, _currentPitch);

        ResetPitchAfterDelay().Forget();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void UpdatePitch()
    {
        var pitchProgress = Mathf.Clamp01((float)_coinsCollectedInARow / _playingForMaxPitch);
        _currentPitch = Mathf.Lerp(_minPitch, _maxPitch, pitchProgress);
    }

    private async UniTaskVoid ResetPitchAfterDelay()
    {
        _resetPitchCts?.Cancel();
        _resetPitchCts = new CancellationTokenSource();

        await UniTask.WaitForSeconds(_resetTime, cancellationToken: _resetPitchCts.Token);
        _coinsCollectedInARow = 0;

        OnPitchReset?.Invoke();
    }

    public void SubscribeToOnPitchReset(System.Action callback) => OnPitchReset += callback;
    public void UnsubscribeToOnPitchReset(System.Action callback) => OnPitchReset -= callback;

    private void ClearToken(ref CancellationTokenSource cts) => ClearTokenSupport.ClearToken(ref cts);

    public void Dispose()
    {
        ClearToken(ref _resetPitchCts);
    }
}
