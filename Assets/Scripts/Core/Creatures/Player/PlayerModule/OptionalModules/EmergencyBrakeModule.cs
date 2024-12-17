using Core.Installers;
using Creatures.Player;
using Cysharp.Threading.Tasks;
using ScriptableObject.ModulesConfig.SupportModules;
using System.Threading;
using UnityEngine;
using Zenject;

public class EmergencyBrakeModule : BaseModule
{
    const float Z_ROTATION_OFFSET = 90f;
    const float COOLDOWN = 1f;
    public const float COLLIDER_RADIUS = PlayerController.PLAYER_RADIUS + 0.1f;

    [SerializeField] Collider2D _collider;
    [SerializeField] ParticleSystem _particleSystem;

    float _stopRate;
    bool _cooldownStatus = true;

    Rigidbody2D _rb;
    ChargeSystem _chargeSystem;
    EmergencyBrakeModuleIndicator _emergencyBrakeModuleIndicator;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
    private void Construct(EmergencyBrakeModuleConfig config, PlayerController player, EmergencyBrakeModuleIndicator emergencyBrakeModuleIndicator)
    {
        _rb = player.Rb;
        _emergencyBrakeModuleIndicator = emergencyBrakeModuleIndicator;

        SnapToPlayer(player.transform);
        player.MultiTargetRotationFollower.RegisterRotationObject(transform, Z_ROTATION_OFFSET);

        UpdateCharacteristics(config);
    }

    public void UpdateCharacteristics() => UpdateCharacteristics(GameplaySceneInstaller.DiContainer.Resolve<EmergencyBrakeModuleConfig>());

    public void UpdateCharacteristics(EmergencyBrakeModuleConfig moduleConfig)
    {
        _stopRate = moduleConfig.StopRate;

        _chargeSystem = new();
        _chargeSystem.Initialize(moduleConfig.MaxCharges, moduleConfig.ChargeCooldown);

        _emergencyBrakeModuleIndicator.UpdateMaxChargeAmount(moduleConfig.MaxCharges).Forget();
        _chargeSystem.SubscribeToChargeChange(_emergencyBrakeModuleIndicator.UpdateCurrentChargeAmount);

        var main = _particleSystem.main;

        main.duration = _stopRate;

        _collider.offset = new(_collider.offset.x, moduleConfig.SensingDistance);
    }

    public override void EnableModule()
    {
        _collider.enabled = true;
        _cooldownStatus = true;
    }

    public override void DisableModule()
    {
        _collider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.isTrigger || !_chargeSystem.HasCharge) return;

        if (_cooldownStatus)
        {
            ActivateEmergencyBrake();
            _cooldownStatus = false;
            Reload().Forget();
        }
    }

    private async UniTaskVoid Reload()
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(COOLDOWN));
        _cooldownStatus = true;
    }

    private void ActivateEmergencyBrake()
    {
        _chargeSystem.UseCharge();

        _particleSystem.Play();

        _rb.velocity = Vector2.zero;
    }

    private void OnDestroy()
    {
        _chargeSystem.Dispose();
        _chargeSystem.UnsubscribeFromChargeChange(_emergencyBrakeModuleIndicator.UpdateCurrentChargeAmount);
    }
}
