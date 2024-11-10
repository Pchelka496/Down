using UnityEngine;
using Zenject;
using Cysharp.Threading.Tasks;
using System.Threading;
using System;

public class HealthModule : BaseModule
{
    [SerializeField] float _cameraShakeTime = 0.3f;
    [SerializeField] float _invulnerabilityDuration = 0.5f;
    [SerializeField] SoundPlayerRandomPitch _soundPlayer;
    [SerializeField] int _maxHealth;
    [SerializeField] int _currentHealth;

    AudioSourcePool _audioSourcePool;
    HealthModuleConfig _config;
    EffectController _effectController;
    CamerasController _camerasController;

    // bool _isInvulnerable = false;
    // CancellationTokenSource _invulnerabilityCancellationTokenSource;

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
        _maxHealth = 1;//_config.GetMaxHealth();
        _currentHealth = _maxHealth;
        // _isInvulnerable = false;

        // _invulnerabilityCancellationTokenSource?.Cancel();
        //  _invulnerabilityCancellationTokenSource = null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == EnemyManager.ENEMY_LAYER_INDEX)
        {
            ApplyDamage(collision.contacts[0].point);
        }
    }

    public void ApplyDamage(Vector2 point)
    {
        // if (_isInvulnerable) return;

        _currentHealth--;

        _effectController.PlayImpactEffect(point);
        _camerasController.EnableCameraShake(_cameraShakeTime);
        _soundPlayer.PlayNextSound();

        if (_currentHealth < 0)
        {
            GameplaySceneInstaller.DiContainer.Resolve<LevelManager>().RoundEnd().Forget();
        }
        // else
        //  {
        //      StartInvulnerability();
        // }
    }

    //private async void StartInvulnerability()
    //{
    //    _isInvulnerable = true;
    //    _invulnerabilityCancellationTokenSource = new CancellationTokenSource();
    //    try
    //    {
    //        await UniTask.Delay((int)(_invulnerabilityDuration * 1000), cancellationToken: _invulnerabilityCancellationTokenSource.Token);
    //    }
    //    catch (OperationCanceledException)
    //    {
    //    }
    //    finally
    //    {
    //        _isInvulnerable = false;
    //        _invulnerabilityCancellationTokenSource = null;
    //    }
    //}

}

