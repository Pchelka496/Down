using UnityEngine;

[System.Serializable]
public class WarpEngineVisualPart
{
    [SerializeField] ParticleSystem _warpEffectParticle;
    [SerializeField] Transform _particleSystemParent;

    public void Initialize()
    {
        _warpEffectParticle.Stop();

        var main = _warpEffectParticle.main;
        main.loop = false;

        _warpEffectParticle.transform.SetParent(_particleSystemParent);
        _warpEffectParticle.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    public void UpdateMoveDuration(float duration)
    {
        _warpEffectParticle.Stop();

        var main = _warpEffectParticle.main;
        main.duration = duration;
    }

    public void Play()
    {
        _warpEffectParticle.Play();
    }
}
