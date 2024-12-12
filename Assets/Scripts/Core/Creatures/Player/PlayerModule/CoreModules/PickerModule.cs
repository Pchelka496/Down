using Core.Installers;
using Creatures.Player;
using ScriptableObject.ModulesConfig.SupportModules;
using UnityEngine;
using Zenject;

public class PickerModule : BaseModule
{
    [SerializeField] CircleCollider2D _circleCollider;

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:������� �������������� �������� �����", Justification = "<��������>")]
    private void Construct(PlayerController player, PickerModuleConfig config, RewardCounter rewardCounter)
    {
        SnapToPlayer(player.transform);

        UpdateCharacteristics(config, rewardCounter);
    }

    public void UpdateCharacteristics()
    {
        UpdateCharacteristics(GameplaySceneInstaller.DiContainer.Resolve<PickerModuleConfig>(),
                              GameplaySceneInstaller.DiContainer.Resolve<RewardCounter>()
                             );
    }

    public void UpdateCharacteristics(PickerModuleConfig config, RewardCounter rewardCounter)
    {
        _circleCollider.radius = config.PickUpRadius;

        var rewardMultiplier = config.PickUpRewardMultiplier;

        if (rewardMultiplier < 1)
        {
            Debug.LogWarning($"rewardMultiplier < 1, rewardMultiplier = {rewardMultiplier}");
            rewardMultiplier = 1;
        }

        rewardCounter.PickUpRewardMultiplier = rewardMultiplier;
    }

    public override void EnableModule()
    {
        _circleCollider.enabled = true;
    }

    public override void DisableModule()
    {
        _circleCollider.enabled = false;
    }

}
