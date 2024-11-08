using UnityEngine;
using Zenject;

public class PickerModule : BaseModule
{
    [SerializeField] CircleCollider2D _circleCollider;
    PickerModuleConfig _config;

    [Inject]
    private void Construct(CharacterController player, PickerModuleConfig config)
    {
        SnapToPlayer(player.transform);

        _config = config;
    }

}
