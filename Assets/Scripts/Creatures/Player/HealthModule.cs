using UnityEngine;
using Zenject;

public class HealthModule : BaseModule
{
    [SerializeField] float _cameraShakeTime = 0.4f;
    [SerializeField] SoundPlayerRandomPitch _soundPlayer;

    [SerializeField] int _maxHealth;
    [SerializeField] int _currentHealth;

    [SerializeField] int _repairKitNumberForRepair;
    [SerializeField] int _currentRepairKit;
    bool _workFlag = true;
    Vector2 _previousVelocity;

    Rigidbody2D _rb;
    RepairKitIndicator _repairKitIndicator;
    HealthIndicator _healthIndicator;

    EffectController _effectController;
    CamerasController _camerasController;

    private int MaxHealth
    {
        set
        {
            _maxHealth = value;
            _healthIndicator.Initialize(_maxHealth, _currentHealth);
        }
    }

    private int CurrentHealth
    {
        set
        {
            _currentHealth = value;
            _healthIndicator.UpdateHealth(_currentHealth);

            if (_currentHealth < 0)
            {
                GameplaySceneInstaller.DiContainer.Resolve<LevelManager>().RoundEnd().Forget();
            }
        }
    }

    private int CurrentRepairKit
    {
        set
        {
            _currentRepairKit = value;
            _repairKitIndicator.UpdateCurrentRepairKit(_currentRepairKit);
        }
    }

    private int RepairKitNumberForRepair
    {
        set
        {
            _repairKitNumberForRepair = value;
            _repairKitIndicator.Initialize(_repairKitNumberForRepair);
        }
    }

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Удалите неиспользуемые закрытые члены", Justification = "<Ожидание>")]
    private void Construct(HealthModuleConfig config,
                           EffectController effectController,
                           CamerasController camerasController,
                           AudioSourcePool audioSourcePool,
                           RepairKitIndicator repairKitIndicator,
                           HealthIndicator healthIndicator,
                           CharacterController player,
                           LevelManager levelManager
                           )
    {
        _effectController = effectController;
        _camerasController = camerasController;
        _soundPlayer.Initialize(audioSourcePool);
        _repairKitIndicator = repairKitIndicator;
        _healthIndicator = healthIndicator;
        _rb = player.Rb;

        levelManager.SubscribeToRoundStart(RoundStart);

        UpdateCharacteristics(config);
    }

    private void RoundStart(LevelManager levelManager)
    {
        levelManager.SubscribeToRoundEnd(RoundEnd);
        UpdateCharacteristics();
    }

    private void RoundEnd(LevelManager levelManager, EnumRoundResults results)
    {
        levelManager.SubscribeToRoundStart(RoundStart);
    }

    public void UpdateCharacteristics() => UpdateCharacteristics(GameplaySceneInstaller.DiContainer.Resolve<HealthModuleConfig>());

    public void UpdateCharacteristics(HealthModuleConfig config)
    {
        MaxHealth = config.MaximumHealth;
        CurrentHealth = _maxHealth;

        RepairKitNumberForRepair = config.RepairKitNumberForRepair;
        CurrentRepairKit = 0;
    }

    public override void EnableModule()
    {
        _workFlag = true;
    }

    public override void DisableModule()
    {
        _workFlag = false;
    }

    private void FixedUpdate()
    {
        _previousVelocity = _rb.velocity;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!_workFlag) return;

        if (collision.gameObject.layer == EnemyManager.ENEMY_LAYER_INDEX)
        {
            ApplyDamage(collision.contacts[0].point);
        }
    }

    private void ApplyDamage(Vector2 point)
    {
        CurrentHealth = _currentHealth - 1 - (int)_previousVelocity.magnitude;

        _effectController.PlayImpactEffect(point);
        _camerasController.EnableCameraShake(_cameraShakeTime);
        _soundPlayer.PlayNextSound();
    }

    public void TestDealDamage(int damage)
    {
        CurrentHealth = System.Math.Clamp(_currentHealth - damage, 1, _maxHealth);
    }

    public void ApplyRepairKit()
    {
        CurrentRepairKit = _currentRepairKit + 1;

        if (_currentRepairKit >= _repairKitNumberForRepair)
        {
            HealthRegeneration();
        }
    }

    private void HealthRegeneration()
    {
        CurrentRepairKit = 0;
        CurrentHealth = _maxHealth;
    }

}

