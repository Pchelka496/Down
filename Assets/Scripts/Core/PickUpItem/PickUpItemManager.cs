using ScriptableObject.PickUpItem;
using UnityEngine;
using Zenject;

public class PickUpItemManager : System.IDisposable
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
    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:", Justification = "<>")]
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

    public void Dispose()
    {
        _rewardController.Dispose();
        _repairKitController.Dispose();
    }
}
