using System.Threading;
using UnityEngine;

[System.Serializable]
public class SoundPlayerRandomPitch
{
    [SerializeField] AudioClip[] _sounds;
    [SerializeField][Range(0f, 3f)] float _minPitch = 0.8f;
    [SerializeField][Range(0f, 3f)] float _maxPitch = 1.2f;
    [SerializeField][Range(0f, 1f)] float _volume = 0.7f;
    AudioSourcePool _audioSourcePool;
    int _currentSoundIndex = 0;

    public void Initialize(AudioSourcePool audioSourcePool)
    {
        _audioSourcePool = audioSourcePool;
    }

    public AudioClip PlayNextSound(float? volume = null)
    {
        if (_sounds.Length == 0) return default;

        (var sound, var pitch) = GetSoundAndPitch();

        _audioSourcePool.PlaySound(sound, volume ?? _volume, pitch);

        return sound;
    }

    public void PlayNextSound(CancellationToken token, float? volume = null)
    {
        if (_sounds.Length == 0) return;

        (var sound, var pitch) = GetSoundAndPitch();

        _audioSourcePool.PlaySound(sound, token, volume ?? _volume, pitch);
    }

    private (AudioClip sound, float pitch) GetSoundAndPitch()
    {
        var sound = _sounds[_currentSoundIndex];
        _currentSoundIndex = (_currentSoundIndex + 1) % _sounds.Length;

        var pitch = Random.Range(_minPitch, _maxPitch);

        return (sound, pitch);
    }

    public void PlaySound(int index)
    {
        if (index > _sounds.Length - 1) return;

        var sound = _sounds[index];

        var pitch = Random.Range(_minPitch, _maxPitch);

        _audioSourcePool.PlaySound(sound, _volume, pitch);
    }
}
