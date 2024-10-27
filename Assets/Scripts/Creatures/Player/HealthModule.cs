using UnityEngine;
using Zenject;

public class HealthModule : BaseModule
{
    [SerializeField] float _cameraShakeTime = 0.3f;
    [SerializeField] SoundPlayerRandomPitch _soundPlayer;
    [SerializeField] int _maxHealth;
    [SerializeField] int _currentHealth;

    AudioSourcePool _audioSourcePool;
    HealthModuleConfig _config;
    EffectController _effectController;
    CamerasController _camerasController;

    [Inject]
    private void Construct(HealthModuleConfig config, EffectController effectController, CamerasController camerasController, LevelManager levelManager, AudioSourcePool audioSourcePool)
    {
        _effectController = effectController;
        _camerasController = camerasController;
        _audioSourcePool = audioSourcePool;
        _config = config;
        _soundPlayer.Initialize(audioSourcePool);

        levelManager.SubscribeToRoundStart(RoundStart);
    }

    private void RoundStart(LevelManager levelManager)
    {
        _maxHealth = _config.GetMaxHealth();

        _currentHealth = _maxHealth;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        _effectController.PlayImpactEffect(collision.contacts[0].point);
        _camerasController.EnableCameraShake(_cameraShakeTime);
        _soundPlayer.PlayNextSound();

        if (collision.gameObject.layer == EnemyManager.ENEMY_LAYER_INDEX)
        {
            ApplyDamage();
        }
    }

    public void ApplyDamage()
    {
        _currentHealth--;

        if (_currentHealth < 0)
        {
            GameplaySceneInstaller.DiContainer.Resolve<LevelManager>().RoundEnd().Forget();
        }
    }

}
