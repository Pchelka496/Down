using UnityEngine;

namespace Creatures.Player.FlightModule.EngineModule
{
    [System.Serializable]
    public class EngineModuleVisualPart
    {
        const float PARTICLES_START_SPEED_MIN = 1f;
        const float PARTICLES_START_SPEED_MAX = 15f;

        [SerializeField] ParticleSystem _defaultParticleSystem;
        [SerializeField] ParticleSystem _boostParticleSystem;

        public void Boost() => _boostParticleSystem.Play();

        public void UpdateDefaultWork(float maxForce, float currentForce, float forceApplyRate)
        {
            var main = _defaultParticleSystem.main;
            var emission = _defaultParticleSystem.emission;

            var rateMultiplier = 1f / forceApplyRate;
            emission.rateOverTime = rateMultiplier;

            main.startSpeed = Mathf.Lerp(PARTICLES_START_SPEED_MIN, PARTICLES_START_SPEED_MAX, currentForce / maxForce);

            if (currentForce > 0 && !_defaultParticleSystem.isPlaying)
            {
                _defaultParticleSystem.Play();
            }
            else if (currentForce <= 0 && _defaultParticleSystem.isPlaying)
            {
                _defaultParticleSystem.Stop();
            }
        }
    }
}