using UnityEngine;

[System.Serializable]
public class FastTravelVisualPart
{
    [SerializeField] ParticleSystem _mainTravelEffectParticle;
    [SerializeField] ParticleSystem _preparationParticle;
    [SerializeField] Transform _particleSystemParent;

    public void Initialize()
    {
        _mainTravelEffectParticle.Stop();

        var main = _mainTravelEffectParticle.main;
        main.loop = false;

        _mainTravelEffectParticle.transform.SetParent(_particleSystemParent);
        _mainTravelEffectParticle.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
    }


    public void UpdateMoveDuration(float duration)
    {
        _mainTravelEffectParticle.Stop();

        var main = _mainTravelEffectParticle.main;
        main.duration = duration;
    }

    public void PlayMoving()
    {
        _mainTravelEffectParticle.Play();
    }

    public void PlayerPreparation()
    {
        _preparationParticle.Play();
    }
}
