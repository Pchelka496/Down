using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Runtime.CompilerServices;

public class AudioSourcePool
{
    const int INITIAL_POOL_SIZE = 5;

    readonly AudioSource _audioSourcePrefab;
    readonly Transform _transform;
    readonly Queue<AudioSource> _sourcesPool = new();

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

    private AudioSource AudioSourcePrefab()
    {
        var newGameObject = MonoBehaviour.Instantiate(new GameObject("AudioSource"), _transform);
        var audioSourcePrefab = newGameObject.AddComponent<AudioSource>();

        return audioSourcePrefab;
    }

    private AudioSource CreateNewAudioSource()
    {
        AudioSource newSource = MonoBehaviour.Instantiate(_audioSourcePrefab, _transform);
        return newSource;
    }

    public void PlaySound(AudioClip clip, float volume = 1.0f, float pitch = 1f)
    {
        AudioSource source = GetAudioSource();
        source.clip = clip;
        source.volume = volume;
        source.pitch = pitch;
        source.Play();
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

        WaitingEnd(audioSource).Forget();

        return audioSource;
    }

    private async UniTask WaitingEnd(AudioSource audioSource)
    {
        await UniTask.WaitUntil(() => !audioSource.isPlaying);
        ReturnOnPool(audioSource);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReturnOnPool(AudioSource audioSource) => _sourcesPool.Enqueue(audioSource);

}
