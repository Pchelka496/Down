using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Runtime.CompilerServices;

public class AudioSourcePool : MonoBehaviour
{
    [Tooltip("value + 1 Prefab = initialPoolSize")]
    [SerializeField] int initialPoolSize = 5;
    AudioSource _audioSourcePrefab;

    Queue<AudioSource> _sourcesPool = new();

    private void Start()
    {
        _audioSourcePrefab = AudioSourcePrefab();
        _sourcesPool.Enqueue(_audioSourcePrefab);

        for (int i = 0; i < initialPoolSize; i++)
        {
            CreateNewAudioSource();
        }
    }

    private AudioSource AudioSourcePrefab()
    {
        var newGameObject = Instantiate(new GameObject("AudioSource"), transform);
        var audioSourcePrefab = newGameObject.AddComponent<AudioSource>();

        return audioSourcePrefab;
    }

    private AudioSource CreateNewAudioSource()
    {
        AudioSource newSource = Instantiate(_audioSourcePrefab, transform);
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
        AudioSource audioSource = null;

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
