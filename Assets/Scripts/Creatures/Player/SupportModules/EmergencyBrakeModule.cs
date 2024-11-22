using UnityEngine;
using Zenject;

public class EmergencyBrakeModule : BaseModule
{
    const float Z_ROTATION_OFFSET = 90f;
    public const float COLLIDER_RADIUS = 0.8f;
    [SerializeField] Collider2D _collider;
    [SerializeField] ParticleSystem _particleSystem;

    float _stopRate;

    Rigidbody2D _rb;
    ChargeSystem _chargeSystem;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Удалите неиспользуемые закрытые члены", Justification = "<Ожидание>")]
    private void Construct(EmergencyBrakeModuleConfig config, CharacterController player)
    {
        _rb = player.Rb;

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

        var main = _particleSystem.main;

        main.duration = _stopRate;

        _collider.offset = new(_collider.offset.x, moduleConfig.SensingDistance);
    }

    public override void EnableModule()
    {
        _collider.enabled = true;
    }

    public override void DisableModule()
    {
        _collider.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.isTrigger || !_chargeSystem.HasCharge) return;

        ActivateEmergencyBrake();
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
    }

}

