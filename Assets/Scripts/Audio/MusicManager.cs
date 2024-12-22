using System;
using System.Threading;
using Additional;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class MusicManager : IDisposable
{
    readonly AudioSource _audioSource;
    IAudioSettingContainer _audioSettingContainer;

    readonly AudioClip[] _gameplayMusic;
    readonly AudioClip[] _lobbyMusic;
    readonly AudioClip[] _warpMusic;

    AudioClip[] _currentMusic;

    CancellationTokenSource _cts;

    event Action SubscribeToGlobalEventsManagerEvents;
    event Action UnsubscribeFromGlobalEventsManagerEvents;
    event Action DisposeEvents;

    public MusicManager(Initializer initializer)
    {
        _audioSource = initializer.AudioSource;
        _gameplayMusic = initializer.GameplayMusic;
        _lobbyMusic = initializer.LobbyMusic;
        _warpMusic = initializer.FastTravelMusic;
    }

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(GlobalEventsManager globalEventsManager, IAudioSettingContainer audioSettingContainer)
    {
        _audioSettingContainer = audioSettingContainer;

        SubscribeToGlobalEventsManagerEvents += () => globalEventsManager?.SubscribeToRoundStarted(RoundStart);
        SubscribeToGlobalEventsManagerEvents += () => globalEventsManager?.SubscribeToRoundEnded(RoundEnd);
        SubscribeToGlobalEventsManagerEvents += () => globalEventsManager?.SubscribeToFastTravelStarted(FastTravelStart);

        UnsubscribeFromGlobalEventsManagerEvents += () => globalEventsManager?.UnsubscribeFromFastTravelStarted(FastTravelStart);
        UnsubscribeFromGlobalEventsManagerEvents += () => globalEventsManager?.UnsubscribeFromRoundStarted(RoundStart);
        UnsubscribeFromGlobalEventsManagerEvents += () => globalEventsManager?.UnsubscribeFromRoundEnded(RoundEnd);

        audioSettingContainer.SubscribeToChangeSoundEnableEvent(AudioEnabledFlagProcessing);
        audioSettingContainer.SubscribeToChangeMaxVolumeEvent(UpdateMusicVolume);
        audioSettingContainer.SubscribeToChangeMusicVolumeEvent(UpdateMusicVolume);

        DisposeEvents += () => UnsubscribeFromGlobalEventsManagerEvents?.Invoke();
        DisposeEvents += () => audioSettingContainer?.UnsubscribeFromChangeSoundEnableEvent(AudioEnabledFlagProcessing);
        DisposeEvents += () => audioSettingContainer?.UnsubscribeFromChangeMaxVolumeEvent(UpdateMusicVolume);
        DisposeEvents += () => audioSettingContainer?.UnsubscribeFromChangeMusicVolumeEvent(UpdateMusicVolume);

        AudioEnabledFlagProcessing(audioSettingContainer.SoundEnabledFlag);
        RoundEnd();
    }

    private void UpdateMusicVolume(float value)
    {
        _audioSource.volume = _audioSettingContainer.MusicVolume;
    }

    private void AudioEnabledFlagProcessing(bool flag)
    {
        if (flag)
        {
            UnsubscribeFromGlobalEventsManagerEvents?.Invoke();
            SubscribeToGlobalEventsManagerEvents?.Invoke();

            if (_currentMusic != null)
            {
                ClearToken();
                _cts = new();

                PlayMusicArrayAsync(_currentMusic, _cts.Token).Forget();
            }
        }
        else
        {
            UnsubscribeFromGlobalEventsManagerEvents?.Invoke();
            ClearToken();
            _audioSource.Stop();
        }
    }

    private void FastTravelStart()
    {
        ClearToken();
        _cts = new();

        PlayMusicArrayAsync(_warpMusic, _cts.Token).Forget();
    }

    private void RoundStart()
    {
        ClearToken();
        _cts = new();

        PlayMusicArrayAsync(_gameplayMusic, _cts.Token).Forget();
    }

    private void RoundEnd()
    {
        ClearToken();
        _cts = new();

        PlayMusicArrayAsync(_lobbyMusic, _cts.Token).Forget();
    }

    private async UniTaskVoid PlayMusicArrayAsync(AudioClip[] musicArray, CancellationToken token)
    {
        if (musicArray == null || musicArray.Length == 0) return;

        _currentMusic = musicArray;
        _audioSource.volume = _audioSettingContainer.MusicVolume;

        if (musicArray.Length == 1)
        {
            _audioSource.loop = true;
            _audioSource.clip = musicArray[0];
            _audioSource.Play();

            return;
        }

        foreach (var clip in musicArray)
        {
            if (token.IsCancellationRequested) return;

            _audioSource.clip = clip;
            _audioSource.Play();

            await UniTask.Delay(System.TimeSpan.FromSeconds(clip.length), cancellationToken: token);
        }

        PlayMusicArrayAsync(musicArray, token).Forget();
    }

    private void ClearToken() => ClearTokenSupport.ClearToken(ref _cts);

    public void Dispose()
    {
        ClearToken();
        DisposeEvents?.Invoke();
    }

    [System.Serializable]
    public record Initializer
    {
        [field: SerializeField] public AudioSource AudioSource { get; set; }
        [field: SerializeField] public AudioClip[] GameplayMusic { get; set; }
        [field: SerializeField] public AudioClip[] LobbyMusic { get; set; }
        [field: SerializeField] public AudioClip[] FastTravelMusic { get; set; }
    }
}
