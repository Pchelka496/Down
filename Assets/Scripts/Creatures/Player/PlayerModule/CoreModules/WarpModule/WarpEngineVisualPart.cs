using UnityEngine;

[System.Serializable]
public class WarpEngineVisualPart
{
    [SerializeField] ParticleSystem _warpEffectParticle;
    [SerializeField] Transform _particleSystemParent;

    public void Initialize(float duration)
    {
        var main = _warpEffectParticle.main;
        main.duration = duration;
        main.loop = false;

        _warpEffectParticle.transform.SetParent(_particleSystemParent);
        _warpEffectParticle.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }

    public void Play()
    {
        _warpEffectParticle.Play();
    }

}
