using UnityEngine;
using Zenject;

public class EmergencyBrakeModule : BaseModule
{
    const float Z_ROTATION_OFFSET = 90f;
    [SerializeField] Collider2D _collider;
    EmergencyBrakeModuleConfig _config;
    Rigidbody2D _rb;

    [Inject]
    private void Construct(EmergencyBrakeModuleConfig config, CharacterController player)
    {
        player.EmergencyBrakeModule = this;

        _config = config;
        _rb = player.Rb;
        SnapToPlayer(player.transform);
        player.MultiTargetRotationFollower.RegisterRotationObject(transform, Z_ROTATION_OFFSET);
    }

    public void EnableModule()
    {
        gameObject.SetActive(true);
    }

    public void DisableModule()
    {
        gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        _rb.velocity = Vector2.zero;
    }

}
