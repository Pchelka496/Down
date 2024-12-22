using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Threading;

public class AudioSourcePool : System.IDisposable
{
    const int INITIAL_POOL_SIZE = 5;

    readonly AudioSource _audioSourcePrefab;
    readonly Transform _transform;
    readonly Queue<AudioSource> _sourcesPool = new();

    IAudioSettingContainer _audioSettingContainer;

    public AudioSourcePool(Transform transform)
    {
        _transform = transform;

        _audioSourcePrefab = AudioSourcePrefab();
        _sourcesPool.Enqueue(_audioSourcePrefab);

        for (int i = 0; i < INITIAL_POOL_SIZE; i++)
        {
            CreateNewAudioSource();
        }
    }

    [Zenject.Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(IAudioSettingContainer audioSettingContainer)
    {
        _audioSettingContainer = audioSettingContainer;
    }

    private AudioSource AudioSourcePrefab()
    {
        var newGameObject = MonoBehaviour.Instantiate(new GameObject("AudioSource"), _transform);
        var audioSourcePrefab = newGameObject.AddComponent<AudioSource>();

        return audioSourcePrefab;
    }

    private AudioSource CreateNewAudioSource()
    {
        var newSource = MonoBehaviour.Instantiate(_audioSourcePrefab, _transform);
        return newSource;
    }

    public void PlaySound(AudioClip clip, float volume = 1.0f, float pitch = 1f)
    {
        if (!_audioSettingContainer.SoundEnabledFlag) return;

        var source = GetAudioSource();

        source.clip = clip;
        source.volume = UnityEngine.Mathf.Clamp(volume, 0f, _audioSettingContainer.MaxVolume);
        source.pitch = pitch;

        WaitingEnd(source).Forget();
        source.Play();
    }

    public void PlaySound(AudioClip clip, CancellationToken token, float volume = 1.0f, float pitch = 1f)
    {
        if (!_audioSettingContainer.SoundEnabledFlag) return;

        var source = GetAudioSource();

        source.clip = clip;
        source.volume = UnityEngine.Mathf.Clamp(volume, 0f, _audioSettingContainer.MaxVolume);
        source.pitch = pitch;

        PlayWithCancellation(source, token).Forget();
    }

    private AudioSource GetAudioSource()
    {
        AudioSource audioSource;

        if (_sourcesPool.Count > 0)
        {
            audioSource = _sourcesPool.Dequeue();
        }
        else
        {
            audioSource = CreateNewAudioSource();
        }

        return audioSource;
    }

    private async UniTask PlayWithCancellation(AudioSource audioSource, CancellationToken token)
    {
        try
        {
            audioSource.Play();

            await UniTask.WaitUntil(() => !audioSource.isPlaying, cancellationToken: token);

            if (token.IsCancellationRequested && audioSource.isPlaying)
            {
                audioSource.Stop();
            }
        }
        finally
        {
            ReturnOnPool(audioSource);
        }
    }

    private async UniTask WaitingEnd(AudioSource audioSource)
    {
        await UniTask.WaitUntil(() => !audioSource.isPlaying);
        ReturnOnPool(audioSource);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReturnOnPool(AudioSource audioSource) => _sourcesPool.Enqueue(audioSource);

    public void Dispose()
    {
        throw new System.NotImplementedException();
    }
}
