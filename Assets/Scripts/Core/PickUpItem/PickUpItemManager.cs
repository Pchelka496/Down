using UnityEngine;
using Zenject;

public class PickUpItemManager
{
    public const int REWARD_LAYER_INDEX = 11;
    readonly Transform _transform;

    RewardSpawner _rewardController;
    RepairItemSpawner _repairKitController;

    public PickUpItemManager(Transform transform)
    {
        _transform = transform;
    }

    [Inject]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Удалите неиспользуемые закрытые члены", Justification = "<Ожидание>")]
    private void Construct(RewardCounter rewardCounter, DiContainer diContainer)
    {
        PickUpReward.Initialize(rewardCounter);

        _rewardController = diContainer.Instantiate<RewardSpawner>();
        _repairKitController = diContainer.Instantiate<RepairItemSpawner>();
    }

    public void Initialize(PickUpItemManagerConfig config)
    {
        _rewardController.Initialize(config.RewardControllerConfig, _transform);
        _repairKitController.Initialize(config.RepairKitControllerConfig, _transform);
    }

}
