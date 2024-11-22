using UnityEngine;
using Zenject;

public class PickUpItemManager
{
    public const int REWARD_LAYER_INDEX = 11;
    readonly Transform _transform;

    PickUpItemManagerConfig _config;
    RewardController _rewardController;
    RepairKitController _repairKitController;

    public PickUpItemManager(Transform transform)
    {
        _transform = transform;
    }

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Удалите неиспользуемые закрытые члены", Justification = "<Ожидание>")]
    private void Construct(CharacterController player, RewardCounter rewardCounter)
    {
        Reward.Initialize(player, rewardCounter);

        _rewardController = GameplaySceneInstaller.DiContainer.Instantiate<RewardController>();
        _repairKitController = GameplaySceneInstaller.DiContainer.Instantiate<RepairKitController>();
    }

    public void Initialize(PickUpItemManagerConfig config)
    {
        _config = config;
        _rewardController.Initialize(config.RewardControllerConfig, _transform);
        _repairKitController.Initialize(config.RepairKitControllerConfig, _transform);
    }

    public int GetPoints() => _config.GetPoints();

    public void IncreasePoints(int increaseValue) => _config.IncreasePoints(increaseValue);

    public void DecreasePoints(int decreaseValue) => _config.DecreasePoints(decreaseValue);

}
